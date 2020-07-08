using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Linq;
using System.Reflection;

namespace Morpheus.DependencyInjection
{
    /// <summary>
    /// This class defines how to return objects for specified <see cref="Type"/> s. It is part
    /// of a FLUENT API (google it if you don't know).
    /// </summary>
    public interface IClassConfig
    {
        /// <summary>
        /// Copy all elements of a given <see cref="IClassConfig"/> into this object.
        /// </summary>
        /// <param name="other">
        /// The <see cref="IClassConfig"/> object to copy into this one. Probably will enforce
        /// type safety!
        /// </param>
        /// <returns>IClassConfig</returns>
        IClassConfig CopyFrom( IClassConfig other );

        /// <summary>
        /// Specify an Object Factory. The objects returned will be cast to the Type being
        /// configured by this object.
        /// </summary>
        /// <remarks>Factory has 1st priority.</remarks>
        /// <param name="factory">
        /// The factory to use to generate objects for the Type configured by this object. Set
        /// this to NULL to prevent Factory processing.
        /// </param>
        /// <returns>IClassConfig</returns>
        IClassConfig UseFactory( Func<object> factory );

        /// <summary>
        /// For this type, set it to the return value of the <see cref="MethodInfo"/> provided,
        /// assuming the <see cref="IDI.New{T}"/> caller passes in any required parameters
        /// </summary>
        /// <param name="method">
        /// The method whose return value should be returned as the object for an
        /// <see cref="IDI.New{T}"/> call
        /// </param>
        /// <returns>IClassConfig</returns>
        IClassConfig UseMethod( MethodInfo method );

        /// <summary>
        /// For this type, set it to a Singleton object. NOTE- This is run-time cast checking!
        /// </summary>
        /// <remarks>Singleton has Second Priority</remarks>
        /// <param name="singleton">
        /// The object to use as a Singleton. Set this to NULL to cancel or prevent singleton
        /// processing
        /// </param>
        /// <returns>IClassConfig</returns>
        IClassConfig UseSingleton( object singleton );

        /// <summary>
        /// Use this <see cref="Type"/> to generate new objects associated with this
        /// configuration. The type <typeparamref name="T"/> must expose a public default
        /// constructor.
        /// </summary>
        /// <typeparam name="T">
        /// The <see cref="Type"/> to use. Objects of this Type must be assignable to a variable
        /// whose Type is configured by this object.
        /// </typeparam>
        /// <returns>IClassConfig</returns>
        IClassConfig UseType<T>();

        /// <summary>
        /// Tells this object to set up object pooling for this configured Type. Only Singleton
        /// configurations are not supported (the object pool will remain unused). Pooling is
        /// OFF by default.
        /// </summary>
        /// <param name="pool">
        /// The object pool to use. If NULL, object pooling will be turned off (default).
        /// 
        /// IT IS HIGHLY RECOMMENDED THAT THE IMPLEMENTOR OF THIS INTERFACE CREATE AND USE A
        /// STATIC OBJECT POOL.
        /// </param>
        /// <returns>IClassConfig</returns>
        IClassConfig UseObjectPool<T>( IObjectPool<T> pool );

        /// <summary>
        /// For use when deciding how to create a "new" object of this Type. The constructor
        /// specified MUST contain only reference-valued elements.
        /// </summary>
        /// <param name="constructor"></param>
        /// <returns>IClassConfig</returns>
        IClassConfig UseConstructor( ConstructorInfo constructor );

        /// <summary>
        /// Member Types specify how a Type is injected- primarily the difference between Fields
        /// and Properties. However, an application has exact control over which members are
        /// used to inject a new class.
        /// </summary>
        EMemberTypes MemberTypes { get; set; }

        /// <summary>
        /// 
        /// Due to the way Reflection works, the collection can contain <see cref="MemberInfo"/>
        /// objects that are not Properties or
        /// </summary>
        ICollection<MemberInfo> SpecificMembers { get; set; }
    }

}
