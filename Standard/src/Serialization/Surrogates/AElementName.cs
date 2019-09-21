using System;

namespace Morpheus
{
    /// <summary>
    /// This attribute is used to specify a specific name for elements of a collection when
    /// serializing that collection. This is not used for deserialization
    /// </summary>
    public class AElementName : Attribute
    {
        /// <summary>
        /// Construct the attribute with the name to be used to serialize the collection
        /// </summary>
        /// <param name="_name">The name for elements of the collection</param>
        public AElementName( string _name )
        {
            Name = _name;
        }

        /// <summary>
        /// The name to be used when serializing elements of the collection.
        /// </summary>
        public string Name { get; }
    }
}
