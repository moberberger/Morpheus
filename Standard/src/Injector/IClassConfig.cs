﻿using System;
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
        /// Tells the implementation how to treat this object's configured members during
        /// injection when said member already contains a non-null value.
        /// 
        /// It is expected that the default operation is NOT to inject if the member is already
        /// set (not-null).
        /// </summary>
        /// <param name="injectIfNotNull"></param>
        /// <returns></returns>
        IClassConfig InjectIfNotNull( bool injectIfNotNull = true );

        /// <summary>
        /// Specify an Object Factory. The objects returned will be cast to the Type being
        /// configured by this object.
        /// </summary>
        /// <remarks>This provider has 1st priority.</remarks>
        /// <param name="factory">
        /// The factory to use to generate objects for the Type configured by this object. Set
        /// this to NULL to prevent Factory processing.
        /// </param>
        /// <returns>IClassConfig</returns>
        IClassConfig UseFactory( Func<object> factory );

        /// <summary>
        /// For this type, generate values from the return value of the <see cref="MethodInfo"/>
        /// provided, assuming the <see cref="IDI.New"/> caller passes in any required
        /// parameters for the provided <see cref="MethodInfo"/> .
        /// </summary>
        /// <remarks>This provider has 2nd priority.</remarks>
        /// <returns>IClassConfig</returns>
        IClassConfig UseFactory( MethodInfo method );

        /// <summary>
        /// For this type, set it to a Singleton object. NOTE- This is run-time cast checking!
        /// </summary>
        /// <remarks>This provider has 3rd priority.</remarks>
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
        /// <remarks>This provider has 4th priority.</remarks>
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
        /// <returns>IClassConfig</returns>
        ///IClassConfig UseObjectPool( IObjectPool pool );

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
        IClassConfig UseMemberTypes( EMemberTypes memberTypes = EMemberTypes.Inherit );

        /// <summary>
        /// Due to the way Reflection works, the enumeration can contain
        /// <see cref="MemberInfo"/> objects that are not Properties or Fields. These should be
        /// cause for throwing an Exception in the implementing class.
        /// </summary>
        IClassConfig UseSpecificMembers( IEnumerable<MemberInfo> SpecificMembers );

        /// <summary>
        /// Return an IParameterConfig for the parameter index specified. All typecast
        /// information is performed at run-time.
        /// </summary>
        /// <param name="parameterIndex"></param>
        /// <returns></returns>
        IParameterConfig ForParameterConfig( int parameterIndex );

        /// <summary>
        /// Get an instance of the specified object based on the configuration present in this
        /// object.
        /// </summary>
        /// <param name="type">The Type of the object to retrieve</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        object GetInstance( Type type, object[] parameters );
    }

    /// <summary>
    /// Configuration for a parameter for a method/constructor
    /// </summary>
    public interface IParameterConfig
    {
    }
}
