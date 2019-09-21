using System;
using System.Collections;
using System.Xml;

namespace Morpheus.Serialization
{
    /// <summary>
    /// Contains information on how to serialize and deserialize a Queue using <see cref="ICollection"/>
    /// </summary>
    internal class CQueueSurrogate : CSystemCollectionsBaseSurrogate<Queue, object>, IExternalSurrogate
    {
        /// <summary>
        /// Serialize a Queue using its enumerator
        /// </summary>
        /// <param name="_object">The Queue object</param>
        /// <param name="_useType">Treat the "_object" parameter as if it were of this type</param>
        /// <param name="_parentNode">The node which will receive the Queue data</param>
        /// <param name="_serializer">The Serializer controlling this serialization</param>
        /// <returns>TRUE always</returns>
        public bool Serialize( object _object, Type _useType, XmlElement _parentNode, CSerializer _serializer ) => BaseSerialize( _object, _useType, _parentNode, _serializer );

        /// <summary>
        /// Enumerate through the child nodes of an Queue and deserialize each one. Use the
        /// <see cref="Queue.Enqueue"/> method to add each element to the Queue.
        /// </summary>
        /// <param name="_object">The working object to receive the data about the Queue</param>
        /// <param name="_parentNode">The node containing the elements of the Queue</param>
        /// <param name="_deserializer">The Deserializer controlling this deserializtion</param>
        /// <returns>TRUE always</returns>
        public bool Deserialize( CWorkingObject _object, XmlElement _parentNode, CDeserializer _deserializer ) => BasicDeserialize( _object, _parentNode, _deserializer );

        /// <summary>
        /// Add an element to a Queue by calling <see cref="Queue.Enqueue"/>
        /// </summary>
        /// <param name="_collection">The Collection to add an element to</param>
        /// <param name="_element">The element to add to the collection</param>
        protected override void AddElement( Queue _collection, object _element ) => _collection.Enqueue( _element );
    }
}