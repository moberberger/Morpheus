using System.Reflection;

#nullable disable

namespace Morpheus.Serialization;


/// <summary>
/// This class is used to track information about a specific field on a class. This is considered "immutable" data in that
/// it will never be modified by any "current serialziation context". However, the current context CAN ignore any of the 
/// directives here if it needs to.
/// </summary>
internal class CFieldData
{
    /// <summary>
    /// The TypeData object that this field belongs to. Used for the auto-field-renaming aspect.
    /// </summary>
    private readonly CTypeData m_typeData;

    /// <summary>
    /// This is, for all intents and purposes, the name by what the framework should know this field by. If it is NULL,
    /// then the XmlName property will query the m_typeData object for the name of the field.
    /// </summary>
    private readonly string m_fieldName;


    /// <summary>
    /// The FieldInfo object that this data object represents
    /// </summary>
    internal FieldInfo Field { get; }

    /// <summary>
    /// When set, this field is never to be serialized.
    /// </summary>
    internal bool DoNotSerialize { get; } = false;

    /// <summary>
    /// When set, this field should ALWAYS be serialized by the framework- this has no effect if a surrogate is handling serialization
    /// </summary>
    internal bool ExplicitlySerialize { get; } = false;

    /// <summary>
    /// The name that should be used for the XmlNode for this field
    /// </summary>
    internal string XmlName
    {
        get
        {
            if (m_fieldName != null)
                return m_fieldName;
            else
                return m_typeData.RenameField( Field.Name, Field );
        }
    }


    /// <summary>
    /// Construct the field data from a FieldInfo object. Internal Constructor means that noone outside this library
    /// should be creating objects of this class.
    /// </summary>
    /// <param name="_fieldInfo">The FieldInfo object</param>
    /// <param name="_typeData">The TypeData object that is constructing this CFieldData</param>
    internal CFieldData( FieldInfo _fieldInfo, CTypeData _typeData )
    {
        Field = _fieldInfo;
        m_typeData = _typeData;
        m_fieldName = null;

        var attributeList = _fieldInfo.GetCustomAttributes( false );
        foreach (var attr in attributeList)
        {
            if (attr is ADoNotSerialize)
            {
                DoNotSerialize = true;
            }
            else if (attr is AExplicitlySerialize)
            {
                ExplicitlySerialize = true;
            }
            else if (attr is ASerializedName renamer)
            {
                m_fieldName = renamer.NewName; // This actually overrides an AFieldRenamer on the Parent Type
            }
        }

        if (m_fieldName == null && !m_typeData.m_dynamicFieldRenamer)
            // If we are NOT changing the renaming algorithm at runtime, then-
            m_fieldName = m_typeData.RenameField( Field.Name, Field );
    }


    /// <summary>
    /// Use this method to check to see if this field has a particular attribute
    /// </summary>
    /// <param name="_attributeType">The <see cref="Type"/> of the attribute that the application is looking for</param>
    /// <returns>TRUE if the field has the attribute associated with it.</returns>
    internal bool HasAttribute( Type _attributeType )
    {
        var list = Field.GetCustomAttributes( _attributeType, false );
        return list != null && list.Length > 0;
    }

    /// <summary>
    /// Get the attributes of the given type that are associated with the field.
    /// </summary>
    /// <param name="_attributeType">The Type of the attribute that the application is interested in</param>
    /// <returns>An array of Attributes that meet the Type specified</returns>
    internal object[] GetAttributes( Type _attributeType ) => Field.GetCustomAttributes( _attributeType, false );

    /// <summary>
    /// Get a single attribute of the Type specified as the generic parameter for this method.
    /// </summary>
    /// <typeparam name="TAttributeType">The Type of the attribute requested</typeparam>
    /// <returns>NULL if the attribute was not found, or the first attribute found in the custom attributes
    /// of the requested type.</returns>
    internal TAttributeType GetAttribute<TAttributeType>()
    {
        var list = Field.GetCustomAttributes( typeof( TAttributeType ), false );
        if (list == null || list.Length < 1)
            return default;

        return (TAttributeType)list[0];
    }
}