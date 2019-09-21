using System;
using System.Collections;
using System.Xml;

namespace Morpheus.Serialization
{
    /// <summary>
    /// The SyncStack class is a PRIVATE class in mscorlib that is created when the application wishes
    /// to have a "syncronized" Stack. To get this "sync'ed" version, the application calls
    /// <see cref="Stack.Synchronized"/> with the Stack that synchronization is desired for as the
    /// parameter.
    /// </summary>
    internal class CSyncStackSurrogate : CSystemCollectionsBaseSurrogate<Stack, object>, IExternalSurrogate
    {
        /// <summary>
        /// The Type of a SyncStack- Since this is a private class, its Type has to be figured out using
        /// alternate means.
        /// </summary>
        public static readonly Type SyncStackType;

        /// <summary>
        /// The static constructor's only job is to get the Type of a SyncStack.
        /// </summary>
        static CSyncStackSurrogate()
        {
            var tmp = new Stack( 0 );
            var sync = Stack.Synchronized( tmp );
            SyncStackType = sync.GetType();
        }

        /// <summary>
        /// Serialize an SyncStack using its enumerator
        /// </summary>
        /// <param name="_object">The SyncStack object</param>
        /// <param name="_useType">Treat the "_object" parameter as if it were of this type</param>
        /// <param name="_parentNode">The node which will receive the SyncStack data</param>
        /// <param name="_serializer">The serializer context</param>
        public bool Serialize( object _object, Type _useType, XmlElement _parentNode, CSerializer _serializer ) => BaseSerialize( _object as ICollection, _useType, _parentNode, _serializer );

        /// <summary>
        /// Enumerate through the child nodes of a Stack and deserialize each one. Use the
        /// <see cref="Stack.Push"/> method to add each element to the Stack.
        /// </summary>
        /// <param name="_object">The working object to receive the data about the Stack</param>
        /// <param name="_parentNode">The node containing the elements of the Stack</param>
        /// <param name="_deserializer">The Deserializer controlling this deserializtion</param>
        /// <returns>TRUE always</returns>
        public bool Deserialize( CWorkingObject _object, XmlElement _parentNode, CDeserializer _deserializer ) => SpecialDeserialize( _object, _parentNode, _deserializer );


        /// <summary>
        /// Add an element to a Stack by calling <see cref="Stack.Push"/>
        /// </summary>
        /// <param name="_collection">The Collection to add an element to</param>
        /// <param name="_element">The element to add to the collection</param>
        protected override void AddElement( Stack _collection, object _element ) => _collection.Push( _element );

        /// <summary>
        /// Turn a Stack into a Synchronized Stack
        /// </summary>
        /// <param name="_collection">The collection to turn into something special</param>
        /// <returns>The "Special" collection</returns>
        protected override Stack MakeSpecialCollection( Stack _collection ) => Stack.Synchronized( _collection );

        /// <summary>
        /// Just like the "Stack", the SyncStack needs the ChildNodes in reverse order
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