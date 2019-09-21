using System;

namespace Morpheus
{
    /// <summary>
    /// This attribute must only be used on a constructor of a class, or on a static method of the class that is meant
    /// to construct the object using an XmlElement. The constructor must take one or three parameters. The first (and 
    /// required) parameter is an XmlElement or an XmlNode that will represent the serialized version of the object. 
    /// The second and third (optional) parameters will be the <see cref="CWorkingObject"/> and <see cref="CDeserializer"/> 
    /// objects that are controlling the deserialization.
    /// 
    /// The return type must be either void or bool. If it is bool, then a TRUE value implies that the object was completely
    /// deserialized, FALSE implies that more work needs to be done by the framework
    /// </summary>
    [AttributeUsage( AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = false )]
    public sealed class AImplicitDeserializer : Attribute
    {
    }
}