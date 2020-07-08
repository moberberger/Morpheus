using System;
using System.Collections.Generic;
using System.Text;

namespace Morpheus
{
    /// <summary>
    /// Implement an Object Pool.
    /// </summary>
    /// <remarks>
    /// The implementor can easily cheat and just "new" every request without processing
    /// "Returns". See
    /// </remarks>
    public interface IObjectPool
    {
        /// <summary>
        /// Calling this signals the implementation to try to have ready the specified number of
        /// objects for the specified Type.
        /// 
        /// The semantics of this call are:
        /// 
        /// (1) This is not meant to be a max-limit, but the implementation could interpret it
        /// this way
        /// 
        /// (2) Its hypothesized that an implementation will pool (save) a
        /// <see cref="Return(object)"/> 'ed object IIF the unused-objects count is below this
        /// value. This is far from a requirement.
        /// 
        /// (3) The pool may already have MORE than this number of objects pooled, especially
        /// "in-use" objects. Regardless of In-Use or not, the pool IS NOT required to decrease
        /// the currently-allocated size under any circumstances. Its decision is reflected in
        /// the return value.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="minimumPoolSize">
        /// If negative, then there is no minimum and objects that are not returned may live
        /// forever. Otherwise an indication of how many available objects are enough to satisfy
        /// future requests.
        /// 
        /// Allows an application to tune its object pool based on usage. By "tune", I primarily
        /// mean "ignore <see cref="Return(object)"/> -ed objects if we have enough waiting to
        /// be used."
        /// </param>
        /// <param name="inusePoolObjects">
        /// How many objects have been <see cref="Get(Type)"/> -ed from this pool and not
        /// <see cref="Return(object)"/> -ed.
        /// </param>
        /// <returns>
        /// The actual number of unused elements in the pool after processing this request.
        /// </returns>
        int Allocate( Type type, int minimumPoolSize, out int inusePoolObjects );

        /// <summary>
        /// Return an object. The caller does not know if the object has been recycled or not.
        /// </summary>
        /// <returns></returns>
        object Get( Type type );

        /// <summary>
        /// Return an object to the pool when its done being used.
        /// </summary>
        /// <param name="obj"></param>
        void Return( object obj );
    }

    /// <summary>
    /// Don't make every implementation do this.
    /// </summary>
    public static class Extension_IObjectPool
    {
        /// <summary>
        /// Get an object using typeparams.
        /// </summary>
        /// <typeparam name="T">
        /// The Type of objects presumed to be in the pool, or they're at least castable to this
        /// Type
        /// </typeparam>
        /// <param name="pool">The pool to Get from</param>
        /// <returns></returns>
        public static T Get<T>( this IObjectPool pool ) => (T)pool.Get( typeof( T ) );

        /// <summary>
        /// Calling this signals the implementation to try to have ready the specified number of
        /// objects for the specified Type.
        /// 
        /// Specifically absent is a statement about whether or not the Object Pool MUST
        /// (re)allocate regardless of the pool size passed as a parameter.
        /// 
        /// The caller MUST NOT depend on any particular implementation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pool"></param>
        /// <param name="minimumPoolSize"></param>
        /// <returns>
        /// The number of pre-allocated items, used or not, after this call is executed. A value
        /// of 0 typically might mean that the object pool implementation isn't really pooling
        /// objects.
        /// </returns>
        public static int Allocate<T>( this IObjectPool pool, int minimumPoolSize = 0 ) => pool.Allocate( typeof( T ), minimumPoolSize );
    }

    /// <summary>
    /// Default IObjectPool implementation
    /// </summary>
    public class NullObjectPool : IObjectPool
    {
        /// <summary>
        /// A Hint can be ignored
        /// </summary>
        /// <param name="type"></param>
        /// <param name="minimumPoolSize"></param>
        /// <returns>0, signalling that this object is not really pooling objects</returns>
        public int Allocate( Type type, int minimumPoolSize = 0 ) { return 0; }

        /// <summary>
        /// Get simply returns a new object- nothing else.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public object Get( Type type ) => Activator.CreateInstance( type );

        /// <summary>
        /// Return does nothing, leaving the object to the <see cref="GC"/> .
        /// </summary>
        /// <param name="obj"></param>
        public void Return( object obj ) { }
    }
}
