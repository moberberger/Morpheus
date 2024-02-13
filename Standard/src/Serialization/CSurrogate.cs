using System.Reflection;
using System.Xml;
using Morpheus.Serialization;

#nullable disable

namespace Morpheus;


/// <summary>
/// This class defines information about an implicit surrogate. This wraps up Implicit surrogates- External surrogates are controlled
/// on a "contextual" basis, while Implicit surrogates are always used.
/// </summary>
/// <remarks>
/// Information on Surrogates:
/// 
/// Implicit Surrogates are essentially function pointers to methods in a class responsible for serialization and deserialization
/// for that class. It was a difficult decision to use reflection to find implicit surrogates. Because there
/// is no way to specify both constructors AND static methods using interfaces (the prefered mechanism), the
/// Deserialization process would have been subject to Reflection regardless of the use of an interface for the
/// Serialization aspect. Because of this dichotomy, I chose to use a similar mechanism for BOTH serialization AND
/// Deserialization- the alternative would have been to use an interface for one and reflection for the other, and
/// i really dislike that option.
/// 
/// All implicit surrogates must follow this calling convention:
/// 
///             Serializer: bool = fn( XmlNode [, CSerializer] )
///                         -- Serializer is an INSTANCE Method
///           Return Value: a "bool" value telling the framework whether or not the surrogate "completed" the job. 
///                             A "FALSE" return value tells the framework to continue processing the object 
///                             as if there were no surrogate. This can be used in conjunction with the <see cref="ADoNotSerialize"/>
///                             attribute to do custom serialization on those fields that need special consideration, while 
///                             letting the Framework do all of the "standard" work.
///               XmlNode : Contains the "parent" XmlNode (XmlElement) into which this surrogate is to add data.
///           CSerializer : Contains the Serialization Context that MAY GIVE HINTS to the surrogate, but the surrogate can do whatever it wants
/// 
/// 
///           Deserializer: bool = fn( CWorkingObject, XmlNode [, CDeserializer] )
///                         -- Deserializer is a STATIC Method
///           Return Value: a "bool" value telling the framework whether or not the surrogate "completed" the job. 
///        CWorkingObject : A "Working Object" that the surrogate can use to determine if it needs to create a new object or use a pre-created one
///               XmlNode : Contains the data, in XML format, for the deserialization.
///         CDeserializer : Contains the Deserialization Context that MAY GIVE HINTS to the surrogate, but the surrogate can do whatever it wants
/// 
/// An XmlElement will be sent to the surrogate, but the surrogate can accept either an XmlElement -or- an XmlNode.
/// 
/// Because reflection is being used, and because there's no way to enforce type-safety at compile-time, I chose
/// to allow the programmer some leeway in the construction of the methods. 
/// 
/// * Both serialization and deserialization REQUIRE the presence of an XmlNode or an XmlElement parameter, for this is the crux of 
///   this library's functionality.
/// * All of the parameters can be placed in any position in the parameter list- this class will sort it all out.
/// 
/// </remarks>
internal class CSurrogate
{
    /// <summary>
    /// This class contains reflection data about the implicit surrogate
    /// </summary>
    private class CImplicitSurrogate
    {
        /// <summary>
        /// The method that can be .Invoke'ed
        /// </summary>
        internal MethodInfo m_method;

        /// <summary>
        /// The number of parameters that the surrogate accepts
        /// </summary>
        internal int m_parameterCount = 0;

        /// <summary>
        /// The index in the parameter array where to put the (required) XmlNode
        /// </summary>
        internal int m_indexXml = -1;

        /// <summary>
        /// The index in the parameter array where to put the (optional) CFramework object
        /// </summary>
        internal int m_indexFramework = -1;

        /// <summary>
        /// The index in the parameter array where to put the (optional) bool out-param
        /// </summary>
        internal int m_indexObject = -1;

        /// <summary>
        /// Initialize the ImplicitSurrogate object with the method for the surrogate and the number of parameters for the method.
        /// </summary>
        /// <param name="_methodInfo">The MethodInfo object for calling the method</param>
        /// <param name="_parameterCount">The number of parameters for the method</param>
        internal CImplicitSurrogate( MethodInfo _methodInfo, int _parameterCount )
        {
            m_method = _methodInfo;
            m_parameterCount = _parameterCount;
        }
    }

    /// <summary>
    /// The System.Type that this surrogate will take effect for. This must be specified in the constructor
    /// of this surrogate.
    /// </summary>
    private readonly Type m_type;

    /// <summary>
    /// The implicit serializer
    /// </summary>
    private CImplicitSurrogate m_implicitSerializer;

    /// <summary>
    /// The implicit Deserializer
    /// </summary>
    private CImplicitSurrogate m_implicitDeserializer;

    /// <summary>
    /// Return TRUE if there is surrogate information for the Type, FALSE if not
    /// </summary>
    internal bool HasSurrogate => m_implicitDeserializer != null || m_implicitSerializer != null;

    /// <summary>
    /// Construct the surrogate info object using the Type that may have surrogate info.
    /// </summary>
    /// <param name="_type">The Type to search for a Surrogate for</param>
    internal CSurrogate( Type _type )
    {
        m_type = _type ?? throw new ArgumentNullException( "_type" );
        FindSurrogates();
    }

    /// <summary>
    /// Look at the Type for the presence of implicit surrogates. It is OK not to find ANY surrogates, but
    /// it is NOT OK to find one surrogate and not a second.
    /// </summary>
    /// <returns>TRUE if an implicit surrogate was found, FALSE if not</returns>
    private void FindSurrogates()
    {
        // Make sure we don't waste time reflecting on objects in MSCORLIB and Arrays.
        if (ReferenceEquals( m_type.Assembly, typeof( string ).Assembly ))
            // typeof(string).Assembly will get the Assembly of mscorlib
            return;
        if (m_type.IsArray)
            return;

        var desiredMembersFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public |
                                           BindingFlags.Static;
        var members = m_type.GetMembers( desiredMembersFlags );

        foreach (var member in members)
        {
            if (member is ConstructorInfo)
                continue;

            if (Lib.HasAttribute( member, typeof( AImplicitDeserializer ) ))
                EstablishImplicitDeserializer( member );

            if (Lib.HasAttribute( member, typeof( AImplicitSerializer ) ))
                EstablishImplicitSerializer( member );
        }

        // Is this really necessary???? 
        // VerifyBothOrNeither();
    }

    /// <summary>
    /// Given a class member that has already been established as having a <see cref="AImplicitSerializer"/>
    /// attribute, figure out if its a legitimate member to have that attribute. If it is, set the
    /// Implicit Serializer property of this object to the member. If it is not, throw an exception.
    /// </summary>
    /// <param name="_member">The member of the type in question</param>
    private void EstablishImplicitSerializer( MemberInfo _member )
    {
        if (m_implicitSerializer != null)
        {
            throw new XInvalidImplicitSerializer( m_type,
                                                  _member,
                                                  "A class can only have one implicit serializer, and " +
                                                  m_implicitSerializer.m_method.Name +
                                                  " has already been established." );
        }

        var mi = _member as MethodInfo;
        if (mi == null)
        {
            throw new XInvalidImplicitSerializer( m_type,
                                                  _member,
                                                  "Implicit Serializers must be Methods, not " +
                                                  _member.MemberType.ToString() );
        }

        if (mi.IsStatic)
            throw new XInvalidImplicitSerializer( m_type, _member, "Implicit Serializers may not be static." );

        if (mi.IsAbstract)
            throw new XInvalidImplicitSerializer( m_type, _member, "Implicit Serializers may not be abstract." );

        if (mi.ReturnType != typeof( bool ) && mi.ReturnType != typeof( void ))
            throw new XInvalidImplicitSerializer( m_type, _member, "Implicit Serializers must return VOID or BOOL." );

        var surrogate = CreateImplicitSurrogate( mi, false );

        if (surrogate.m_indexXml == -1)
        {
            throw new XInvalidImplicitSerializer( m_type,
                                                  null,
                                                  "There must exist an 'XmlElement' parameter to be a Serializer" );
        }

        m_implicitSerializer = surrogate;
    }


    /// <summary>
    /// Given a class member that has already been established as having a <see cref="AImplicitDeserializer"/>
    /// attribute, figure out if its a legitimate memeber to have that attribute. If it is, set the
    /// Implicit Deserializer property of this object to this member. If it is not, throw an exception.
    /// </summary>
    /// <param name="_member">The member of the type to examine</param>
    private void EstablishImplicitDeserializer( MemberInfo _member )
    {
        if (m_implicitDeserializer != null)
        {
            throw new XInvalidImplicitSerializer( m_type,
                                                  _member,
                                                  "a class can only have one implicit deserializer, and " +
                                                  m_implicitDeserializer.m_method.Name +
                                                  " has already been registered." );
        }

        var mi = _member as MethodInfo;
        if (mi == null)
        {
            throw new XInvalidImplicitSerializer( m_type,
                                                  _member,
                                                  "Implicit Deserializers must be Methods, not " +
                                                  _member.MemberType.ToString() );
        }

        if (!mi.IsStatic)
            throw new XInvalidImplicitSerializer( m_type, _member, "Implicit Deserializers must be static." );

        if (mi.ReturnType != typeof( bool ) && mi.ReturnType != typeof( void ))
        {
            throw new XInvalidImplicitSerializer( m_type,
                                                  _member,
                                                  "Implicit Deserializers must return VOID or BOOL." );
        }

        var surrogate = CreateImplicitSurrogate( mi, true );

        if (surrogate.m_indexObject == -1)
        {
            throw new XInvalidImplicitSerializer( m_type,
                                                  null,
                                                  "There must exist a 'CWorkingObject' parameter to be a deserializer" );
        }
        if (surrogate.m_indexXml == -1)
        {
            throw new XInvalidImplicitSerializer( m_type,
                                                  null,
                                                  "There must exist an 'XmlNode' parameter to be a deserializer" );
        }

        m_implicitDeserializer = surrogate;
    }


    /// <summary>
    /// Assuming that the member is already "legal", now check its parameters. 
    /// </summary>
    /// <param name="_method">The method information that needs to be checked.</param>
    /// <param name="_isDeserializer">If TRUE, then check the member to see if it has an "CWorkingObject" parameter.</param>
    /// <returns>The surrogate information IF the member is a valid surrogate.</returns>
    private CImplicitSurrogate CreateImplicitSurrogate( MethodInfo _method, bool _isDeserializer )
    {
        var parms = _method.GetParameters();
        var imp = new CImplicitSurrogate( _method, parms.Length );

        for (var i = 0; i < imp.m_parameterCount; i++)
        {
            var pi = parms[i];

            if (_isDeserializer)
            {
                if (ProcessParameterType( pi, typeof( CWorkingObject ), _method, ref imp.m_indexObject ))
                    continue;
            }
            if (ProcessParameterType( pi, typeof( XmlNode ), _method, ref imp.m_indexXml ))
                continue;
            if (ProcessParameterType( pi, typeof( CFramework ), _method, ref imp.m_indexFramework ))
                continue;

            throw new XInvalidImplicitSerializer( m_type,
                                                  _method,
                                                  "an implicit (de)serializer may not have a parameter of type: " +
                                                  pi.ParameterType.ToString() );
        }
        return imp;
    }


    /// <summary>
    /// Handler for dealing with a specific parameter on an implicit surrogate.
    /// </summary>
    /// <remarks>
    /// When processing parameters for an implicit surrogate, we test the parameter against an "expected type". The parameter type
    /// must be cast-able to the expected type. Also, we check the "index" of the parameter to make sure we haven't "found" a 
    /// parameter of that type before. 
    /// 
    /// If the parameter is NOT of the type, then return false.
    /// If the parameter IS of the type, check the index to make sure its not been set before, and set it to the value specified
    /// as the "currentIndex" parameter to this function.
    /// 
    /// Huge fallback is that this does not work as-is if a method is meant to have multiple parameters of the same type.
    /// 
    /// The only reason that this is not a STATIC member is 
    /// </remarks>
    /// <param name="_paramInfo">The ParameterInfo object for this parameter</param>
    /// <param name="_expectedType">The expected type for the parameter- The actual type must be cast-able to this type</param>
    /// <param name="_method">The <see cref="MethodInfo"/> for the method we're checking</param>
    /// <param name="_index">A reference to the storage location where the Index will be stored.</param>
    /// <returns></returns>
    private bool ProcessParameterType( ParameterInfo _paramInfo,
                                       Type _expectedType,
                                       MethodInfo _method,
                                       ref int _index )
    {
        var parameterType = _paramInfo.ParameterType;

        if (_expectedType.IsAssignableFrom( parameterType ))
        {
            if (_index != -1)
            {
                throw new XInvalidImplicitSerializer( m_type,
                                                      _method,
                                                      "an Implicit Serializer may only accept a single " +
                                                      _expectedType.ToString() + " parameter." );
            }
            _index = _paramInfo.Position;
            return true;
        }
        return false;
    }


    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // The rest of the file contains the code to actually serialize or deserialize using the data constructed above. //
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    /// <summary>
    /// Create the parameter array for an implicit surrogate using the data created by the rest of this class.
    /// </summary>
    /// <param name="_implicit">The implicit surrogate</param>
    /// <param name="_xmlToSerializeTo">The XML that the object will serialize itself to</param>
    /// <param name="_framework">The Context that the object will use (optionally) to serialize the data.</param>
    /// <param name="_object">The "Working Object" useful to a deserializer</param>
    /// <returns>an object[] containing the parameter data as dictated by the implicitSurrogate object</returns>
    private static object[] BuildParamArray( CImplicitSurrogate _implicit,
                                             XmlElement _xmlToSerializeTo,
                                             CFramework _framework,
                                             CWorkingObject _object )
    {
        var arr = new object[_implicit.m_parameterCount];
        arr[_implicit.m_indexXml] = _xmlToSerializeTo;

        if (_implicit.m_indexFramework != -1)
            arr[_implicit.m_indexFramework] = _framework;

        if (_implicit.m_indexObject != -1)
            arr[_implicit.m_indexObject] = _object;

        return arr;
    }


    /// <summary>
    /// Serialize an object of this type.
    /// </summary>
    /// <param name="_object">The object to serialize</param>
    /// <param name="_xml">The XML node that will receive the data for the serialization</param>
    /// <param name="_serializer">The serialization context to help with serialization</param>
    /// <returns>TRUE if the serialization is complete, FALSE if the framework needs to complete the serialization</returns>
    internal bool Serialize( object _object, XmlElement _xml, CSerializer _serializer )
    {
        var surrogate = m_implicitSerializer;
        if (surrogate == null)
            return false;

        var paramArray = BuildParamArray( surrogate, _xml, _serializer, null );

        var isComplete = surrogate.m_method.Invoke( _object, paramArray );
        // serializer is always an instance method
        if (isComplete is bool)
            return (bool) isComplete;
        else
            return true;
        // An implicit serializer is assumed to completely serialize the Type and all base classes unless explicitly stated otherwise.
    }

    /// <summary>
    /// Deserialize an object of this type.
    /// </summary>
    /// <param name="_object">A reference to an object that is being deserialized. NULL --> The serializer needs to create the object!</param>
    /// <param name="_xml">The XML node that contains the data for the deserialization</param>
    /// <param name="_framework">The serialization context to help with deserialization</param>
    /// <returns>TRUE if the deserialization is complete, FALSE if the framework needs to complete the deserialization</returns>
    internal bool Deserialize( CWorkingObject _object, XmlElement _xml, CFramework _framework )
    {
        var surrogate = m_implicitDeserializer;
        if (surrogate == null)
            return false;

        var paramArray = BuildParamArray( surrogate, _xml, _framework, _object );

        var isComplete = surrogate.m_method.Invoke( null, paramArray ); // deserializer is always a static method
        if (isComplete is bool)
            return (bool) isComplete;
        else
            return true;
        // An implicit deserializer is assumed to completely serialize the Type and all base classes unless explicitly stated otherwise.
    }
}