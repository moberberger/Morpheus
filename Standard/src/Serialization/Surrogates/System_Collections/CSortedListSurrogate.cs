using System.Collections;
using System.Xml;

#nullable disable

namespace Morpheus.Serialization;


/// <summary>
/// A SortedList can often be serialized using an enumerator and deserialized using an "Add" method.
/// </summary>
internal class CSortedListSurrogate : CSystemCollectionsBaseSurrogate<SortedList, DictionaryEntry>, IExternalSurrogate
{
    /// <summary>
    /// Serialize a SortedList using its enumerator
    /// </summary>
    /// <param name="_object">The SortedList object</param>
    /// <param name="_useType">Treat the "_object" parameter as if it were of this type</param>
    /// <param name="_parentNode">The node which will receive the SortedList data</param>
    /// <param name="_serializer">The Serializer controlling this serialization</param>
    /// <returns>TRUE always</returns>
    public bool Serialize( object _object, Type _useType, XmlElement _parentNode, CSerializer _serializer ) => BaseSerialize( _object as ICollection, _useType, _parentNode, _serializer );

    /// <summary>
    /// Enumerate through the child nodes of an SortedList and deserialize each one. Use the
    /// <see cref="SortedList.Add"/> method to add each element to the SortedList.
    /// </summary>
    /// <param name="_object">The working object to receive the data about the SortedList</param>
    /// <param name="_parentNode">The node containing the elements of the SortedList</param>
    /// <param name="_deserializer">The Deserializer controlling this deserializtion</param>
    /// <returns>TRUE always</returns>
    public bool Deserialize( CWorkingObject _object, XmlElement _parentNode, CDeserializer _deserializer ) => BasicDeserialize( _object, _parentNode, _deserializer );

    /// <summary>
    /// Add a new entry to a SortedList.
    /// </summary>
    /// <param name="_collection">The SortedList to add the entry to</param>
    /// <param name="_element">The element to add to the collection</param>
    protected override void AddElement( SortedList _collection, DictionaryEntry _element ) => _collection.Add( _element.Key, _element.Value );
}