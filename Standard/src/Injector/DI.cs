using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

using Morpheus.DependencyInjection;

namespace Morpheus
{
    /// <summary>
    /// A Dependency Injection Scope.
    /// </summary>
    public class DI
    {
        #region Static stuff for Morpheus integration
        /// <summary>
        /// AppDomain Default DI scope. All DI scopes will eventually link down to this scope.
        /// </summary>
        public static DI Default { get; } = new DI( null );

        /// <summary>
        /// Load Morpheus Defaults into the Default scope.
        /// </summary>
        static DI()
        {
            Default.For<Random>().UseNewInstance<CryptoRandomNumbers>();
            Default.For<LCPRNG>().UseNewInstance<LCPRNG_MMIX>();
        }
        #endregion



        /// <summary>
        /// internal lookup table
        /// </summary>
        // private readonly EncapsulatingDictionary
        // <Type, TypeConfig> m_typeLookup = null;
        private readonly Dictionary<Type, TypeConfig> m_typeLookup = new();

        public DI Parent => m_parentDI;
        private readonly DI m_parentDI;

        /// <summary>
        /// Construct with a parent. Must be called by <see cref="DI.New"/> - App cannot
        /// construct directly.
        /// </summary>
        /// <param name="parent"></param>
        private DI( DI parent ) => m_parentDI = parent;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DI New() => new DI( this );


        /// <summary>
        /// Informs about whether or not this DI contains information about a type. Unlike the
        /// <see cref="Get"/> method, this operation is a C++-style "const" method.
        /// </summary>
        /// <param name="m_type"></param>
        /// <returns></returns>
        public bool Contains( Type m_type ) => m_typeLookup.ContainsKey( m_type );


        /// <summary>
        /// Lookup Type Metadata
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> the caller is interested in</typeparam>
        /// <returns></returns>
        public TypeConfig For<T>() => GetTypeConfig( typeof( T ) );


        /// <summary>
        /// Get the DI <see cref="TypeConfig"/> for a given <see cref="Type"/>
        /// </summary>
        /// <param name="_type"></param>
        /// <returns></returns>
        public TypeConfig GetTypeConfig( Type _type ) =>
            m_typeLookup.GetOrAdd( _type, _t => new TypeConfig( _type, this ) );

        /// <summary>
        /// Get an object for a Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>( params object[] @params ) =>
            (T)Get( typeof( T ), @params );

        /// <summary>
        /// Get an object for a Type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="params"></param>
        /// <returns></returns>
        public object Get( Type type, params object[] @params )
        {
            var cfg = GetTypeConfig( type );
            var obj = cfg.Get( @params );
            Inject( obj );
            return obj;
        }

        /// <summary>
        /// For each non-primitive in the object, attempt to set it using configured types.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public void Inject( object obj )
        {
            var injector = new Injector( obj );
            injector.Inject();
        }
    }


}
