using Morpheus.Serialization;
using System;


namespace Morpheus
{
    /// <summary>
    /// This attribute is optional, and instructs the serialization framework whether or not to treat a field
    /// as an interface even though it may be declared as a specific implementation class (Hashtable vs IDictionary).
    /// The distinction is important because, when treated as an Interface, none of the "extra" fields governing
    /// the specific implementation are included (Capacity, Comparers, etc).
    /// </summary>
    [AttributeUsage( AttributeTargets.Field, AllowMultiple = false )]
    public class ATreatAsInterface : Attribute
    {
        /// <summary>
        /// Specifiy with this attribute whether or not "interface serialization" should be used rather than
        /// serialization using the actual class.
        /// </summary>
        /// <param name="_useSpecialSerialization">TRUE will result in more concise serialization based on the 
        /// Interface for collections.</param>
        public ATreatAsInterface( bool _useSpecialSerialization )
        {
            UseSpecialSerialization = _useSpecialSerialization;
        }

        /// <summary>
        /// When TRUE, use the Interface rather than the implementation for collections.
        /// </summary>
        public bool UseSpecialSerialization { get; } = true;

        /// <summary>
        /// Helper function that will allow a surrogate to check to see if a collection should be treated
        /// as if it were an interface as opposed to being treated as a full class.
        /// </summary>
        /// <param name="_framework">The (de)serialization framework in charge of this operation</param>
        /// <returns>TRUE if the collection SHOULD be treated as if it were a simple interface</returns>
        public static bool TreatAsInterface( CFramework _framework )
        {
            var attr = _framework.GetAttribute<ATreatAsInterface>();
            if (attr == null)
                return true;

            return attr.UseSpecialSerialization;
        }
    }
}