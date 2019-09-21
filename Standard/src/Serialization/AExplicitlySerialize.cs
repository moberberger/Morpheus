using System;

namespace Morpheus
{
    /// <summary>
    /// This attribute instructs the Serializer to ALWAYS serialize the field -or- to only 
    /// serialize fields of a class that have this attribute on them.
    /// </summary>
    /// <remarks>
    /// This attribute will signal the fields that DO get serialized.
    /// 
    /// When this attribute is used on a class, then the framework will only serialize fields 
    /// from that class that have this attribute on them.
    /// </remarks>
    [AttributeUsage( AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = false )]
    public sealed class AExplicitlySerialize : Attribute
    {
    }
}