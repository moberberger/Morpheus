using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

namespace Morpheus.Serialization
{
    /// <summary>
    /// This surrogate will (de)serialize the <see cref="List{T}"/> class.
    /// </summary>
    internal class CGenStackSurrogate : CGenericCollectionsBaseSurrogate
    {
        protected override IEnumerable GetXmlChildren( XmlElement _parent )
        {
            var last = _parent.ChildNodes.Count - 1;
            for (var i = last; i >= 0; i--)
            {
                yield return _parent.ChildNodes[i];
            }
        }

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
            var mi = ct.GetMethod( "Push", _expectedTypes );

            mi.Invoke( _collection, new object[] { o } );
        }
    }
}