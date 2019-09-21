using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Morpheus
{
    /// <summary>
    /// This attribute tells the serialization framework to use EntitySemantics when serializing
    /// objects of this class. See documentation in <see cref="Serialization.CEntityTypeData"/>
    /// for more information about EntitySemantics.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = false )]
    public class AUseEntitySemantics : Attribute
    {
        /// <summary>
        /// Exposed for all to see the preferred collection type
        /// </summary>
        public static readonly Type DefaultPreferredCollectionType = typeof( List<> );

        /// <summary>
        /// Use this generic type when deserializing Entities that have collection properties
        /// that are of type ICollection{T}
        /// </summary>
        public Type PreferredCollectionType;

        /// <summary>
        /// Default constructor uses List{} for the preferred collection
        /// </summary>
        public AUseEntitySemantics()
        {
            PreferredCollectionType = DefaultPreferredCollectionType;
        }

        /// <summary>
        /// Construct with a preferred entity type. Should be like "List{}" (no Type specified
        /// within the angle-brackets)
        /// </summary>
        /// <param name="_preferredEntityType">
        /// Should be like "List{}" (no Type specified within the angle-brackets)
        /// </param>
        public AUseEntitySemantics( Type _preferredEntityType )
        {
            PreferredCollectionType = _preferredEntityType;
        }
    }
}
