using System;
using System.Collections;
using System.Xml;

namespace Morpheus.Serialization
{
    /// <summary>
    /// Contains information on how to serialize and deserialize an ArrayList
    /// </summary>
    internal class CArrayListSurrogate : CSystemCollectionsBaseSurrogate<ArrayList, object>, IExternalSurrogate
    {
        /// <summary>
        /// Serialize an ArrayList using its enumerator
        /// </summary>
        /// <param name="_object">The ArrayList object</param>
        /// <param name="_useType">The Type that _object is to be treated as</param>
        /// <param name="_parentNode">The node which will receive the ArrayList data</param>
        /// <param name="_serializer">The Serializer controlling this serialization</param>
        /// <returns>TRUE always</returns>
        public bool Serialize( object _object, Type _useType, XmlElement _parentNode, CSerializer _serializer ) => BaseSerialize( _object, _useType, _parentNode, _serializer );

        /// <summary>
        /// Enumerate through the child nodes of an ArrayList and deserialize each one. Use the
        /// <see cref="ArrayList.Add"/> method to add each element to the ArrayList.
        /// </summary>
        /// <param name="_object">The working object to receive the data about the arraylist</param>
        /// <param name="_parentNode">The node containing the elements of the ArrayList</param>
        /// <param name="_deserializer">The Deserializer controlling this deserializtion</param>
        /// <returns>TRUE always</returns>
        public bool Deserialize( CWorkingObject _object, XmlElement _parentNode, CDeserializer _deserializer ) => BasicDeserialize( _object, _parentNode, _deserializer );

        /// <summary>
        /// Add an element to an ArrayList
        /// </summary>
        /// <param name="_collection">The ArrayList to add an element to</param>
        /// <param name="_element">The element to add to the collection</param>
        protected override void AddElement( ArrayList _collection, object _element ) => _collection.Add( _element );
    }
}