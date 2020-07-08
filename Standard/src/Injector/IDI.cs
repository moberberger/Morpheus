using Morpheus.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Morpheus
{
    /// <summary>
    /// This interface offers the exposed functionality of the Morpheus Dependency Injection Framework
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
        /// Return an object for a type, as specified in the type parameter.
        /// </summary>
        /// <typeparam name="T">The Type to look up</typeparam>
        /// <returns>
        /// A new object for the given Type, or an exception detailing why a/the object could
        /// not be returned
        /// </returns>
        T New<T>();

        /// <summary>
        /// Inject all reference-typed members as configured
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        void Inject<T>( T obj );

        /// <summary>
        /// Return a config object for a given type.
        /// </summary>
        /// <typeparam name="T">The Type to get the config for</typeparam>
        /// <param name="_params">
        /// If the configured Type is using a <see cref="MethodInfo"/> , then these are the
        /// parameters sent to that Method
        /// </param>
        /// <returns></returns>
        IClassConfig For<T>( params object[] _params );
    }
}
