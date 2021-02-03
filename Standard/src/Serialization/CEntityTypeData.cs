using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Morpheus.Serialization
{
    /// <summary>
    /// This class encapsulates reflected information about an Entity (see Entity Framework)
    /// </summary>
    /// <remarks>
    /// About EntitySemantics:
    /// 
    /// The largest difference is that Properties are used in addition to Fields. Also, because
    /// Entities must conform to numerous EntityFramework rules, the serializer doesn't give as
    /// much control to the application when serializing. Public Fields will be handled, as
    /// Unity3d's editor deals with Public Fields instead of Properties. However, the
    /// application needs to be aware that Microsoft's Entity Framework does not "handle"
    /// fields- it only handles properties.
    /// 
    /// The goal is to create a serialization that ignores all Proxy information (from the
    /// EntityFramework) and can be DeSerialized into a "vanilla" version of the Entity (sans
    /// Proxy stuff). While the typical method of operation will have the server and client use
    /// the same Assembly with the same Data Entities, allowances are made for a situation where
    /// a client needs a "Unity3d Friendly" version of the client object.
    /// 
    /// With the exception of <see cref="ADoNotSerialize"/> and <see cref="ASerializedName"/> ,
    /// none of the other attributes are honored for Entities. Namely, Surrogates are not
    /// supported (yet), as Entities are supposed to be of a very basic data-representation
    /// form. It would be easy to support Surrogates, but philosophically I believe its more
    /// important that Data Entities stay true to form.
    /// 
    /// Properties and Fields will only be serialized if: (a) They inherit from
    /// <see cref="System.Collections.ICollection"/> -or- (b) They inherit from
    /// <see cref="ICollection"/> (c) Collection Properties must have public Getter (d)
    /// Non-collection Properties must have public Getter AND Setter (e) Fields must be Public
    /// 
    /// Even though I decided to support Fields AND Properties. I expect the target Type to
    /// still be "Entity-Like", in that its purpose needs to stay true to a "Data Representation
    /// Only" class (as opposed to, say, the System.IO.File class, which represents more of an
    /// interfacing mechanism and not so much a Data Representatino). Unlike normal
    /// De/serialization, ONLY public fields will be de/serialized, as the sole reason for this
    /// allowance (fields instead of properties on de/serialization) is that the Unity Editor
    /// only exposes Public Fields to the designer.
    /// </remarks>
    internal class CEntityTypeData
    {
        #region Cache Handling / Static Functions
        /// <summary>
        /// Cache so that we don't have to re-figure-out Types more than once. If the key's
        /// value is NULL, we have tested the Type for EntitySemantics and found that the Type
        /// should NOT use EntitySemantics.
        /// </summary>
        private static readonly Dictionary<Type, CEntityTypeData> sm_cache =
            new Dictionary<Type, CEntityTypeData>();

        /// <summary>
        /// Given a Type, return its CEntityTypeData. If the Type has been seen before, a cached
        /// version will be returned. This ASSUMES that _type will use EntitySemantics. To
        /// check, use <see cref="UsesEntitySemantics"/>
        /// </summary>
        /// <param name="_type">The Type to get information about</param>
        /// <returns>New Entity-Meaningful TypeData for an Entity</returns>
        internal static CEntityTypeData GetTypeData( Type _type )
        {
            _type = StripProxyType( _type );

            if (!sm_cache.TryGetValue( _type, out var retval ))
            {
                retval = new CEntityTypeData( _type );
                sm_cache[_type] = retval;
            }
            return retval;
        }

        /// <summary>
        /// The determiner of whether a Type is to use EntitySemantics
        /// </summary>
        /// <param name="_type"></param>
        /// <returns></returns>
        internal static bool UsesEntitySemantics( Type _type )
        {
            // If this is a proxy class, then by default use entity semantics
            if (IsProxyClass( _type ))
                return true;

            // If the Type is already in the cache, then its value (being NULL or not) tells us
            // whether or not it should use EntitySemantics.
            if (sm_cache.TryGetValue( _type, out var cached ))
                return cached != null;

            // If the Type is
            var attr = Lib.GetSingleAttribute<AUseEntitySemantics>( _type );
            if (attr != null)
                sm_cache[_type] = new CEntityTypeData( _type, attr );
            else
                sm_cache[_type] = null;

            return attr != null;
        }

        /// <summary>
        /// Helper function to get an Entity's Type sans the Proxy
        /// </summary>
        /// <param name="_type">The type to check</param>
        /// <returns></returns>
        internal static Type StripProxyType( Type _type )
        {
            if (IsProxyClass( _type ))
                return _type.BaseType;
            else
                return _type;
        }

        /// <summary>
        /// The check that determines if a Type is a Proxy Class. Does not check to see if the
        /// Type should be handled with EntitySemantics.
        /// </summary>
        /// <param name="_type">The Type to check for Proxy status</param>
        /// <returns>TRUE if the Type is an EntityFramework Proxy</returns>
        internal static bool IsProxyClass( Type _type ) => _type.Namespace == "System.Data.Entity.DynamicProxies";
        #endregion

        public CPropertyData[] NonCollectionProperties { get; private set; }
        public CPropertyData[] CollectionProperties { get; private set; }
        public static Type Sm_typeofGenericICollection { get; } = typeof( ICollection<> );
        public static Type Sm_typeofBasicICollection { get; } = typeof( ICollection );
        public Type PreferredCollectionType { get; } = AUseEntitySemantics.DefaultPreferredCollectionType;

        /// <summary>
        /// Basically construct the entire pertinent information for an Entity Type
        /// </summary>
        /// <param name="_type">
        /// The Type of the entity- If this is a Proxy type (the Namespace is ""), then the
        /// BaseType will be taken instead
        /// </param>
        /// <param name="_attribute">The UseEntitySemantics attribute, if available</param>
        private CEntityTypeData( Type _type, AUseEntitySemantics _attribute = null )
        {
            if (_attribute != null)
                PreferredCollectionType = _attribute.PreferredCollectionType;

            var properties = _type.GetProperties(
                BindingFlags.FlattenHierarchy |     // Entities aren't considered hierarchies
                BindingFlags.Instance |             // Entities don't save static properties
                BindingFlags.Public );              // Entities only use Public Properties

            var collectionProperties = new List<CPropertyData>();
            var nonCollectionProperties = new List<CPropertyData>();

            for (var i = 0; i < properties.Length; i++) // much faster than foreach on Arrays
            {
                var pi = properties[i];

                // must have getter and setter
                if (!pi.CanRead || !pi.CanWrite)
                    continue;

                // The only attribute we respect is the DoNotSerialize attribute
                if (pi.GetSingleAttribute<ADoNotSerialize>() != null)
                    continue;

                var pd = new CPropertyData( pi );
                var differentName = pi.GetSingleAttribute<ASerializedName>();
                if (differentName != null)
                    pd.Name = differentName.NewName;

                // Figure out where to put the property
                if (pi.PropertyType.ImplementsInterface( typeof( ICollection<> ) ))
                {
                    collectionProperties.Add( pd );
                }
                else // Add other conditions here as needed, otherwise the fall-through is...
                {
                    nonCollectionProperties.Add( pd );
                }
            }

            NonCollectionProperties = nonCollectionProperties.ToArray();
            CollectionProperties = collectionProperties.ToArray();
        }

    }
}
