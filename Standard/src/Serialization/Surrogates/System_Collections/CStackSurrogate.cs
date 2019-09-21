using System;
using System.Collections;
using System.Xml;

namespace Morpheus.Serialization
{
    /// <summary>
    /// An Stack can often be serialized using an enumerator and deserialized using the "Push" method.
    /// </summary>
    internal class CStackSurrogate : CSystemCollectionsBaseSurrogate<Stack, object>, IExternalSurrogate
    {
        /// <summary>
        /// Serialize a Stack using its enumerator
        /// </summary>
        /// <param name="_object">The Stack object</param>
        /// <param name="_useType">Treat the "_object" parameter as if it were of this type</param>
        /// <param name="_parentNode">The node which will receive the Stack data</param>
        /// <param name="_serializer">The Serializer controlling this serialization</param>
        /// <returns>TRUE always</returns>
        public bool Serialize( object _object, Type _useType, XmlElement _parentNode, CSerializer _serializer ) => BaseSerialize( _object as ICollection, _useType, _parentNode, _serializer );

        /// <summary>
        /// Enumerate through the child nodes of an Stack and deserialize each one. Use the
        /// <see cref="Stack.Push"/> method to add each element to the Stack.
        /// </summary>
        /// <param name="_object">The working object to receive the data about the Stack</param>
        /// <param name="_parentNode">The node containing the elements of the Stack</param>
        /// <param name="_deserializer">The Deserializer controlling this deserializtion</param>
        /// <returns>TRUE always</returns>
        public bool Deserialize( CWorkingObject _object, XmlElement _parentNode, CDeserializer _deserializer ) => BasicDeserialize( _object, _parentNode, _deserializer );

        /// <summary>
        /// Add an element to a Stack by calling <see cref="Stack.Push"/>
        /// </summary>
        /// <param name="_collection">The Collection to add an element to</param>
        /// <param name="_element">The element to add to the collection</param>
        protected override void AddElement( Stack _collection, object _element ) => _collection.Push( _element );

        /// <summary>
        /// The Stack actually needs the elements in the reverse order because of the way its
        /// enumerator works when serializing.
        /// </summary>
        /// <param name="_parent"></param>
        /// <returns></returns>
        protected override IEnumerable GetXmlChildren( XmlElement _parent )
        {
            var count = _parent.ChildNodes.Count;
            for (var i = count - 1; i >= 0; i--)
                yield return _parent.ChildNodes[i];
        }
    }
}