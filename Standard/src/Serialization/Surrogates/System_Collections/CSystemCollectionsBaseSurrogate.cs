using System.Collections;
using System.Xml;

#nullable disable

namespace Morpheus.Serialization;


/// <summary>
/// This is the base class for the <see cref="System.Collections"/> collections supported by the framework.
/// </summary>
internal abstract class CSystemCollectionsBaseSurrogate<TCollectionType, TElementType> where TCollectionType : ICollection, new()
{
    /// <summary>
    /// This janky P.O.S. code is to fix the bug in SyncHashtable and SyncSortedList that prevents the proper
    /// enumeration using IEnumerable and ICollection- By casting to IDictionary, we can get the "Real" enumerator
    /// </summary>
    /// <param name="_collection">The collection that needs to be enumerated</param>
    /// <returns>A "proper" enumerator for the collection, based on the BUG in the MSCORLIB code</returns>
    private static IEnumerator GetEnumerator( object _collection )
    {
        if (_collection is IDictionary d)
            return d.GetEnumerator();

        var e = _collection as IEnumerable;
        return e.GetEnumerator();
    }

    /// <summary>
    /// Serialize the enumeration, serializing each element 
    /// </summary>
    /// <param name="_collection">The collection that is to be enumerated</param>
    /// <param name="_parentNode">The XmlNode to receive the elements</param>
    /// <param name="_serializer">The framework controlling the serialization</param>
    private static void SerializeEnumeration( object _collection, XmlElement _parentNode, CSerializer _serializer )
    {
        var elementName = _serializer.GetNameForCollectionElement();
        var e = GetEnumerator( _collection );

        while (e.MoveNext())
        {
            var element = e.Current;
            _serializer.FrameworkSerialize( elementName, element, _parentNode, typeof( TElementType ) );
        }
    }


    /// <summary>
    /// Serialize an ICollection using the enumerator and the Synchronization hints
    /// </summary>
    /// <param name="_collection">The collection to serialize</param>
    /// <param name="_useType">Treat the "_object" parameter as if it were of this type</param>
    /// <param name="_parentNode">The Xml node to receive the elements of the collection</param>
    /// <param name="_serializer">The serializer used to serialize the collection</param>
    /// <returns>TRUE always</returns>
    protected static bool BaseSerialize( object _collection, Type _useType, XmlElement _parentNode, CSerializer _serializer )
    {
        if (!ATreatAsInterface.TreatAsInterface( _serializer ))
            return false;

        var collection = (TCollectionType) _collection;
        var useSynchronization = collection.IsSynchronized;

        if (useSynchronization)
            Monitor.Enter( collection.SyncRoot );

        try
        {
            SerializeEnumeration( collection, _parentNode, _serializer );
        }
        finally
        {
            if (useSynchronization)
                Monitor.Exit( collection.SyncRoot );
        }

        return true;
    }


    /// <summary>
    /// Deserialization helper to deserialize XML nodes into a "special" collection.
    /// </summary>
    /// <remarks>
    /// A "special" collection is one of the "hidden" collections that implement Synchronized or ReadOnly
    /// behavior for the System.Collections collections.
    /// </remarks>
    /// <param name="_object">The working object to receive the deserialized values</param>
    /// <param name="_node">The XmlNode containing the data for the collection</param>
    /// <param name="_deserializer">The deserialization framework performing the overall deserialization</param>
    /// <returns>TRUE always</returns>
    protected bool SpecialDeserialize( CWorkingObject _object, XmlElement _node, CDeserializer _deserializer )
    {
        if (!ATreatAsInterface.TreatAsInterface( _deserializer ))
            return false;

        // First, get an object to deserialize into
        var tmp = new CWorkingObject();
        if (_object.WorkingObject != null)
            tmp.Set( _object.WorkingObject );

        // Deserialize into the temporary WorkingObject
        BasicDeserialize( tmp, _node, _deserializer );

        // At this point, IF there was an object set in _object prior to this routine, its contents
        //  should have been set by the standard collection deserialization and we should be done.
        if (_object.IsSet)
            return true;

        // Get the collection object populated so far with information from the Xml
        var tmpCollection = tmp.GetWorkingObject<TCollectionType>();

        // Using that object, create a new, syncronized version of that ArrayList.
        var specialCollection = MakeSpecialCollection( tmpCollection );

        // Set this sync'ed version to the _object, and we're done
        _object.Set( specialCollection );
        return true;
    }

    /// <summary>
    /// Enumerate through the child nodes of the collection and deserialize each one. Use the
    /// method provided to add each element to the collection.
    /// </summary>
    /// <param name="_object">The working object to receive the data about the Stack</param>
    /// <param name="_parentNode">The node containing the elements of the Stack</param>
    /// <param name="_deserializer">The Deserializer controlling this deserializtion</param>
    /// <returns>TRUE always</returns>
    protected bool BasicDeserialize( CWorkingObject _object, XmlElement _parentNode, CDeserializer _deserializer )
    {
        if (!ATreatAsInterface.TreatAsInterface( _deserializer ))
            return false;

        var theCollection = _object.GetExistingOrCreateNew<TCollectionType>();

        foreach (XmlElement element in GetXmlChildren( _parentNode ))
        {
            var theElement = (TElementType) _deserializer.FrameworkDeserialize( element, typeof( TElementType ) );
            AddElement( theCollection, theElement );
        }

        return true;
    }


    /// <summary>
    /// The standard enumeration for xml children is a "forwards" enumeration over the ChildNodes
    /// </summary>
    /// <param name="_parent">The Xml node that contains all of the individual elements of the collection as ChildNodes</param>
    /// <returns>An iterator over those element-nodes</returns>
    protected virtual IEnumerable GetXmlChildren( XmlElement _parent )
    {
        foreach (XmlElement element in _parent.ChildNodes)
        {
            yield return element;
        }
    }

    /// <summary>
    /// Overridden by the subtype to add an element to the collection once that element has been deserialized
    /// </summary>
    /// <param name="_collection">The collection to add the item to</param>
    /// <param name="_element">The element to add to the collection</param>
    protected virtual void AddElement( TCollectionType _collection, TElementType _element ) => throw new NotImplementedException( "Never call AddElement for a class that isn't 'Special'." );

    /// <summary>
    /// Turn a collection into a "Special" collection- Synchronized or ReadOnly, etc.
    /// </summary>
    /// <param name="_collection">The collection to turn into something special</param>
    /// <returns>The "Special" collection</returns>
    protected virtual TCollectionType MakeSpecialCollection( TCollectionType _collection ) => throw new NotImplementedException( "Never call MakeSpecialCollection for a class that isn't 'Special'." );
}