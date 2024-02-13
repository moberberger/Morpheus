using System.Xml;

#nullable disable

namespace Morpheus.Serialization;


/// <summary>
/// This surrogate will (de)serialize the <see cref="Queue{T}"/> class.
/// </summary>
internal class CGenQueueSurrogate : CGenericCollectionsBaseSurrogate
{
    /// <summary>
    /// Add a single XmlElement to a collection, where that XmlElement is a child of the parent Collection element.
    /// </summary>
    /// <param name="_collection"></param>
    /// <param name="_xmlData"></param>
    /// <param name="_expectedTypes"></param>
    /// <param name="_deserializer"></param>
    protected override void AddElementFromXml( object _collection,
                                               XmlElement _xmlData,
                                               Type[] _expectedTypes,
                                               CDeserializer _deserializer )
    {
        var o = _deserializer.FrameworkDeserialize( _xmlData, _expectedTypes[0] );

        var ct = _collection.GetType();
        var mi = ct.GetMethod( "Enqueue", _expectedTypes ) ?? throw new MissingMemberException( "Enqueue" );

        mi.Invoke( _collection, new object[] { o } );
    }
}