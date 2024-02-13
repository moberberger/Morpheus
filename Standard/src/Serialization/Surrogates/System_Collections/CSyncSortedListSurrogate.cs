using System.Collections;
using System.Xml;

#nullable disable

namespace Morpheus.Serialization;


/// <summary>
/// The SyncSortedList class is a PRIVATE class in mscorlib that is created when the application wishes
/// to have a "syncronized" SortedList. To get this "sync'ed" version, the application calls
/// <see cref="SortedList.Synchronized"/> with the SortedList that synchronization is desired for as the
/// parameter.
/// </summary>
internal class CSyncSortedListSurrogate : CSystemCollectionsBaseSurrogate<SortedList, DictionaryEntry>, IExternalSurrogate
{
    /// <summary>
    /// The Type of a SyncSortedList- Since this is a private class, its Type has to be figured out using
    /// alternate means.
    /// </summary>
    public static readonly Type SyncSortedListType;

    /// <summary>
    /// The static constructor's only job is to get the Type of a SyncSortedList.
    /// </summary>
    static CSyncSortedListSurrogate()
    {
        var tmp = new SortedList( 0 );
        var sync = SortedList.Synchronized( tmp );
        SyncSortedListType = sync.GetType();
    }

    /// <summary>
    /// Serialize an SyncSortedList using its enumerator
    /// </summary>
    /// <param name="_object">The SyncSortedList object</param>
    /// <param name="_useType">Treat the "_object" parameter as if it were of this type</param>
    /// <param name="_parentNode">The node which will receive the SyncSortedList data</param>
    /// <param name="_serializer">The serializer context</param>
    public bool Serialize( object _object, Type _useType, XmlElement _parentNode, CSerializer _serializer ) => BaseSerialize( _object as ICollection, _useType, _parentNode, _serializer );

    /// <summary>
    /// Enumerate through the child nodes of a SortedList and deserialize each one. Use the
    /// <see cref="SortedList.Add"/> method to add each element to the SortedList.
    /// </summary>
    /// <param name="_object">The working object to receive the data about the SortedList</param>
    /// <param name="_parentNode">The node containing the elements of the SortedList</param>
    /// <param name="_deserializer">The Deserializer controlling this deserializtion</param>
    /// <returns>TRUE always</returns>
    public bool Deserialize( CWorkingObject _object, XmlElement _parentNode, CDeserializer _deserializer ) => SpecialDeserialize( _object, _parentNode, _deserializer );


    /// <summary>
    /// Add a new entry to a SortedList.
    /// </summary>
    /// <param name="_collection">The SortedList to add the entry to</param>
    /// <param name="_element">The element to add to the collection</param>
    protected override void AddElement( SortedList _collection, DictionaryEntry _element ) => _collection.Add( _element.Key, _element.Value );

    /// <summary>
    /// Return a Sync'ed SortedList from an ordinary one
    /// </summary>
    /// <param name="_collection">The SortedList that needs to be synchronized</param>
    /// <returns>A Sync'ed version of the collection</returns>
    protected override SortedList MakeSpecialCollection( SortedList _collection ) => SortedList.Synchronized( _collection );
}