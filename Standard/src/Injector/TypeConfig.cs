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
    /// This config describes this type.
    /// </summary>
    Type m_type;

    /// <summary>
    /// This is the <see cref="DI"/> that created this config. If this is null,
    /// then this <see cref="TypeConfig"/> cannot forward any request to an
    /// encapsulating <see cref="DI"/> and therefore must handle the resolution
    /// without forwarding.
    /// </summary>
    DI m_owner;

    /// <summary>
    /// For a given type and parent DI, create a new DI config object
    /// </summary>
    /// <param name="type">
    /// The <see cref="Type"/> being configured by this object
    /// </param>
    /// <param name="owner">The owner <see cref="DI"/> for this object</param>
    /// <exception cref="ArgumentNullException">
    /// the specified <see cref="Type"/> cannot be NULL
    /// </exception>
    internal TypeConfig( Type type, DI owner )
    {
        m_type = type ?? throw new ArgumentNullException( "type" );
        m_owner = owner ?? throw new ArgumentNullException( "owner" );
        resolver = new OverrideCreator( m_type, owner.Parent );
    }

    internal TypeConfig( TypeConfig other )
    {
        m_type = other.m_type;
        m_owner = other.m_owner;
        resolver = other.resolver;
    }

    private void AssertAssignable( Type type )
    {
        if (type == null)
            throw new ArgumentNullException( "type" );

        if (!m_type.IsAssignableFrom( type ))
            throw new InvalidCastException( $"The specified activatorType '{type.GetType()}' must be a subclass of '{m_type}'" );
    }

    public TypeConfig UseNewInstance<T>() where T : notnull =>
        UseNewInstance( typeof( T ) );

    public TypeConfig UseNewInstance( Type? type = null )
    {
        type ??= m_type;

        AssertAssignable( type );
        resolver = new ActivatorResolver( type );
        return this;
    }

    public TypeConfig UseSingleton( object singleton )
    {
        if (singleton is null) throw new ArgumentNullException( "singleton" );

        AssertAssignable( singleton.GetType() );
        resolver = new SingletonResolver( singleton );
        return this;
    }

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

    public TypeConfig UseFactory( IResolver creator )
    {
        resolver = creator;
        return this;
    }


    /// <summary>
    /// Return an object for this Type based on this configuration.
    /// </summary>
    /// <returns>an object for this Type based on this configuration.</returns>
    public object Get( params object[] @params ) => resolver.Get( @params );
}
