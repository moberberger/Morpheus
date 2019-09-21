using System;

namespace Morpheus
{
    /// <summary>
    /// This attribute instructs the Serializer to NEVER serialize the field or the class that this
    /// is applied to. For fields, this may simply mean that a field has no persisting meaning or 
    /// is very volatile. 
    /// 
    /// For classes, this prevents the framework from serializing the class (maybe due to sensitive 
    /// data, or whatever). This will override the <see cref="AExplicitlySerialize"/> attribute
    /// placed on fields in the class- use that same attribute on the class if that's the behavior
    /// desired.
    /// 
    /// Further, when used on a class, this will prevent any implicit serialization of that class.
    /// Again, if you want to use implicit serialization, use the <see cref="AExplicitlySerialize"/>
    /// attribute.
    /// </summary>
    [AttributeUsage( AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false )]
    public sealed class ADoNotSerialize : Attribute
    {
    }
}