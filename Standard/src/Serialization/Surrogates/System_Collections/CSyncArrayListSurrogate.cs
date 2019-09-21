using System;
using System.Collections;
using System.Xml;

namespace Morpheus.Serialization
{
    /// <summary>
    /// The SyncArrayList class is a PRIVATE class in mscorlib that is created when the application wishes to have a "syncronized" 
    /// ArrayList. To get this "sync'ed" version, the application calls <see cref="ArrayList.Synchronized(ArrayList)"/> with the 
    /// ArrayList that synchronization is desired for as the parameter.
    /// </summary>
    internal class CSyncArrayListSurrogate : CSystemCollectionsBaseSurrogate<ArrayList, object>, IExternalSurrogate
    {
        /// <summary>
        /// The Type of a SyncArrayList- Since this is a private class, its Type has to be figured out using
        /// alternate means.
        /// </summary>
        public static readonly Type SyncArrayListType;

        /// <summary>
        /// The static constructor's only job is to get the Type of a SyncArrayList.
        /// </summary>
        static CSyncArrayListSurrogate()
        {
            var tmp = new ArrayList( 0 );
            var sync = ArrayList.Synchronized( tmp );
            SyncArrayListType = sync.GetType();
        }


        /// <summary>
        /// Serialize an SyncArrayList using its enumerator
        /// </summary>
        /// <param name="_object">The working object to receive the data about the arraylist</param>
        /// <param name="_useType">Treat the "_object" parameter as if it were of this type</param>
        /// <param name="_parentNode">The node containing the elements of the ArrayList</param>
        /// <param name="_serializer">The Deserializer controlling this deserializtion</param>
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
        public bool Deserialize( CWorkingObject _object, XmlElement _parentNode, CDeserializer _deserializer ) => SpecialDeserialize( _object, _parentNode, _deserializer );

        /// <summary>
        /// Add an element to an ArrayList
        /// </summary>
        /// <param name="_collection">The ArrayList to add an element to</param>
        /// <param name="_element">The element to add to the collection</param>
        protected override void AddElement( ArrayList _collection, object _element ) => _collection.Add( _element );

        /// <summary>
        /// Turn an ArrayList into a synchronized ArrayList
        /// </summary>
        /// <param name="_collection">The standard ArrayList</param>
        /// <returns>A Synchronized ArrayList</returns>
        protected override ArrayList MakeSpecialCollection( ArrayList _collection ) => ArrayList.Synchronized( _collection );
    }
}