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
        #region Static stuff
        /// <summary>
        /// AppDomain Default DI scope. All DI scopes will eventually link down to this scope.
        /// </summary>
        public static DI Default { get; private set; } = new DI();

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
        private readonly EncapsulatingDictionary<Type, TypeConfig> m_typeLookup = null;

        /// <summary>
        /// Construct with a parent.
        /// </summary>
        /// <param name="_parent"></param>
        private DI( DI _parent = null ) =>
            m_typeLookup = new( _parent?.m_typeLookup );

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DI New() => new DI( this );


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
        public TypeConfig GetTypeConfig( Type _type )
        {
            if (!m_typeLookup.ContainsKeyShallow( _type ))
                m_typeLookup.Add( _type, new TypeConfig( _type, this ) );
            return m_typeLookup[ _type ];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>( params object[] @params ) =>
            (T)Get( typeof( T ), @params );

        /// <summary>
        /// 
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
        }
    }


}
