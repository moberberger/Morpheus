namespace Morpheus.DependencyInjection
{
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
        /// This is the <see cref="DI"/> that created this config. If this is null, then this
        /// <see cref="TypeConfig"/> cannot "forward" any request to an encapsulating
        /// <see cref="DI"/> and therefore must handle the resolution without forwarding.
        /// </summary>
        DI m_owner;

        /// <summary>
        /// For a given type and parent DI, create a new DI config object
        /// </summary>
        /// <param name="type">The <see cref="Type"/> being configured by this object</param>
        /// <param name="owner">The owner <see cref="DI"/> for this object</param>
        /// <exception cref="ArgumentNullException">
        /// the specified <see cref="Type"/> cannot be NULL
        /// </exception>
        public TypeConfig( Type type, DI owner )
        {
            if (type == null)
                throw new ArgumentNullException( "type" );

            m_type = type;
            m_owner = owner;
            resolver = new OverrideCreator( m_type, owner.Parent );
        }

        public TypeConfig UseNewInstance<T>() => UseNewInstance( typeof( T ) );

        public TypeConfig UseNewInstance( Type activatorType = null )
        {
            resolver = new ActivatorCreator( activatorType ?? m_type );
            return this;
        }

        public TypeConfig UseSingleton( object singleton )
        {
            resolver = new SingletonCreator( singleton );
            return this;
        }

        public TypeConfig UseFactoryLambda( Func<object> factory )
        {
            resolver = new FactoryLambdaCreator( factory );
            return this;
        }

        public TypeConfig UseFactoryLambda( Func<object, object[]> factory )
        {
            resolver = new FactoryLambdaCreator( factory );
            return this;
        }

        public TypeConfig UseCreator( IResolver creator )
        {
            this.resolver = creator;
            return this;
        }


        /// <summary>
        /// Return an object for this Type based on this configuration.
        /// </summary>
        /// <returns>an object for this Type based on this configuration.</returns>
        public object Get( params object[] @params ) => resolver.Get( @params );
    }
}
