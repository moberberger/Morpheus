using System.Collections;
using System.Reflection;

#nullable disable

namespace Morpheus.Serialization;


/// <summary>
/// This class contains Serializer-interesting data about a given Type. It contains Declared-Only fields- fields belonging to a base
/// class are found on the m_baseType member of this class.
/// </summary>
internal class CTypeData : IEnumerable<CFieldData>
{
    /// <summary>
    /// A Cache of all of the Type's that have had CTypeData generated for them before.
    /// </summary>
    private static readonly Dictionary<Type, CTypeData> sm_typeTable = new Dictionary<Type, CTypeData>();

    /// <summary>
    /// All of the fields for this Type.
    /// </summary>
    private readonly List<CFieldData> m_fields = new List<CFieldData>();

    /// <summary>
    /// When Set by adding the <see cref="AExplicitlySerialize"/> attribute to the CLASS, only fields with this attribute will be added to the field-list
    /// </summary>
    private bool m_onlySerializeExplicitFields = false;

    /// <summary>
    /// This is set when the class has the <see cref="ADoNotSerialize"/> attribute on it. This merely makes sure that no fields on the class are serialized.
    /// </summary>
    private bool m_doNotSerialize = false;

    /// <summary>
    /// The Renamer specified by adding a <see cref="AUseFieldRenamer"/> attribute to the CLASS and setting the Type of the renamer as the attribute-parameter.
    /// </summary>
    private IFieldRenamer m_renamer = null;

    /// <summary>
    /// When TRUE, the CFieldData object has to call the <see cref="RenameField"/> method for each and every
    /// time its name is queried. FALSE means that the m_renamer can be set to null after all fields are "discovered".
    /// </summary>
    internal bool m_dynamicFieldRenamer = false;


    /// <summary>
    /// The Type for this object
    /// </summary>
    internal Type Type { get; }

    /// <summary>
    /// The Base Type data for this Type.
    /// </summary>
    internal CTypeData BaseType { get; private set; } = null;

    /// <summary>
    /// The Implicit Surrogate information for the Type. If this value is null, then there are no implicit surrogates.
    /// </summary>
    internal CSurrogate ImplicitSurrogate { get; private set; } = null;

    /// <summary>
    /// The Explicit Surrogate information for the Type. If this value is null, then there are no explicit surrogates.
    /// </summary>
    internal ConstructorInfo ExplicitSurrogate { get; private set; } = null;



    /// <summary>
    /// The number of serializable fields for this type
    /// </summary>
    internal int FieldCount => m_fields.Count;

    /// <summary>
    /// Indexing the CTypeData object will return the CFieldData for the field at the specified index.
    /// </summary>
    /// <param name="_index">The index of the field desired</param>
    /// <returns>The CFieldData object for the requested field.</returns>
    internal CFieldData this[int _index]
    {
        get
        {
            if (_index < 0 || _index >= m_fields.Count)
                return null;

            return m_fields[_index];
        }
    }

    /// <summary>
    /// Index the Fields array based on the fieldName of the field
    /// </summary>
    /// <param name="_fieldName">The name of the field</param>
    /// <returns>The CFieldData for the field named _fieldName, or NULL if on field by that name exists</returns>
    internal CFieldData this[string _fieldName]
    {
        get
        {
            foreach (var field in m_fields)
            {
                if (field.XmlName == _fieldName)
                    return field;
            }
            return null;
        }
    }


    /// <summary>
    /// The External interface into this class- Find a CTypeData object for a given Type. Uses the cache to 
    /// limit the amount of processing performed.
    /// </summary>
    /// <param name="_type">The Type to search for</param>
    /// <returns>A CTypeData object containing interesting features of the Type</returns>
    internal static CTypeData GetTypeData( Type _type )
    {
        lock (sm_typeTable)
        {
            if (sm_typeTable.TryGetValue( _type, out var typeData ))
                return typeData;

            typeData = new CTypeData( _type );
            sm_typeTable.Add( _type, typeData );
            return typeData;
        }
    }


    /// <summary>
    /// Must construct one of these with a Type. Private constructor means that only this class may create an
    /// object of this type.
    /// </summary>
    /// <param name="_type">The Type to generate data for</param>
    private CTypeData( Type _type )
    {
        Type = _type;

        GenerateAttributeData();
        GenerateFieldData();
        GenerateBaseClassData();
        GetImplicitSurrogate();
        GetPrivateDefaultConstructor();

        if (!m_dynamicFieldRenamer) // If there is no dynamic field renamer, then...
            m_renamer = null; //  make sure that the object created to rename fields is released for GC
    }

    private void GetPrivateDefaultConstructor()
    {
        var constructors = Type.GetConstructors( BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public );
        foreach (var ci in constructors)
        {
            var parms = ci.GetParameters();
            if (parms.Length == 0)
            {
                var impDeserializerAttr = ci.GetCustomAttributes( typeof( AImplicitDeserializer ), false );
                if (impDeserializerAttr.Length > 0)
                    ExplicitSurrogate = ci;
            }
        }
    }


    /// <summary>
    /// For the m_type, analyse the attributes on the Type and store the useful ones in this object
    /// </summary>
    private void GenerateAttributeData()
    {
        var attrList = Type.GetCustomAttributes( false );
        foreach (var a in attrList)
        {
            if (a is AExplicitlySerialize)
            {
                m_onlySerializeExplicitFields = true;
            }
            else if (a is ADoNotSerialize)
            {
                m_doNotSerialize = true;
            }
            else if (a is AUseFieldRenamer renamerAttr)
            {
                m_renamer = (IFieldRenamer) Activator.CreateInstance( renamerAttr.RenamerType );
                m_dynamicFieldRenamer = renamerAttr.DynamicRenaming;
            }
        }
    }

    /// <summary>
    /// This method will enumerate over all of the object's fields and generate the m_fields list.
    /// </summary>
    private void GenerateFieldData()
    {
        if (m_doNotSerialize) // if this class is not to be serialized, then don't find any fields for it.
            return;

        if (Type.Namespace == "System.Data.Entity.DynamicProxies")
            return;

        var allFields = Type.GetFields( BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public );

        // Go through each field found and process according to certain rules
        foreach (var fi in allFields)
        {
            var t = fi.FieldType;
            if (!CFramework.IsValidType( t ))
                continue;

            var fld = new CFieldData( fi, this );

            if (fld.DoNotSerialize) // If the field does not want to be serialized, then don't.
                continue;

            if (m_onlySerializeExplicitFields && !fld.ExplicitlySerialize)
                // the field is NOT explicitly marked AND the class requests ONLY explicitly marked fields get serialized
                continue;

            m_fields.Add( fld );
        }
    }

    /// <summary>
    /// Generate meaningful data for the base-type of this type.
    /// </summary>
    private void GenerateBaseClassData()
    {
        if (Type.BaseType == typeof( object ) || Type.BaseType == null)
            return;

        BaseType = GetTypeData( Type.BaseType );
    }

    /// <summary>
    /// Get the implicit surrogate information for the Type.
    /// </summary>
    private void GetImplicitSurrogate()
    {
        var implicitSurrogate = new CSurrogate( Type );
        if (implicitSurrogate.HasSurrogate) // Only assign to the member field IF the surrogate exists
            ImplicitSurrogate = implicitSurrogate;
    }


    /// <summary>
    /// If there is an explicit constructor defined, then invoke it and return the object that was created.
    /// </summary>
    /// <returns></returns>
    internal object TryExplicitConstructor()
    {
        if (ExplicitSurrogate == null)
            return null;

        return ExplicitSurrogate.Invoke( new object[0] );
    }

    /// <summary>
    /// Used by the CFieldData object to generate an appropriate Xml Name for a field
    /// </summary>
    /// <param name="_fieldName">The name of the field to rename</param>
    /// <param name="_fieldInfo">The FieldInfo data from the CFieldData object</param>
    /// <returns>NULL if there is no rename performed, or the new XmlName if the field is to be renamed.</returns>
    internal string RenameField( string _fieldName, FieldInfo _fieldInfo )
    {
        if (m_renamer == null)
            return _fieldName;

        return m_renamer.ConvertFieldName( _fieldName, _fieldInfo );
    }


    public IEnumerator<CFieldData> GetEnumerator() => m_fields.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => m_fields.GetEnumerator();
}