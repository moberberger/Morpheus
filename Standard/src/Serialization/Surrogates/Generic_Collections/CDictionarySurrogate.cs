using System.Collections;
using System.Xml;

#nullable disable

namespace Morpheus.Serialization;


/// <summary>
/// This surrogate will (de)serialize the <see cref="Dictionary{TKey,TValue}"/> class.
/// </summary>
internal class CDictionarySurrogate : CGenericCollectionsBaseSurrogate
{
    /// <summary>
    /// This matches the name of the "key" Field in KeyValuePair
    /// </summary>
    public const string KEY_ELEMENT_NAME = "key";

    /// <summary>
    /// This matches the name of the "value" Field in KeyValuePair
    /// </summary>
    public const string VALUE_ELEMENT_NAME = "value";

    /// <summary>
    /// A Dictionary actually has 2 generic Types in the definition- one for the Key, and one for the Value
    /// </summary>
    protected override int NumberOfExpectedTypes => 2;

    /// <summary>
    /// For serializtion, we need to make sure we're extracting the non-generic <see cref="DictionaryEntry"/> elements
    /// from the collection.
    /// </summary>
    /// <remarks>
    /// By default, the base class would iterate over the Dictionary's default iterator, which would return generic 
    /// KeyValuePair's. The actual Type of these elements would not be known at compile time, so we cannot expect to
    /// see these objects here. Instead, we iterate over the more simplistic IDictionary interface which returns 
    /// the non-generic DictionaryEntry objects.
    /// </remarks>
    /// <param name="_collection">The collection, assumed to be a generic Dictionary.</param>
    /// <returns>An enumeration appropriate for the serialization of the Dictionary</returns>
    protected override IEnumerable GetCollectionElements( object _collection )
    {
        var dict = (IDictionary) _collection;
        foreach (DictionaryEntry entry in dict)
        {
            yield return entry;
        }
    }

    /// <summary>
    /// Add each element of the collection (a <see cref="DictionaryEntry"/> to the Xml
    /// </summary>
    /// <param name="_name">The name for the element</param>
    /// <param name="_element">The element from the collection</param>
    /// <param name="_parentElement">The Xml to receive the element</param>
    /// <param name="_expectedTypes">The Expected Types for the element's components</param>
    /// <param name="_serializer">The serializer handling the serialization process.</param>
    protected override void AddElementToXml( string _name,
                                             object _element,
                                             XmlElement _parentElement,
                                             Type[] _expectedTypes,
                                             CSerializer _serializer )
    {
        var entryElement = _parentElement.AddElement( _name );

        var entry = (DictionaryEntry) _element;

        _serializer.FrameworkSerialize( KEY_ELEMENT_NAME, entry.Key, entryElement, _expectedTypes[0] );
        _serializer.FrameworkSerialize( VALUE_ELEMENT_NAME, entry.Value, entryElement, _expectedTypes[1] );
    }

    /// <summary>
    /// Add an Xml ChildNode to the collection being deserialized
    /// </summary>
    /// <param name="_collection">The collection that's being deserialized</param>
    /// <param name="_xmlElement">The XmlElement containing the data for this element</param>
    /// <param name="_expectedTypes">The expected Type(s) for this element</param>
    /// <param name="_deserializer">The deserialization framework controlling this process</param>
    protected override void AddElementFromXml( object _collection,
                                               XmlElement _xmlElement,
                                               Type[] _expectedTypes,
                                               CDeserializer _deserializer )
    {
        var keyElem = _xmlElement[KEY_ELEMENT_NAME];
        if (keyElem == null)
            throw new XDeserializationError( "Could not find the KEY element for the Dictionary<,> entry." );

        var valueElem = _xmlElement[VALUE_ELEMENT_NAME];
        if (valueElem == null)
            throw new XDeserializationError( "Could not find the VALUE element for the Dictionary<,> entry." );

        var key = _deserializer.FrameworkDeserialize( keyElem, _expectedTypes[0] );
        var value = _deserializer.FrameworkDeserialize( valueElem, _expectedTypes[1] );

        var collection = (IDictionary) _collection;
        collection.Add( key, value );
    }
}