using System;

namespace Morpheus
{
    /// <summary>
    /// Placing this attribute on a type's member will allow the code to assign an XPath expression that shall be used to find this member's value with an XML document.
    /// This should be used with <see cref="Xml2Object"/>.
    /// </summary>
    [AttributeUsage( AttributeTargets.Property )]
    public class AXPath : Attribute
    {
        /// <summary>
        /// This is the XPath expression used to find the attributed member's value
        /// </summary>
        public string XPath { get; private set; }

        /// <summary>
        /// Construct with the XPath expression used to find the attributed member's value.
        /// </summary>
        /// <param name="_xpath">The XPath expression used to find the attributed member's value</param>
        public AXPath( string _xpath )
        {
            XPath = _xpath;
        }
    }
}
