using System;
using System.Collections;
using System.Xml;

namespace Morpheus.Serialization
{
    /// <summary>
    /// The SyncHashtable class is a PRIVATE class in mscorlib that is created when the application wishes to have a 
    /// "syncronized" Hashtable. To get this "sync'ed" version, the application calls
    /// <see cref="Hashtable.Synchronized(Hashtable)"/> with the Hashtable that synchronization is desired for as the
    /// parameter.
    /// </summary>
    internal class CSyncHashtableSurrogate : CSystemCollectionsBaseSurrogate<Hashtable, DictionaryEntry>, IExternalSurrogate
    {
        /// <summary>
        /// The Type of a SyncHashtable- Since this is a private class, its Type has to be figured out using
        /// alternate means.
        /// </summary>
        public static readonly Type SyncHashtableType;

        /// <summary>
        /// The static constructor's only job is to get the Type of a SyncHashtable.
        /// </summary>
        static CSyncHashtableSurrogate()
        {
            var tmp = new Hashtable( 0 );
            var sync = Hashtable.Synchronized( tmp );
            SyncHashtableType = sync.GetType();
        }


        /// <summary>
        /// Serialize an SyncHashtable using its enumerator
        /// </summary>
        /// <param name="_object">The working object to receive the data about the Hashtable</param>
        /// <param name="_useType">Treat the "_object" parameter as if it were of this type</param>
        /// <param name="_parentNode">The node containing the elements of the Hashtable</param>
        /// <param name="_serializer">The Deserializer controlling this deserializtion</param>
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
        public bool Deserialize( CWorkingObject _object, XmlElement _parentNode, CDeserializer _deserializer ) => SpecialDeserialize( _object, _parentNode, _deserializer );


        /// <summary>
        /// Add a new DictionaryEntry to a Hashtable
        /// </summary>
        /// <param name="_collection">The Hashtable to receive the new element</param>
        /// <param name="_element">The DictionaryEntry</param>
        protected override void AddElement( Hashtable _collection, DictionaryEntry _element ) => _collection.Add( _element.Key, _element.Value );

        /// <summary>
        /// Turn an ordinary Hashtable into a Synchronized hashtable
        /// </summary>
        /// <param name="_collection">The ordinary HashTable</param>
        /// <returns>The Synchronized hashtable</returns>
        protected override Hashtable MakeSpecialCollection( Hashtable _collection ) => Hashtable.Synchronized( _collection );
    }
}