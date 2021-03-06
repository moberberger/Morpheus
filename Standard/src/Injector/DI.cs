﻿using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

using Morpheus.DependencyInjection;

namespace Morpheus
{
    using THIS_DICT = IDictionary<Type, ClassConfig>;

    /// <summary>
    /// A Dependency Injection Scope.
    /// </summary>
    public class DI : IDI
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
            Default.For<Random>().Use<CryptoRandomNumbers>();
            Default.For<LCPRNG>().Use<LCPRNG_MMIX>();
        }

        /// <summary>
        /// Create a new DI Scope
        /// </summary>
        /// <returns></returns>
        public static DI NewDefault() => new DI( Default );
        #endregion



        /// <summary>
        /// internal lookup table
        /// </summary>
        private readonly THIS_DICT m_typeLookup = null;

        /// <summary>
        /// Construct with a parent.
        /// </summary>
        /// <param name="_parent"></param>
        private DI( DI _parent = null ) => m_typeLookup = _parent?.m_typeLookup ?? new EncapsulatingDictionary<Type, ClassConfig>();


        /// <summary>
        /// Get the DI <see cref="ClassConfig"/> for a given <see cref="Type"/>
        /// </summary>
        /// <param name="_type"></param>
        /// <returns></returns>
        public ClassConfig GetClassConfig( Type _type ) => m_typeLookup.GetOrAdd( _type, t => new ClassConfig( t, this, null ) );

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IDI New() => new DI( this );


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
        public T Get<T>() where T : class => (T)(For<T>().GetObject());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        public object GetInstance( Type type, params object[] parms )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public object Inject( object obj )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parms"></param>
        /// <returns></returns>
        public IClassConfig For<T>( params object[] parms )
        {
            throw new NotImplementedException();
        }
    }




}
