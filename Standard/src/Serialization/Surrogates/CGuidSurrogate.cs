using System;
using System.Xml;

namespace Morpheus
{
    /// <summary>
    /// The fields in a guid are numerous and small- the string representation of a guid is much more efficient
    /// </summary>
    internal class CGuidSurrogate : IExternalSurrogate
    {
        /// <summary>
        /// Turn a guid into a string and add that string to the parent node
        /// </summary>
        /// <param name="_object">The GUID object</param>
        /// <param name="_useType">Treat the "_object" parameter as if it were of this type</param>
        /// <param name="_parentNode">The node to place the guid string into</param>
        /// <param name="_serializer">The serializer- not used.</param>
        public bool Serialize( object _object, Type _useType, XmlElement _parentNode, CSerializer _serializer )
        {
            var g = (Guid) _object;
            _parentNode.InnerText = g.ToString();
            return true;
        }

        /// <summary>
        /// Turn the innerText of an XML node into a guid and return that guid
        /// </summary>
        /// <param name="_node">The node containing the GUID string</param>
        /// <param name="_object">The object to deserialize into</param>
        /// <param name="_deserializer">The Deserializer- not used</param>
        /// <returns>a GUID object</returns>
        public bool Deserialize( CWorkingObject _object, XmlElement _node, CDeserializer _deserializer )
        {
            _object.Set( new Guid( _node.InnerText ) );
            return true;
        }
    }
}