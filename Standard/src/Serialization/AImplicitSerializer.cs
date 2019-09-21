using System;

namespace Morpheus
{
    /// <summary>
    /// This attribute must only be used on a non-static method of a class that is meant to turn
    /// the object into an Xml fragment.
    /// </summary>
    /// <remarks>
    /// The method must take either one or two parameters. The first (and required) parameter is
    /// an XmlElement or an XmlNode that represents the document fragment into which this object
    /// will be serialized. The second (and optional) parameter will be the
    /// <see cref="Serialization.CFramework"/> object that is controlling the serialization.
    /// </remarks>
    [AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
    public sealed class AImplicitSerializer : Attribute
    {
    }
}
