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
    public class DI
    {
        #region Static stuff
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
        public DI CreateChild() => new DI( this );


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
    /// this looks like its a little over the top for accessing the static object- but maybe not
    /// for simple applications
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
        /// </summary>
        /// <param name="_type"></param>
        public static void Use( Type _type ) => DI.Default.For<T>().Use( _type );
    }
}
