using Morpheus.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

namespace Morpheus
{
    /// <summary>
    /// This class provides methods that allow data to be transmuted between a .NET object and
    /// Xml.
    /// </summary>
    /// <remarks>
    /// This class will operate on ALL fields, including protected and private ones. If you
    /// don't like this idea, then don't use this class. Use the .NET Xml serializer instead.
    /// 
    /// <code>
    /// EXAMPLES AND FORMATS
    /// --------------------
    /// class CPerson
    /// {
    ///     string m_name;
    ///     int m_age;
    ///     CAddress m_address;
    ///     string[] m_kidsNames;
    ///     string m_aNullValue = null;
    ///     CAddress m_otherAddress;
    /// }
    /// 
    /// class CAddress
    /// {
    ///     string Street;
    ///     string City;
    ///     int Zip;
    /// }
    /// 
    /// class CSuperAddress : CAddress
    /// {
    ///     string Country;
    /// }
    /// 
    /// ---   yields   ---
    /// 
    /// <CPerson>
    ///     <Name>Homer Simpson</Name>
    ///     <Age>35</Age>
    ///     <Address>
    ///         <Street>45 Mount Horrible</Street>
    ///         <City>Springfield</City>
    ///         <Zip>84372</Zip>
    ///     </Address>
    ///     <KidsNames _Array="3">Bart,Lisa,Maggie</KidsNames>
    ///     <ANullValue Null="true"/>
    ///     <OtherAddress Type="CSuperAddress">
    ///         <Street>88 Schroeder Way</Street>
    ///         <City>Boston</City>
    ///         <Zip>12837</Zip>
    ///         <Country>USA</Country>
    ///     </OtherAddress>
    /// </CPerson>
    /// 
    /// </code>
    /// 
    /// Use the "Helper" class for static methods.
    /// </remarks>
    public class CSerializer : CFramework
    {
        /// <summary>
        /// This table represents a set of all objects that have been referenced. It is used to
        /// prevent multiple-references (performance) and circular references (infinite loops).
        /// </summary>
        private Dictionary<object, XmlElement> m_references;

        /// <summary>
        /// An incrementing ID representing the ID to be assigned to the next reference to be
        /// serialized.
        /// </summary>
        private int m_refId;


        /// <summary>
        /// Default constructor uses the Global context
        /// </summary>
        public CSerializer()
        {
        }

        /// <summary>
        /// Construct with a default context object
        /// </summary>
        /// <param name="_context">The context object to use for the serialization</param>
        public CSerializer( CSerializationContext _context )
            : base( _context )
        {
        }


        /// <summary>
        /// Serialize an object.
        /// </summary>
        /// <param name="_object">The object to serialize</param>
        /// <returns>An XmlDocument containing the serialized object</returns>
        public XmlDocument Serialize( object _object )
        {
            StartProcessing();

            m_references = new Dictionary<object, XmlElement>();
            m_refId = 1;

            var xmlDoc = new XmlDocument();

            FrameworkSerialize( m_context.RootElementName, _object, xmlDoc, null );

            return xmlDoc;
        }


        /// <summary>
        /// Apply all of the rules of the framework to serialize an object into XML and add that
        /// XmlElement to a parent XmlNode.
        /// </summary>
        /// <param name="_nameForElement">
        /// The name to be attached to the element created for this object
        /// </param>
        /// <param name="_object">The object reference that is to be serialized</param>
        /// <param name="_parentNode">
        /// The "parent node" that will receive, as an appended child, the new serialized
        /// object's node
        /// </param>
        /// <param name="_expectedType">
        /// The Type that the object is "expected" to be. If the object is NOT of this type,
        /// then explicit Type information is added.
        /// </param>
        public XmlElement FrameworkSerialize( string _nameForElement, object _object, XmlNode _parentNode, Type _expectedType )
        {
            // Console.WriteLine( "FS: {0}", _nameForElement );
            var newElement = CreateElementForValue( _nameForElement, _object, _parentNode, _expectedType );
            _parentNode.AppendChild( newElement );
            if (!ApplySurrogates( _object, newElement, null ))
                AddObjectToElement( _object, newElement );

            return newElement;
        }


        /// <summary>
        /// Create an XmlElement for an object. Do not populate the new element.
        /// </summary>
        /// <remarks>
        /// FieldRenamers are NOT called at this level- the caller of this method needs to
        /// handle all field renaming
        /// </remarks>
        /// <param name="_elementName">
        /// The name that should be assigned to the newly created element
        /// </param>
        /// <param name="_expectedType">
        /// The expected type of the object. Used to decide whether or not to add explicit Type
        /// data
        /// </param>
        /// <param name="_object">The object that is being serialized</param>
        /// <param name="_parentNode">The XML node that the new node will be a child of</param>
        /// <returns>An XmlElement with some basic attributes for an object</returns>
        private XmlElement CreateElementForValue( string _elementName, object _object, XmlNode _parentNode, Type _expectedType )
        {
            var fixedElementName = FixMemberName( _elementName );
            var elem = CXmlHelper.CreateElement( _parentNode, fixedElementName );

            if (_object == null)
            {
                CXmlHelper.AddAttribute( elem, m_context.NullAttributeName, m_context.NullAttributeValue );
            }
            else
            {
                var oType = _object.GetType();
                oType = CEntityTypeData.StripProxyType( oType );

                if (_expectedType != oType) // There must be a Type attribute added
                    CXmlHelper.AddAttribute( elem, m_context.TypeAttributeName, oType.AssemblyQualifiedName );
            }

            return elem;
        }

        /// <summary>
        /// Apply any surrogates to the object for serialization
        /// </summary>
        /// <param name="_object">The object that may be serialized with a surrogate</param>
        /// <param name="_elementForObject">
        /// The XmlElement that is to receive the object data
        /// </param>
        /// <param name="_useType">
        /// The Type that the object should be treated as. If NULL, use object.GetType()
        /// </param>
        /// <returns>
        /// TRUE if the serialziation was completed by a surrogate, FALSE if the framework
        /// should do its job.
        /// </returns>
        private bool ApplySurrogates( object _object, XmlElement _elementForObject, Type _useType )
        {
            // If nothing is sent in, then do nothing.
            if (_object == null)
                return false;

            // Duplicate References are a "special" kind of built-in surrogate- If this object
            // has been serialized before, then simply refer to that version and return true.
            if (HandleDuplicateReferences( _object, _elementForObject ))
                return true;

            // Setup
            var oType = _useType ?? _object.GetType();
            var isComplete = false;

            // Check if the object is a dynamic proxy created by Entity Framework
            if (CEntityTypeData.UsesEntitySemantics( oType ))
            {
                // There are important conventions and differences when dealing with Entities
                SerializeUsingEntitySemantics( _object, oType, _elementForObject );
                return true;
            }

            // Check the external surrogate
            var externalSurrogate = GetExternalSurrogate( oType );
            if (externalSurrogate != null)
                isComplete = externalSurrogate.Serialize( _object, _useType, _elementForObject, this );
            if (isComplete)
                return true;

            // Check implicit surrogates
            var typeData = CTypeData.GetTypeData( oType );
            var impSurrogate = typeData.ImplicitSurrogate;
            if (impSurrogate != null)
                isComplete = impSurrogate.Serialize( _object, _elementForObject, this );

            return isComplete;
        }


        /// <summary>
        /// This "different" serializer handles Properties, not Fields, and conforms to the
        /// conventions found in Microsoft's Entity Framework
        /// </summary>
        /// <param name="_object"></param>
        /// <param name="_useType"></param>
        /// <param name="_elementForObject"></param>
        private void SerializeUsingEntitySemantics( object _object, Type _useType, XmlElement _elementForObject )
        {
            CXmlHelper.AddAttribute( _elementForObject, m_context.UseEntitySemanticsAttributeName, "1" );

            // Gets (cached) type data pertinent to serializing an Entity
            var typeData = CEntityTypeData.GetTypeData( _useType );

            // First serialize all the "single" properties- No collections
            for (var i = 0; i < typeData.NonCollectionProperties.Length; i++)
            {
                var prop = typeData.NonCollectionProperties[i];

                var val = prop.GetValue( _object );
                FrameworkSerialize( prop.Name, val, _elementForObject, prop.PropertyType );
            }

            // Now serialize all the collections which are presumed to be the "Many" in a
            // Many-To-One relationship. The exact Type of the collection in this object is
            // irrelevant because the collection should merely implement ICollection for the
            // deserialization, and the deserialization target class should determine the exact
            // collection Type, or if its an Interface the PreferredCollectionType on the
            // AUseEntitySemantics attribute will dictate what to create.
            for (var i = 0; i < typeData.CollectionProperties.Length; i++)
            {
                var col = typeData.CollectionProperties[i];
                var collectionElement = CXmlHelper.AddElement( _elementForObject, col.Name );

                Type elementType = null; // set if the PropertyType is a generic collection
                if (col.PropertyType.IsGenericType)
                    elementType = col.PropertyType.GetGenericArguments()[0];

                foreach (var item in col.GetValue( _object ) as System.Collections.IEnumerable)
                {
                    FrameworkSerialize( m_context.ArrayElementName, item, collectionElement, elementType );
                }
            }
        }

        /// <summary>
        /// Check to see if the object has been serialized before. If it has, then make sure
        /// that the reference information for it is added.
        /// </summary>
        /// <remarks>This routine has the overloaded</remarks>
        /// <param name="_object">The object that's being serialized</param>
        /// <param name="_elementForObject">
        /// The element that the object is being serialized to
        /// </param>
        /// <returns>TRUE if the object is a duplicate, FALSE if it is a "new" object</returns>
        private bool HandleDuplicateReferences( object _object, XmlElement _elementForObject )
        {
            var oType = _object.GetType();
            if (oType.IsPrimitive)
                return false;
            if (oType == TYPEOF_STRING && !m_context.DuplicateStringsCanBeReferredTo)
                return false;
            if (oType.IsEnum)
                return false;

            if (m_references.TryGetValue( _object, out var refElem )) // The object was serialized already
            {
                if (ReferenceEquals( _elementForObject, refElem )) // If this IS the referenced element, then skip it
                    return false;

                var refId = CXmlHelper.GetAttributeValue( refElem, m_context.ReferenceIdAttributeName );
                if (refId == null) // The object that was serialized doesn't have its RefID set yet
                {
                    refId = m_refId.ToString();
                    CXmlHelper.AddAttribute( refElem, m_context.ReferenceIdAttributeName, refId );
                    m_refId++;
                }
                // Add the "ReferTo" attribute to the xml
                CXmlHelper.AddAttribute( _elementForObject, m_context.ReferToAttributeName, refId );

                // Remove a "Type" attribute (if it exists) because this becomes redundant with
                // the presence of RefTo
                CXmlHelper.RemoveAttribute( _elementForObject, m_context.TypeAttributeName );

                return true;
            }

            m_references.Add( _object, _elementForObject );
            return false;
        }

        /// <summary>
        /// Add an object to an element. This is the "standard framework way" to serialize an
        /// object.
        /// </summary>
        /// <param name="_object">
        /// The object to be serialized. No assumption made about the object.
        /// </param>
        /// <param name="_elementForObject">
        /// The element that will receive the serialized data
        /// </param>
        public void AddObjectToElement( object _object, XmlElement _elementForObject )
        {
            if (_object == null)
                return;

            var typeOfObject = _object.GetType();

            // If the type is invalid, then do nothing and return.
            if (!IsValidType( typeOfObject ))
                return;

            if (typeOfObject.IsPrimitive || typeOfObject == TYPEOF_STRING || typeOfObject.IsEnum)
                _elementForObject.InnerText = _object.ToString();
            else if (typeOfObject.IsArray)
                AddArrayToXml( _object as Array, _elementForObject );
            else
                AddReferenceTypeToXml( _object, _elementForObject );
        }

        /// <summary>
        /// Add all of the fields of a reference type to the XML. Add all of the base classes of
        /// the type as well.
        /// </summary>
        /// <remarks>
        /// Assume a non-null object. This is a private, thus controlled access, method.
        /// </remarks>
        /// <param name="_object">
        /// The object to be serialized. We know that the object is a ref-type that may have
        /// fields and a base-class
        /// </param>
        /// <param name="_elementForObject">The Element to add data to for this object</param>
        private void AddReferenceTypeToXml( object _object, XmlElement _elementForObject )
        {
            var oType = _object.GetType();
            var typeData = CTypeData.GetTypeData( oType );
            while (typeData != null) // Need to deal with CTypeData for this and all base-classes of this
            {
                AddTypeFieldsToXml( _object, _elementForObject, typeData );
                ClearIgnoredFields();

                var baseTypeData = typeData.BaseType;
                if (baseTypeData != null)
                {
                    // Surrogates were already handled for the first iteration of the loop
                    if (ApplySurrogates( _object, _elementForObject, baseTypeData.Type ))
                        return;
                }
                typeData = baseTypeData;
            }
        }

        /// <summary>
        /// Add all of the fields of a given object identified by a CTypeData-descriptor to the
        /// XML.
        /// </summary>
        /// <param name="_object">The object containing the fields / fieldData</param>
        /// <param name="_elementForObject">
        /// The XML Element to add the data to (each field becomes a childNode)
        /// </param>
        /// <param name="_typeData">
        /// The descriptor which identifies which fields on the _object to add to the XML
        /// </param>
        private void AddTypeFieldsToXml( object _object, XmlElement _elementForObject, CTypeData _typeData )
        {
            foreach (var fieldData in _typeData)
            {
                var fi = fieldData.Field;
                if (IsIgnoredField( fi.Name ))
                    continue;

                var name = GetFieldName( fieldData );
                var expectedType = fi.FieldType;
                var obj = fi.GetValue( _object );

                PushCurrentField( fieldData );
                FrameworkSerialize( name, obj, _elementForObject, expectedType );
                PopField();
            }
        }

        /// <summary>
        /// Use this method when an Array field needs to be added to the XML serialization
        /// </summary>
        /// <remarks>
        /// For element types that are primitives and strings, build a comma-separated list of
        /// values that should end up taking less space. For all other element types, create
        /// child-elements for each array element.
        /// </remarks>
        /// <param name="_array">The array to add to the XML Element</param>
        /// <param name="_elementToAddTo">The XML Element that is to contain the array</param>
        private void AddArrayToXml( Array _array, XmlElement _elementToAddTo )
        {
            if (_array.Rank != 1)
            {
                AddMultiDimensionalArray( _array, _elementToAddTo );
                return;
            }

            var arrayType = _array.GetType();
            var elementType = arrayType.GetElementType();

            var count = _array.Length;
            var lowerBound = _array.GetLowerBound( 0 );
            var upperBound = _array.GetUpperBound( 0 );

            CXmlHelper.AddAttribute( _elementToAddTo, m_context.ArrayAttributeName, count );
            if (lowerBound != 0)
                CXmlHelper.AddAttribute( _elementToAddTo, m_context.ArrayLowerBoundAttribute, lowerBound );

            if (elementType.IsPrimitive && !m_context.AllArraysHaveExplicitElements)
            {
                // Helper doesn't care what the element type is, so we screen it first by making
                // sure its a primitive.
                _elementToAddTo.InnerText = CHelper.ConvertArrayToString( _array );
            }
            else if (elementType == TYPEOF_STRING && !m_context.AllArraysHaveExplicitElements)
            {
                // Strings could theoretically be treated with the same helper used above IF
                // they never contained commas.
                var str = ConvertStringArrayToCommaList( _array );
                _elementToAddTo.InnerText = str;
            }
            else
            {
                var skipped = false;
                var elementName = GetNameForCollectionElement();

                for (var i = lowerBound; i <= upperBound; i++)
                {
                    var arrayElementValue = _array.GetValue( i );

                    if (arrayElementValue == null && m_context.RemoveNullValuesFromXml)
                    {
                        skipped = true;
                    }
                    else
                    {
                        var elem = FrameworkSerialize( elementName, arrayElementValue, _elementToAddTo, elementType );

                        if (m_context.ArrayElementsIncludeIndicies || skipped)
                            CXmlHelper.AddAttribute( elem, m_context.ArrayIndexAttributeName, i );

                        skipped = false;
                    }
                }
            }
        }

        /// <summary>
        /// Add a multi-dimensional array to an element.
        /// </summary>
        /// <param name="_array">The array to add</param>
        /// <param name="_elementToAddTo">The element to add the array to</param>
        private void AddMultiDimensionalArray( Array _array, XmlElement _elementToAddTo )
        {
            var recurser = new CArraySerializationHelper( _array, _elementToAddTo, this );
            recurser.Serialize();
        }
    }
}
