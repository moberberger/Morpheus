using Morpheus.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Morpheus
{
    /// <summary>
    /// This interface offers the exposed functionality of the Morpheus Dependency Injection
    /// Framework
    /// </summary>
    public interface IDI
    {
        /// <summary>
        /// Generate a new DI context based on this DI context. See <see cref="DI.NewDefault"/>
        /// to create a new DI context without any ancestors.
        /// </summary>
        /// <returns>A new DI context based on this DI context</returns>
        IDI New();

        /// <summary>
        /// Return an instance of the requested Type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        object Get( Type type, params object[] parms );

        /// <summary>
        /// Inject all reference-typed members as configured
        /// </summary>
        /// <param name="obj"></param>
        object Inject( object obj );

        /// <summary>
        /// Return a config object for a given type.
        /// </summary>
        /// <typeparam name="T">The Type to get the config for</typeparam>
        /// <param name="parms">
        /// If the configured Type is using a <see cref="MethodInfo"/> , then these are the
        /// parameters sent to that Method
        /// </param>
        /// <returns></returns>
        //IClassConfig For<T>( params object[] parms );
    }

    /// <summary>
    /// Extensions that every IDI should support
    /// </summary>
    public static class IDI_Extensions
    {
        /// <summary>
        /// Return an object for a type, as specified in the type parameter.
        /// </summary>
        /// <typeparam name="T">The Type to look up</typeparam>
        /// <returns>
        /// A new object for the given Type, or an exception detailing why a/the object could
        /// not be returned
        /// </returns>
        public static T Get<T>( this IDI idi ) => (T)idi.Get( typeof( T ) );

        /// <summary>
        /// Inject all reference-typed members as configured
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="idi"></param>
        /// <param name="obj"></param>
        public static T Inject<T>( this IDI idi, T obj ) => (T)idi.Inject( obj );

    }
}
