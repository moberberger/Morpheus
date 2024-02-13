using System.Reflection;

#nullable disable

namespace Morpheus.Serialization;


/// <summary>
/// Contains a form of PropertyInfo for a Type that uses EntitySemantics.
/// </summary>
internal class CPropertyData
{
    /// <summary>
    /// The Name to use for the property in serialization / deserialization. Doesn't 
    /// necessarily match PropertyInfo.Name if a renamer has been employed.
    /// </summary>
    public string Name { get; internal set; }

    /// <summary>
    /// The Type of the Property represented by this object
    /// </summary>
    public Type PropertyType => ReflectedPropertyInfo.PropertyType;

    /// <summary>
    /// The PropertyInfo object for this Property
    /// </summary>
    public PropertyInfo ReflectedPropertyInfo { get; }

    /// <summary>
    /// Must construct with a PropertyInfo object from Reflection
    /// </summary>
    /// <param name="_propInfo">The Reflected information about the property</param>
    internal CPropertyData( PropertyInfo _propInfo )
    {
        ReflectedPropertyInfo = _propInfo;
        Name = ReflectedPropertyInfo.Name;
    }

    /// <summary>
    /// Set the value of the Property
    /// </summary>
    /// <param name="_object">The object to set the value on</param>
    /// <param name="_value">The value of the property to set</param>
    public void SetValue( object _object, object _value ) => ReflectedPropertyInfo.SetValue( _object, _value, null );

    /// <summary>
    /// Get the value of the property on a given object
    /// </summary>
    /// <param name="_object">The object to query for the value</param>
    public object GetValue( object _object ) => ReflectedPropertyInfo.GetValue( _object, null );

}
