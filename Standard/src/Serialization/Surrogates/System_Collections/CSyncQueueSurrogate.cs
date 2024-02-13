using System.Collections;
using System.Xml;

#nullable disable

namespace Morpheus.Serialization;


/// <summary>
/// The SyncQueue class is a PRIVATE class in mscorlib that is created when the application wishes
/// to have a "syncronized" Queue. To get this "sync'ed" version, the application calls
/// <see cref="Queue.Synchronized"/> with the Queue that synchronization is desired for as the
/// parameter.
/// </summary>
internal class CSyncQueueSurrogate : CSystemCollectionsBaseSurrogate<Queue, object>, IExternalSurrogate
{
    /// <summary>
    /// The Type of a SyncQueue- Since this is a private class, its Type has to be figured out using
    /// alternate means.
    /// </summary>
    public static readonly Type SyncQueueType;

    /// <summary>
    /// The static constructor's only job is to get the Type of a SyncQueue.
    /// </summary>
    static CSyncQueueSurrogate()
    {
        var tmp = new Queue( 0 );
        var sync = Queue.Synchronized( tmp );
        SyncQueueType = sync.GetType();
    }

    /// <summary>
    /// Serialize an SyncQueue using its enumerator
    /// </summary>
    /// <param name="_object">The SyncQueue object</param>
    /// <param name="_useType">Treat the "_object" parameter as if it were of this type</param>
    /// <param name="_parentNode">The node which will receive the SyncQueue data</param>
    /// <param name="_serializer">The serializer context</param>
    public bool Serialize( object _object, Type _useType, XmlElement _parentNode, CSerializer _serializer ) => BaseSerialize( _object as ICollection, _useType, _parentNode, _serializer );

    /// <summary>
    /// Enumerate through the child nodes of a Queue and deserialize each one. Use the
    /// <see cref="Queue.Enqueue"/> method to add each element to the Queue.
    /// </summary>
    /// <param name="_object">The working object to receive the data about the Queue</param>
    /// <param name="_parentNode">The node containing the elements of the Queue</param>
    /// <param name="_deserializer">The Deserializer controlling this deserializtion</param>
    /// <returns>TRUE always</returns>
    public bool Deserialize( CWorkingObject _object, XmlElement _parentNode, CDeserializer _deserializer ) => SpecialDeserialize( _object, _parentNode, _deserializer );


    /// <summary>
    /// Add an element to a Queue by calling <see cref="Queue.Enqueue"/>
    /// </summary>
    /// <param name="_collection">The Collection to add an element to</param>
    /// <param name="_element">The element to add to the collection</param>
    protected override void AddElement( Queue _collection, object _element ) => _collection.Enqueue( _element );

    /// <summary>
    /// Turn a Queue into a Synchronized Queue
    /// </summary>
    /// <param name="_collection">The collection to turn into something special</param>
    /// <returns>The "Special" collection</returns>
    protected override Queue MakeSpecialCollection( Queue _collection ) => Queue.Synchronized( _collection );
}