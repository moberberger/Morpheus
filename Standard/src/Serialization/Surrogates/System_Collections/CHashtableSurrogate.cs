using System;
using System.Collections;
using System.Xml;

namespace Morpheus.Serialization
{
    /// <summary>
    /// A Hashtable can often be serialized using an enumerator and deserialized using an "Add" method.
    /// </summary>
    internal class CHashtableSurrogate : CSystemCollectionsBaseSurrogate<Hashtable, DictionaryEntry>, IExternalSurrogate
    {
        /// <summary>
        /// Serialize an Hashtable using its enumerator
        /// </summary>
        /// <param name="_object">The Hashtable object</param>
        /// <param name="_useType">Treat the "_object" parameter as if it were of this type</param>
        /// <param name="_parentNode">The node which will receive the Hashtable data</param>
        /// <param name="_serializer">The Serializer controlling this serialization</param>
        /// <returns>TRUE always</returns>
        public bool Serialize( object _object, Type _useType, XmlElement _parentNode, CSerializer _serializer ) => BaseSerialize( _object as ICollection, _useType, _parentNode, _serializer );

        /// <summary>
        /// Enumerate through the child nodes of an Hashtable and deserialize each one. Use the
        /// <see cref="Hashtable.Add"/> method to add each element to the Hashtable.
        /// </summary>
        /// <param name="_object">The working object to receive the data about the Hashtable</param>
        /// <param name="_parentNode">The node containing the elements of the Hashtable</param>
        /// <param name="_deserializer">The Deserializer controlling this deserializtion</param>
        /// <returns>TRUE always</returns>
        public bool Deserialize( CWorkingObject _object, XmlElement _parentNode, CDeserializer _deserializer ) => BasicDeserialize( _object, _parentNode, _deserializer );

        /// <summary>
        /// Add a new DictionaryEntry to a Hashtable
        /// </summary>
        /// <param name="_collection">The Hashtable to receive the new element</param>
        /// <param name="_element">The DictionaryEntry</param>
        protected override void AddElement( Hashtable _collection, DictionaryEntry _element ) => _collection.Add( _element.Key, _element.Value );
    }
}