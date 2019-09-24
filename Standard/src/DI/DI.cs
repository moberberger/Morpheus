using System;
using System.Collections.Generic;
using System.Text;

using Morpheus.DependencyInjection;

namespace Morpheus
{
    /// <summary>
    /// A Dependency Injection Scope.
    /// </summary>
    public class DI : IDisposable
    {
        /// <summary>
        /// AppDomain Default DI scope. All DI scopes will eventually link down to this scope.
        /// </summary>
        public static readonly DI Default = new DI();

        /// <summary>
        /// Load Morpheus Defaults into the Default scope.
        /// </summary>
        static DI()
        {
        }

        /// <summary>
        /// Create a new DI Scope
        /// </summary>
        /// <returns></returns>
        public static DI New() => new DI( Default );




        /// <summary>
        /// internal lookup table
        /// </summary>
        private readonly Dictionary<Type, ClassConfig> m_typeLookup = new Dictionary<Type, ClassConfig>();

        /// <summary>
        /// If NULL, then this is the <see cref="DI.Default"/> object.
        /// </summary>
        private readonly DI m_parent;

        /// <summary>
        /// Construct with a parent.
        /// </summary>
        /// <param name="_parent"></param>
        private DI( DI _parent = null ) => m_parent = _parent;


        /// <summary>
        /// Get the DI <see cref="ClassConfig"/> for a given <see cref="Type"/>
        /// </summary>
        /// <param name="_type"></param>
        /// <returns></returns>
        public ClassConfig GetClassConfig( Type _type )
        {
            if (m_typeLookup.ContainsKey( _type ))
                return m_typeLookup[_type];

            var fromAncestors = m_parent?.GetClassConfig( _type );
            var newConfig = new ClassConfig( _type, this, fromAncestors );

            m_typeLookup[_type] = newConfig;
            return newConfig;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DI CreateChild()
        {
            return new DI( this );
        }

        /// <summary>
        /// TODO: Dispose of contents of this DI scope
        /// </summary>
        public void Dispose() => m_parent?.Dispose();





        /// <summary>
        /// Lookup Type Metadata
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> the caller is interested in</typeparam>
        /// <returns></returns>
        public ClassConfig For<T>() where T : class => GetClassConfig( typeof( T ) );

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public object Get<T>() where T : class => For<T>().GetObject();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class DI<T> where T : class
    {
        /// <summary>
        /// Get the configuration for a class.
        /// </summary>
        /// <returns>The configuration for a class.</returns>
        public static ClassConfig For() => DI.Default.For<T>();

        /// <summary>
        /// For <see cref="DI.Default"/> , use an object as a singleton.
        /// </summary>
        /// <param name="_singleton"></param>
        public static void Use( object _singleton ) => DI.Default.For<T>().Use( _singleton );

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="Component"></typeparam>
        public static void Use<Component>() where Component : class => DI.Default.For<T>().Use<Component>();

        /// <summary>
        /// More specific than <see cref="Use(object)"/> allows Types to be interpretted as
        /// <see cref="ELifecycle.New"/> .
        /// </summary>
        /// <param name="_type"></param>
        public static void Use( Type _type ) => DI.Default.For<T>().Use( _type );
    }
}
