using System;
using System.Collections;
using System.Linq;
using System.Xml;

namespace Morpheus.Serialization
{
    /// <summary>
    /// This base class provides the logic for the serialization and deserialization of generic collections. It relies on
    /// virtual methods to do much of the specialized work for any particular collection type.
    /// </summary>
    internal abstract class CGenericCollectionsBaseSurrogate : IExternalSurrogate
    {
        /// <summary>
        /// Most collections have exactly 1 expected generic parameter Type, but not all (see Dictionary)
        /// </summary>
        protected virtual int NumberOfExpectedTypes => 1;

        /// <summary>
        /// Add, using either default logic (this method) or subclass logic (overridden in that subclass), add an element 
        /// to the parent Xml.
        /// </summary>
        /// <param name="_name"></param>
        /// <param name="_element"></param>
        /// <param name="_parentElement"></param>
        /// <param name="_expectedTypes"></param>
        /// <param name="_serializer"></param>
        protected virtual void AddElementToXml( string _name,
                                                object _element,
                                                XmlElement _parentElement,
                                                Type[] _expectedTypes,
                                                CSerializer _serializer ) => _serializer.FrameworkSerialize( _name, _element, _parentElement, _expectedTypes[0] );

        /// <summary>
        /// The standard enumeration over the collection using the default "IEnumerable" implementation.
        /// </summary>
        /// <param name="_collection">The collection to enumerate over</param>
        /// <returns>The enumeration for the collection</returns>
        protected virtual IEnumerable GetCollectionElements( object _collection ) => (IEnumerable) _collection;

        /// <summary>
        /// The standard enumeration for xml children is a "forwards" enumeration over the ChildNodes
        /// </summary>
        /// <param name="_parent">The Xml node that contains all of the individual elements of the collection as ChildNodes</param>
        /// <returns>An iterator over those element-nodes</returns>
        protected virtual IEnumerable GetXmlChildren( XmlElement _parent ) => _parent.ChildNodes;

        /// <summary>
        /// Add a single XmlElement to a collection, where that XmlElement is a child of the parent Collection element.
        /// </summary>
        /// <param name="_collection"></param>
        /// <param name="_xmlData"></param>
        /// <param name="_expectedTypes"></param>
        /// <param name="_deserializer"></param>
        protected abstract void AddElementFromXml( object _collection,
                                                   XmlElement _xmlData,
                                                   Type[] _expectedTypes,
                                                   CDeserializer _deserializer );

        /// <summary>
        /// Serialize a generic collection to a parent Xml element
        /// </summary>
        /// <param name="_object">The object (collection) to serialize</param>
        /// <param name="_useType">Treat the "_object" parameter as if it were of this type</param>
        /// <param name="_parentElement">The XML element that is to receive the collection as Xml data</param>
        /// <param name="_serializer">The Serializer framework object in charge of serializing the collection</param>
        /// <returns>TRUE unless the collection is not supposed to be treated like an "interface"</returns>
        public bool Serialize( object _object, Type _useType, XmlElement _parentElement, CSerializer _serializer )
        {
            if (!ATreatAsInterface.TreatAsInterface( _serializer ))
                return false;

            var oType = _useType;
            if (oType == null)
                oType = _object.GetType();

            if (oType == null)
            {
                throw new XSerializationError(
                    "When serializing a generic collection, the actual Type of the collection could not be found." );
            }

            var expectedTypes = oType.GetGenericArguments();
            if (expectedTypes.Length != NumberOfExpectedTypes)
            {
                throw new XSerializationError( "The Type '" + oType.FullName + "' has " + expectedTypes.Length +
                                               " generic arguments when it is required to have exactly " + NumberOfExpectedTypes + "." );
            }

            var elementName = _serializer.GetNameForCollectionElement();

            foreach (var element in GetCollectionElements( _object ))
            {
                AddElementToXml( elementName, element, _parentElement, expectedTypes, _serializer );
            }

            return true;
        }


        /// <summary>
        /// Deserialize a series of "ChildNode"s from an XmlElement as elements for a generic Dictionary.
        /// </summary>
        /// <param name="_workingObject">The "working" object to receive the dictionary elements</param>
        /// <param name="_parentElement">The "parent" of the XmlElements containing the individual Dictionary elements</param>
        /// <param name="_deserializer">The deserialization framework handing the deserialization</param>
        /// <returns>TRUE unless the collection is not supposed to be treated like an "interface"</returns>
        public bool Deserialize( CWorkingObject _workingObject, XmlElement _parentElement, CDeserializer _deserializer )
        {
            if (!ATreatAsInterface.TreatAsInterface( _deserializer ))
                return false;

            var oType = _workingObject.WorkingType;
            if (oType == null)
                oType = _deserializer.GetExpectedType( _parentElement );
            if (oType == null)
            {
                throw new XDeserializationError(
                    "When deserializing a generic collection, the actual Type of the collection could not be found." );
            }

            var expectedTypes = oType.GetGenericArguments();
            if (expectedTypes.Length != NumberOfExpectedTypes)
            {
                throw new XDeserializationError( "The Type '" + oType.FullName + "' has " + expectedTypes.Length +
                                                 " generic arguments when it is required to have exactly " + NumberOfExpectedTypes + "." );
            }

            var collection = _workingObject.GetExistingOrCreateNew( oType );
            var elementName = _deserializer.GetNameForCollectionElement();

            foreach (XmlElement xmlElement in GetXmlChildren( _parentElement ))
            {
                if (xmlElement.Name == elementName)
                    AddElementFromXml( collection, xmlElement, expectedTypes, _deserializer );
            }

            return true;
        }
    }
}