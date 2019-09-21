using Morpheus.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;


namespace Morpheus
{
    /// <summary>
    /// This class provides methods that allow data to be transmuted between a .NET object and
    /// Xml.
    /// </summary>
    /// <remarks>
    /// This class will operate on ALL fields, including protected and private ones. If you
    /// don't like this idea, then don't use this class. Use the .NET Xml serializer instead.
    /// 
    /// EXAMPLES AND FORMATS -------------------- class CPerson { string m_name; int m_age;
    /// CAddress m_address; string[] m_kidsNames; string m_aNullValue = null; CAddress
    /// m_otherAddress; }
    /// 
    /// class CAddress { string Street; string City; int Zip; }
    /// 
    /// class CSuperAddress : CAddress { string Country; }
    /// 
    /// --- yields ---
    /// 
    /// <CPerson> <Name> Homer Simpson </Name> <Age> 35 </Age> <Address> <Street> 45 Mount
    /// Horrible </Street> <City> Springfield </City> <Zip> 84372 </Zip> </Address>
    /// <KidsNames _Array="3"> Bart,Lisa,Maggie </KidsNames> <ANullValue Null="true"/>
    /// <OtherAddress Type="CSuperAddress"> <Street> 88 Schroeder Way </Street> <City> Boston
    /// </City> <Zip> 12837 </Zip> <Country> USA </Country> </OtherAddress> </CPerson>
    /// 
    /// 
    /// </remarks>
    /// <remarks>Use the "Helper" class for static methods.</remarks>
    public class CDeserializer : CFramework
    {
        /// <summary>
        /// The table used to remember objects that have been deserialized with a "refID" tag.
        /// </summary>
        private readonly Dictionary<string, object> m_references = new Dictionary<string, object>();

        /// <summary>
        /// Default constructor uses the "Global" serialization context
        /// </summary>
        public CDeserializer()
        {
        }

        /// <summary>
        /// Construct with a default context object
        /// </summary>
        /// <param name="_context">The context object to use for the serialization</param>
        public CDeserializer( CSerializationContext _context )
            : base( _context )
        {
        }


        /// <summary>
        /// Given an XmlNode, turn the data into that node into an object, if possible
        /// </summary>
        /// <param name="_xml">
        /// The Xml that contains data useful in filling out the fields of an object
        /// </param>
        /// <returns>The object created from the XML</returns>
        public object Deserialize( XmlNode _xml )
        {
            StartProcessing();

            var elem = MakeElement( _xml );
            return FrameworkDeserialize( elem, null );
        }

        /// <summary>
        /// Given an XmlNode, turn the data into that node into an object, if possible. Assume
        /// that the object needs to be of generic-parameter Type T
        /// </summary>
        /// <typeparam name="T">
        /// The Type that the Xml is supposed to deserialize into
        /// </typeparam>
        /// <param name="_xml">
        /// The Xml that contains data useful in filling out the fields of an object
        /// </param>
        /// <returns>The object of type T created from the XML</returns>
        public T Deserialize<T>( XmlNode _xml )
        {
            StartProcessing();

            var elem = MakeElement( _xml );
            return (T) FrameworkDeserialize( elem, typeof( T ) );
        }

        /// <summary>
        /// Helper to make sure that what's sent in to the Deserializer is handled properly.
        /// Turns an XmlDocument into an XmlElement if the parameter is a document.
        /// </summary>
        /// <param name="_xml">The XML containing the data for the object</param>
        /// <returns>An XmlElement appropriate for deserialization</returns>
        private static XmlElement MakeElement( XmlNode _xml ) => (_xml as XmlElement) ?? ((XmlDocument) _xml).DocumentElement;

        /// <summary>
        /// This is the recursively-called routine that will actually direct the deserialization
        /// using the analysis of the xml node being deserialized.
        /// </summary>
        /// <param name="_xml">The XML containing the data for the object</param>
        /// <param name="_defaultType">
        /// The Type that is expected for this object- this may be overridden by the XML
        /// </param>
        /// <returns>The object that was created from the XML</returns>
        public object FrameworkDeserialize( XmlElement _xml, Type _defaultType )
        {
            if (_xml == null)
                return null;

            var workingObject = new CWorkingObject();

            // Figure out if this process can be cut way short with "null" or "reference" nodes
            if (CheckNullAndReference( _xml, workingObject ))
                return workingObject.WorkingObject;

            // Figure out the Type that we should be working with
            var oType = GetTypeFromXmlOrDefault( _xml, _defaultType );
            if (oType == null)
            {
                throw new XDeserializationError(
                    "For the XmlElement named '" +
                    _xml.Name +
                    "', there was no Type information available (Type Attribute: '" +
                    m_context.TypeAttributeName + "')" );
            }

            // Check for Surrogates first, and if not then handle deserialization with this
            // class.
            if (!ApplySurrogates( _xml, oType, workingObject ))
            {
                // An invalid type cannot be deserialized
                if (IsValidType( oType ))
                    HandleDeserialization( _xml, oType, workingObject ); // Let the Framework do its thing
            }

            return workingObject.WorkingObject;
        }

        /// <summary>
        /// Check the easiest forms of deserialization- Null and RefTo an object already
        /// deserialized.
        /// </summary>
        /// <param name="_xml">The XML that's being deserialized</param>
        /// <param name="_workObj">
        /// The Object that was deserialized (when the return value is TRUE)
        /// </param>
        /// <returns>
        /// TRUE means that the _workObj parameter contains valid data and nothing further
        /// should be done, FALSE means that no deserialization was performed.
        /// </returns>
        private bool CheckNullAndReference( XmlElement _xml, CWorkingObject _workObj )
        {
            // Check the XML to see if this is a NULL object reference. If it is, then return
            // TRUE.
            if (CXmlHelper.HasAttribute( _xml, m_context.NullAttributeName ))
                return true;

            // Check the XML to see if its referring to some other object.
            var referTo = CXmlHelper.GetAttributeValue( _xml, m_context.ReferToAttributeName );
            if (referTo != null) // There was a reference to another object, so handle it and return that other object.
            {
                // Check the table for the RefID to get the object. The reference must be in the
                // table already because forward-looking references are not supported.
                if (!m_references.TryGetValue( referTo, out var obj ))
                    throw new XUnknownReference( "All object-references must be to backward-defined objects. The RefID " + referTo + " has not been defined yet." );

                _workObj.Set( obj );
                return true;
            }

            // Check to see if this element is associating the XML with a reference ID
            var refId = CXmlHelper.GetAttributeValue( _xml, m_context.ReferenceIdAttributeName );
            if (refId != null)
                _workObj.SetRefInfo( this, refId );

            return false;
        }

        /// <summary>
        /// The framework will deserialize the object. No more surrogates or other "exceptional"
        /// behaviour.
        /// </summary>
        /// <param name="_xml"></param>
        /// <param name="_type"></param>
        /// <param name="_workingObject"></param>
        private void HandleDeserialization( XmlElement _xml, Type _type, CWorkingObject _workingObject )
        {
            // Strings are really easy- just return the "InnerText"
            if (_type == TYPEOF_STRING)
                _workingObject.Set( Environment.ExpandEnvironmentVariables( _xml.InnerText ) );
            // Primitives are also pretty easy, only because of the "Convert" class
            else if (_type.IsPrimitive)
                _workingObject.Set( Convert.ChangeType( _xml.InnerText, _type ) );
            // Check for an Array
            else if (_type.IsArray || CXmlHelper.HasAttribute( _xml, m_context.ArrayAttributeName ))
                DeserializeArray( _xml, _type, _workingObject );
            else if (_type.IsEnum)
                _workingObject.Set( Enum.Parse( _type, _xml.InnerText, false ) );
            else // Handle ref-type fields and base classes.
                DeserializeReferenceType( _xml, _type, _workingObject );
        }

        /// <summary>
        /// The Type is a reference type with zero or more fields, and it is to be deserialized
        /// using the Framework's algorithm
        /// </summary>
        /// <param name="_xml">The XML containing the field data</param>
        /// <param name="_type">The Type that is to be deserialized</param>
        /// <param name="_workingObject">
        /// The working object to be used if it is set (create new object instance if not)
        /// </param>
        /// <returns>The newly deserialized object</returns>
        private void DeserializeReferenceType( XmlElement _xml, Type _type, CWorkingObject _workingObject )
        {
            var typeData = CTypeData.GetTypeData( _type );

            if (!_workingObject.IsSet)
            {
                // Check to see if we can do anything with an explicit constructor (likely to be
                // private)
                var o = typeData.TryExplicitConstructor();
                if (o == null)
                    o = Activator.CreateInstance( _type );
                if (o == null)
                    throw new XDeserializationError( "Could not create a new object of type " + _type.ToString() );

                _workingObject.Set( o );
            }

            var currentObject = _workingObject.WorkingObject;
            while (typeData != null)
            {
                DeserializeObjectFields( _xml, currentObject, typeData );
                ClearIgnoredFields();

                typeData = typeData.BaseType;
                if (typeData != null)
                {
                    if (ApplySurrogates( _xml, typeData.Type, _workingObject ))
                        typeData = null;
                }
            }
        }

        /// <summary>
        /// This routine will enumerate all fields associated with an object and deserialize
        /// each field. This routine's scope is simply the declared fields for the "top" level
        /// of the class's inheritance chain- in other words, this will NOT deserialize a base
        /// class's fields.
        /// </summary>
        /// <param name="_xml">The XML containing the field data</param>
        /// <param name="_objectToPopulate">The object whose fields are being populated</param>
        /// <param name="_typeData">
        /// Serialization Type Data describing this Type (not to include base-class)
        /// </param>
        private void DeserializeObjectFields( XmlElement _xml, object _objectToPopulate, CTypeData _typeData )
        {
            foreach (var fieldData in _typeData)
            {
                var fi = fieldData.Field;
                if (IsIgnoredField( fi.Name ))
                    continue;

                var expectedType = fi.FieldType;
                var name = GetFieldName( fieldData );
                var fixedName = FixMemberName( name );
                var elem = _xml[fixedName];

                if (elem != null)
                {
                    PushCurrentField( fieldData );
                    var fieldValue = FrameworkDeserialize( elem, expectedType );
                    fi.SetValue( _objectToPopulate, fieldValue );
                    PopField();
                }
            }
        }

        /// <summary>
        /// Create an Array from the data in the Xml and return that array.
        /// </summary>
        /// <param name="_xml">The XML containing the data for the array</param>
        /// <param name="_type">The Type of the array, as inferred from the framework</param>
        /// <param name="_workingObject">
        /// A working object (optional) that must be an Array (if its not null)
        /// </param>
        /// <returns></returns>
        private void DeserializeArray( XmlElement _xml, Type _type, CWorkingObject _workingObject )
        {
            var condensedArray = FindCondensedArray( _xml );
            var inferredLength = (condensedArray != null) ? condensedArray.Length : -1;

            var arrayHelper =
                CreateArrayDeserializationHelper( _xml, _type, _workingObject, inferredLength );

            // At this stage, we know its a Rank 1 array. Figure out the serialization of that
            // array Single child node that's a text node is an assumed indicator that the
            // "Condensed" format was used to serialize
            if (condensedArray != null)
                DeserializeArrayFromCondensedArray( condensedArray, arrayHelper );
            else
                // There are individual child XmlElements for each array element. "Index"
                // attributes can be used to "skip" null array values.
                DeserializeArrayFromElements( _xml, arrayHelper );

            if (!_workingObject.IsSet)
                _workingObject.Set( arrayHelper.Array );
        }

        /// <summary>
        /// Using childNodes from an element, deserialize an array by deserializing the
        /// individual elements into the array elements.
        /// </summary>
        /// <param name="_xml">
        /// The XML parent node containing the array elements as childNodes
        /// </param>
        /// <param name="_arrayHelper">The ArrayDeserializationHelper</param>
        private void DeserializeArrayFromElements( XmlElement _xml, CArrayDeserializationHelper _arrayHelper )
        {
            foreach (XmlNode node in _xml.ChildNodes)
            {
                if (!(node is XmlElement elem))
                {
                    throw new XDeserializationError( node.NodeType.ToString() +
                                                     " was the nodetype found (instead of XmlElement) as a child of the array's XML" );
                }

                int[] indicies = null;
                var sIndex = CXmlHelper.GetAttributeValue( elem, m_context.ArrayIndexAttributeName );
                if (sIndex != null)
                    indicies = CHelper.ConvertStringToArray<int>( sIndex, ',' );

                if (indicies != null && indicies.Length > 0)
                    _arrayHelper.SetIndicies( indicies );

                var obj = FrameworkDeserialize( elem, _arrayHelper.ElementType );
                _arrayHelper.Add( obj );
            }
        }

        /// <summary>
        /// Using the info in a "condensed array", deserialize an Array into an object.
        /// </summary>
        /// <param name="_condensedArray">
        /// The "Condensed Array"- a string[] containing data from a CSV list
        /// </param>
        /// <param name="_arrayHelper">The ArrayDeserializationHelper</param>
        private static void DeserializeArrayFromCondensedArray( string[] _condensedArray,
                                                                CArrayDeserializationHelper _arrayHelper )
        {
            var elemType = _arrayHelper.ElementType;
            foreach (var s in _condensedArray)
            {
                if (elemType == TYPEOF_STRING)
                {
                    var str = UnprotectStringFromStringlist( s );
                    _arrayHelper.Add( str );
                }
                else if (elemType.IsPrimitive)
                {
                    var obj = Convert.ChangeType( s, elemType );
                    _arrayHelper.Add( obj );
                }
                else
                {
                    throw new XDeserializationError(
                        "A single XmlText node was found, but the ElementType is complex- " + elemType.ToString() );
                }
            }
        }

        /// <summary>
        /// Given information about the current state, create an ArrayDeserializationHelper to
        /// help with the deserialization process
        /// </summary>
        /// <param name="_xml">The XML containing the Array information</param>
        /// <param name="_type">The Type of the array expected</param>
        /// <param name="_workingObject">
        /// A "Working Object", likely from a surrogate that didn't complete the deserialization
        /// </param>
        /// <param name="_inferredLength">
        /// The Inferred Length of the array, from a "Condensed Array" already found in the XML
        /// </param>
        /// <returns>
        /// A properly formed <see cref="CArrayDeserializationHelper"/> object.
        /// </returns>
        private CArrayDeserializationHelper CreateArrayDeserializationHelper( XmlElement _xml,
                                                                              Type _type,
                                                                              CWorkingObject _workingObject,
                                                                              int _inferredLength )
        {
            CArrayDeserializationHelper arrayHelper;
            if (_workingObject.IsSet)
            {
                arrayHelper = new CArrayDeserializationHelper( _workingObject.WorkingObject as Array );
            }
            else
            {
                // Make sure that the array type actually is an array and it has an ElementType
                var arrType = _type;
                if (!arrType.IsArray)
                {
                    throw new XDeserializationError(
                        "The Type specified is not an array or it does not have an ElementTypes associated with it- " +
                        arrType.ToString() );
                }

                // Get info from the XML
                var sLengths = CXmlHelper.GetAttributeValue( _xml, m_context.ArrayAttributeName );
                var lengths = CHelper.ConvertStringToArray<int>( sLengths, ',' );

                var sLowerBounds = CXmlHelper.GetAttributeValue( _xml, m_context.ArrayLowerBoundAttribute );
                var lowerBounds = CHelper.ConvertStringToArray<int>( sLowerBounds, ',' );

                if (lengths == null)
                {
                    if (_inferredLength > -1)
                        lengths = new int[] { _inferredLength };
                    else
                        lengths = new int[] { InferArrayLength( _xml ) }; // This assumes a 1-dim array at this point.
                }

                arrayHelper = new CArrayDeserializationHelper( arrType.GetElementType(), lengths, lowerBounds );
            }

            return arrayHelper;
        }

        /// <summary>
        /// Check the XML to see if it contains a CSV list of stuff. IF it does, then split it
        /// up and return the resulting string[]
        /// </summary>
        /// <param name="_xml">The XML node to check</param>
        /// <returns>
        /// Null if nothing was found, or a string[] containing the values from the CSV list if
        /// it is found
        /// </returns>
        private static string[] FindCondensedArray( XmlElement _xml )
        {
            string[] condensedArray = null;
            // Single child node that's a text node is an assumed indicator that the "Condensed"
            // format was used to serialize
            if (_xml.ChildNodes.Count == 1 && _xml.ChildNodes[0].NodeType == XmlNodeType.Text)
            {
                var text = _xml.InnerText;
                if (text == "")
                    condensedArray = new string[0];
                else
                    condensedArray = text.Split( ',' );
            }
            return condensedArray;
        }

        /// <summary>
        /// This routine will try to figure out how large an array is based on the nodes in the
        /// XML
        /// </summary>
        /// <param name="_xml"></param>
        /// <returns></returns>
        private int InferArrayLength( XmlElement _xml )
        {
            // XmlNode node = _xml.SelectSingleNode( @"*/*[@idx][last()]" );
            var index = 0;
            foreach (XmlNode node in _xml.ChildNodes)
            {
                var sIndex = CXmlHelper.GetAttributeValue( node, m_context.ArrayIndexAttributeName );
                if (!string.IsNullOrEmpty( sIndex ))
                    index = int.Parse( sIndex );
                index++;
            }

            return index;
        }


        /// <summary>
        /// Check to see if there are surrogates that handle the deserialization.
        /// </summary>
        /// <param name="_xml">The XML containing the object data</param>
        /// <param name="_type">The TYPE that we're trying to deserialize</param>
        /// <param name="_workingObject">
        /// A "Working Object" that deserializers use to determine if they need to create a new
        /// object or if they have an object that they can deserialize into.
        /// </param>
        /// <returns>
        /// TRUE if a surrogate was successful in completing the deserialization of the object,
        /// or FALSE if the surrogates were not able to complete all deserialization
        /// </returns>
        private bool ApplySurrogates( XmlElement _xml, Type _type, CWorkingObject _workingObject )
        {
            var isComplete = false;

            _workingObject.WorkingType = _type;

            // Check for EntitySemantics as a special and overriding Surrogate method The
            // UseEntitySemantics on the XML element is ignored, as the destination Type gets to
            // define how it gets deserialized (the attribute may be little more than a Hint to
            // what happened at serializatiion time) See for more information on EntitySemantics
            if (CEntityTypeData.UsesEntitySemantics( _type ))
            {
                DeserializeUsingEntitySemantics( _xml, _type, _workingObject );
                return true;
            }

            // Check for external surrogates
            var externalSurrogate = GetExternalSurrogate( _type );
            if (externalSurrogate != null)
                isComplete = externalSurrogate.Deserialize( _workingObject, _xml, this );
            if (isComplete)
                return true;

            // Check the Implicit surrogates if the External surrogate(s) didn't finish the
            // deserialization
            var typeData = CTypeData.GetTypeData( _type );
            var implicitSurrogate = typeData.ImplicitSurrogate;
            if (implicitSurrogate != null && implicitSurrogate.HasSurrogate)
                isComplete = implicitSurrogate.Deserialize( _workingObject, _xml, this );
            if (isComplete)
                return true;

            return false;
        }


        /// <summary>
        /// The Type uses EntitySemantics. See <see cref="CEntityTypeData"/> for more info on
        /// EntitySemantics
        /// </summary>
        /// <param name="_xml">The XML containing the data</param>
        /// <param name="_type">
        /// The Type that has already been determined to use Entity Semantics
        /// </param>
        /// <param name="_workingObject">Where to put the resulting object</param>
        private void DeserializeUsingEntitySemantics( XmlElement _xml, Type _type, CWorkingObject _workingObject )
        {
            var obj = _workingObject.GetExistingOrCreateNew( _type );
            var typeData = CEntityTypeData.GetTypeData( _type );

            for (var i = 0; i < typeData.NonCollectionProperties.Length; i++)
            {
                var prop = typeData.NonCollectionProperties[i];
                var xml = _xml[prop.Name];

                if (xml != null) // There was some XmlElement for the property
                {
                    // This isn't a Field, and we're using EntitySemantics, so use there's
                    // nothing to query
                    PushCurrentField( null );

                    var propVal = FrameworkDeserialize( xml, prop.PropertyType );
                    prop.SetValue( obj, propVal );

                    PopField(); // pop the null we pushed
                }
            }

            for (var i = 0; i < typeData.CollectionProperties.Length; i++)
            {
                var colProp = typeData.CollectionProperties[i];

            }
        }


        /// <summary>
        /// Check an XmlElement to see if it includes a specific "Type" attribute. If so, return
        /// the actual Type associated with that attribute's value.
        /// </summary>
        /// <param name="_xml">The XmlElement containing the field's information</param>
        /// <param name="_defaultType">
        /// If there is no Type attribute, then return this value as the "default"
        /// </param>
        /// <returns>
        /// NULL if no Type attribute was found, or the Type object corresponding the value of
        /// the Type attribute
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if there is a Type attribute, but that attribute's value cannot be turned
        /// into a Type object.
        /// </exception>
        private Type GetTypeFromXmlOrDefault( XmlElement _xml, Type _defaultType )
        {
            var sType = CXmlHelper.GetAttributeValue( _xml, m_context.TypeAttributeName );
            if (sType == null) // There is no explicit Type specifier (XmlAttribute)
                return _defaultType;

            var explicitType = ReflectionExtenstions.BetterGetType( sType, true );
            if (explicitType == null)
            {

                // The XML had an explicit Type, but that Type couldn't be found. So... If we
                // are trying to deserialize a Type that uses EntitySemantics, we assume that
                // the destination Type is sufficient to figure out what to deserialize. If the
                // destination Type is wholly inadequate to "receive" the data in the XML, the
                // application is at fault. For Entities, this type of mismatch is OK.
                if (CEntityTypeData.UsesEntitySemantics( _defaultType ))
                    return _defaultType;

                // However, if this isn't using EntitySemantics, then throw an exception as the
                // Type is unknown.
                throw new XDeserializationError( "An Attribute was found for an Explicit Type, but it could not be turned into a Type: " + sType );
            }

            return explicitType;
        }


        /// <summary>
        /// Return the "Expected Type" of the field that the framework is currently working on.
        /// This value may be NULL if this value is at the "Top" of the (de)serialization,
        /// before any "field" has been detected.
        /// </summary>
        /// <returns>
        /// The <see cref="Type"/> of the field the framework is working on, or NULL if at the
        /// Top of the operation.
        /// </returns>
        public Type GetExpectedType( XmlElement _element )
        {
            Type retval = null;

            var fd = TopField;
            if (fd != null)
                retval = fd.Field.FieldType;

            return GetTypeFromXmlOrDefault( _element, retval );
        }


        /// <summary>
        /// Associate the object with the given reference ID.
        /// </summary>
        /// <remarks>
        /// This is called from within the <see cref="CWorkingObject"/> class when the working
        /// object is set.
        /// </remarks>
        /// <param name="_object">The object to be associated with the reference id</param>
        /// <param name="_refId">The refId that the object belongs to</param>
        internal void SetObjectRefId( object _object, string _refId ) => m_references[_refId] = _object;
    }
}
