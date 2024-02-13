using System.Xml;

#nullable disable

namespace Morpheus;


/// <summary>
/// Optimize the data needed to serialize a <see cref="StringBuilder"/> object.
/// </summary>
public class CStringBuilderSurrogate : IExternalSurrogate
{
    /// <summary>
    /// Turn a DateTime into a string and add that string to the parent node
    /// </summary>
    /// <param name="_object">The DateTime object</param>
    /// <param name="_useType">The Type that _object is to be treated as</param>
    /// <param name="_parentNode">The node to place the DateTime string into</param>
    /// <param name="_serializer">The serializer- not used</param>
    public bool Serialize( object _object, Type _useType, XmlElement _parentNode, CSerializer _serializer )
    {
        _parentNode.InnerText = _object.ToString();
        return true;
    }

    /// <summary>
    /// Turn the innerText of an XML node into a DateTime and return that DateTime
    /// </summary>
    /// <param name="_object">The object to deserialize into</param>
    /// <param name="_node">The node containing the DateTime string</param>
    /// <param name="_deserializer">The serializer- not used</param>
    /// <returns>a DateTime object</returns>
    public bool Deserialize( CWorkingObject _object, XmlElement _node, CDeserializer _deserializer )
    {
        var sb = new StringBuilder( _node.InnerText );
        _object.Set( sb );
        return true;
    }
}