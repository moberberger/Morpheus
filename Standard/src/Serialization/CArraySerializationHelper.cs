using System;
using System.Text;
using System.Xml;



namespace Morpheus.Serialization
{
    /// <summary>
    /// Helper class to contain the "State" data for a recursive algorithm used to serialize a
    /// multi-dimensional array of objects
    /// </summary>
    internal class CArraySerializationHelper
    {
        private readonly Array m_array;
        private readonly Type m_arrayType;
        private readonly XmlElement m_xmlToAddTo;
        private readonly CSerializer m_serializer;
        private readonly string m_elementName;

        private bool m_skippedNull = false;
        private readonly int[] m_lengths;
        private readonly int[] m_lowerBounds;
        private readonly int[] m_indicies;

        private StringBuilder m_simpleElements = null;

        private bool SimpleElementSerialization => m_simpleElements != null;

        /// <summary>
        /// Construct the recursive serializer with basic information about its environment
        /// </summary>
        /// <param name="_array">The array that its going to serialize</param>
        /// <param name="_xmlToAddTo">The XML node that the array data is to be added to</param>
        /// <param name="_serializer">
        /// The serializer that will handle serialization of array elements
        /// </param>
        internal CArraySerializationHelper( Array _array, XmlElement _xmlToAddTo, CSerializer _serializer )
        {
            m_array = _array;
            m_arrayType = _array.GetType().GetElementType();
            m_xmlToAddTo = _xmlToAddTo;
            m_serializer = _serializer;
            m_elementName = _serializer.GetNameForCollectionElement();

            var rank = m_array.Rank;
            m_lengths = new int[rank];
            m_lowerBounds = new int[rank];
            m_indicies = new int[rank];

            GenerateAttributeInfo();

            EstablishSimpleElementProcessing();
        }

        /// <summary>
        /// "Simple Arrays" are arrays of primitives or strings. These arrays can be turned into
        /// strings containing comma-separated values. Doing so greatly reduces the resulting
        /// dataset, but is less "friendly" to XML processing.
        /// </summary>
        private void EstablishSimpleElementProcessing()
        {
            if (!m_serializer.Context.AllArraysHaveExplicitElements)
            {
                if (m_arrayType.IsPrimitive || m_arrayType == CFramework.TYPEOF_STRING)
                    m_simpleElements = new StringBuilder();
            }
        }

        /// <summary>
        /// Generate lower-bounds and length arrays from the configuration data of the Array.
        /// Add this information as attribute information on the Xml that we're serializing to.
        /// </summary>
        private void GenerateAttributeInfo()
        {
            var rank = m_array.Rank;
            for (var i = 0; i < rank; i++)
            {
                m_lowerBounds[i] = m_array.GetLowerBound( i );
                m_lengths[i] = m_array.GetLength( i );
            }

            var sLengths = CHelper.ConvertArrayToString( m_lengths );
            var slowerBounds = CHelper.ConvertArrayToString( m_lowerBounds );

            m_xmlToAddTo.AddAttribute( m_serializer.Context.ArrayAttributeName, sLengths );
            m_xmlToAddTo.AddAttribute( m_serializer.Context.ArrayLowerBoundAttribute, slowerBounds );
        }

        /// <summary>
        /// Serialize the array using the data that this instance was constructed with.
        /// </summary>
        internal void Serialize()
        {
            AddArrayDimensionToXml( 0 );
            HandleSimpleElements();
        }

        /// <summary>
        /// If there was a simple-element serialization, then add the results to the XML as this
        /// was not done during serialization for performance reasons.
        /// </summary>
        private void HandleSimpleElements()
        {
            if (SimpleElementSerialization)
            {
                if (m_simpleElements.Length > 0)
                {
                    m_simpleElements.Length--;
                    m_xmlToAddTo.InnerText = m_simpleElements.ToString();
                }
            }
        }

        /// <summary>
        /// The recursive method that uses a current "Rank" parameter to determine where in the
        /// recursion we are.
        /// </summary>
        /// <param name="_rank"></param>
        private void AddArrayDimensionToXml( int _rank )
        {
            var lastRank = m_array.Rank - 1;

            for (var i = 0; i < m_lengths[_rank]; i++)
            {
                m_indicies[_rank] = i + m_lowerBounds[_rank];
                if (_rank < lastRank)
                    AddArrayDimensionToXml( _rank + 1 );
                else
                    AddArrayElementToXml();
            }
        }

        /// <summary>
        /// Use the data found in the recursion state (this object) to serialize a single
        /// element in the array to Xml.
        /// </summary>
        private void AddArrayElementToXml()
        {
            var obj = m_array.GetValue( m_indicies );
            // the m_indicies array happens to be in the format needed for Array.GetValue

            if (SimpleElementSerialization)
            {
                if (m_arrayType.IsPrimitive)
                {
                    m_simpleElements.Append( obj.ToString() );
                }
                else // its a string
                {
                    var preEscaped = obj as string;
                    var escaped = CFramework.ProtectStringForStringlist( preEscaped );
                    m_simpleElements.Append( escaped );
                }
                m_simpleElements.Append( "," );
            }
            else if (obj == null && m_serializer.Context.RemoveNullValuesFromXml)
            {
                m_skippedNull = true;
            }
            else
            {
                var elem =
                    m_serializer.FrameworkSerialize( m_elementName,
                                                     obj,
                                                     m_xmlToAddTo,
                                                     m_arrayType );
                if (m_serializer.Context.ArrayElementsIncludeIndicies || m_skippedNull)
                {
                    elem.AddAttribute( m_serializer.Context.ArrayIndexAttributeName,
                                       CHelper.ConvertArrayToString( m_indicies ) );
                }
                m_skippedNull = false;
            }
        }
    }
}
