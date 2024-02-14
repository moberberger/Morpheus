namespace Morpheus.DependencyInjection;


/// <summary>
/// This class will describe all DI metadata for a Type.
/// </summary>
public class TypeConfig : IResolver
{
    /// <summary>
    /// This is the actual resolver currently used by this config
    /// </summary>
    IResolver resolver;

    /// <summary>
    /// This config object describes how to handle this type.
    /// </summary>
    Type m_type;

    /// <summary>
    /// This is the <see cref="DI"/> that created this config.
    /// </summary>
    DI m_owner;

    /// <summary>
    /// For a given type and parent DI, create a new DI config object
    /// </summary>
    /// <param name="type">
    /// The <see cref="Type"/> being configured by this object
    /// </param>
    /// <param name="owner">The owner <see cref="DI"/> for this object</param>
    internal TypeConfig( Type type, DI owner )
    {
        m_type = type;
        m_owner = owner;
        resolver = new OverrideCreator( m_type, owner.Parent );
    }

    /// <summary>
    /// Copy constructor for this type. Used when a parent's config needs to be
    /// initialized for a child DI.
    /// </summary>
    /// <param name="other">The TypeConfig to copy</param>
    internal TypeConfig( TypeConfig other )
    {
        m_type = other.m_type;
        m_owner = other.m_owner;
        resolver = other.resolver;
    }

    /// <summary>
    /// Makes sure that a Type is assignable to the type being configured by
    /// this object.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Cannot assign an object of the specified <see cref="Type"/> to a
    /// variable of type <see cref="m_type"/>
    /// </exception>
    private void AssertAssignable( Type type )
    {
        if (!m_type.IsAssignableFrom( type ))
            throw new InvalidCastException( $"The specified activatorType '{type.GetType()}' must be a subclass of '{m_type}'" );
    }

    /// <summary>
    /// Generic version of <see cref="UseNewInstance(Type?)"/> with a
    /// compile-time known <see cref="Type"/> .
    /// 
    /// This usage model will use <see cref="Activator.CreateInstance"/> to
    /// create a new instance of the specified type.
    /// </summary>
    /// <returns>
    /// A <see cref="TypeConfig"/> for the specified <see cref="Type"/> .
    /// </returns>
    public TypeConfig UseNewInstance<T>() where T : notnull =>
        UseNewInstance( typeof( T ) );

    /// <summary>
    /// This usage model will use <see cref="Activator.CreateInstance"/> to
    /// create a new instance of the specified type.
    /// </summary>
    /// <param name="type">
    /// The <see cref="Type"/> of the objects returned for the
    /// <see cref="Type"/> specified by this config object.
    /// </param>
    /// <returns>
    /// A <see cref="TypeConfig"/> for the specified <see cref="Type"/> .
    /// </returns>
    public TypeConfig UseNewInstance( Type? type = null )
    {
        type ??= m_type;

        AssertAssignable( type );
        resolver = new ActivatorResolver( type );
        return this;
    }

    /// <summary>
    /// This usage model will use the specified object as the singleton for the
    /// configured Type.
    /// </summary>
    /// <param name="singleton">
    /// The singleton object returned when the configured Type is requested
    /// </param>
    /// <returns>
    /// A <see cref="TypeConfig"/> for the specified <see cref="Type"/> .
    /// </returns>
    public TypeConfig UseSingleton( object singleton )
    {
        AssertAssignable( singleton.GetType() );
        resolver = new SingletonResolver( singleton );
        return this;
    }

    /// <summary>
    /// Use a factory to create objects for this Type.
    /// </summary>
    /// <typeparam name="T">
    /// The Return Type from the Factory method. Must be assignable to the
    /// configured Type.
    /// </typeparam>
    /// <param name="factory"></param>
    /// <returns>
    /// A <see cref="TypeConfig"/> for the specified <see cref="Type"/> .
    /// </returns>
    public TypeConfig UseFactory<T>( Func<T> factory ) where T : notnull
    {
        AssertAssignable( typeof( T ) );
        resolver = new FactoryResolver0<T>( factory );
        return this;
    }

    public TypeConfig UseFactory<T, P1>( Func<P1, T> factory ) where T : notnull
    {
        AssertAssignable( typeof( T ) );
        resolver = new FactoryResolver1<T, P1>( factory );
        return this;
    }

    public TypeConfig UseFactory<T, P1, P2>( Func<P1, P2, T> factory ) where T : notnull
    {
        AssertAssignable( typeof( T ) );
        resolver = new FactoryResolver2<T, P1, P2>( factory );
        return this;
    }

    public TypeConfig UseFactory<T, P1, P2, P3>( Func<P1, P2, P3, T> factory ) where T : notnull
    {
        AssertAssignable( typeof( T ) );
        resolver = new FactoryResolver3<T, P1, P2, P3>( factory );
        return this;
    }

    /// <summary>
    /// Use a factory to create objects for this Type.
    /// </summary>
    /// <param name="creator">
    /// An <see cref="IResolver"/> which returns objects for the configured
    /// Type.
    /// </param>
    /// <returns></returns>
    public TypeConfig UseFactory( IResolver creator )
    {
        resolver = creator;
        return this;
    }


    /// <summary>
    /// Return an object for this configured Type based on the <see cref="IResolver"/> which was 
    /// configured for this type. If none has been configured, then the default operation is to
    /// use <see cref="Activator.CreateInstance(Type)"/>
    /// </summary>
    /// <returns>An object for this Type based on this configuration.</returns>
    public object Get( params object[] @params ) => resolver.Get( @params );
}
