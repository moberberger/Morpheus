namespace Morpheus.DependencyInjection
{
    public interface IResolver
    {
        object Get( object[] @params );
    }


    public class OverrideCreator : IResolver
    {
        protected readonly Type m_type;
        protected readonly DI m_owner;
        public OverrideCreator( Type _type, DI _owner )
        {
            m_type = _type;
            m_owner = _owner;
        }

        public object Get( object[] @params ) =>
            m_owner.Get( m_type, @params );
    }

    public class ActivatorCreator : IResolver
    {
        Type activationType;
        public ActivatorCreator( Type activationType ) => this.activationType = activationType;
        public object Get( params object[] @params ) =>
            Activator.CreateInstance( activationType, @params );
    }

    public class SingletonCreator : IResolver
    {
        object singleton;
        public SingletonCreator( object singleton ) => this.singleton = singleton;
        public object Get( object[] @params ) => singleton;
    }

    public class FactoryLambdaCreator : IResolver
    {
        Func<object> lambda;
        public FactoryLambdaCreator( Func<object> factoryLambda ) => lambda = factoryLambda;
        public object Get( object[] @params ) => lambda();
    }


    /// <summary>
    /// This class will describe all DI metadata for a Type.
    /// </summary>
    public class TypeConfig : IResolver
    {
        IResolver creator;
        Type m_type;

        internal TypeConfig( Type _type, DI _owner )
        {
            if ( _type == null )
                throw new ArgumentNullException( "type" );
            if (_owner == null)
                throw new ArgumentNullException( "owner" );

            m_type = _type;
            creator = new OverrideCreator( m_type, _owner );
        }

        public TypeConfig UseNewInstance<T>() => UseNewInstance( typeof( T ) );

        public TypeConfig UseNewInstance( Type activatorType = null )
        {
            creator = new ActivatorCreator( activatorType ?? m_type );
            return this;
        }

        public TypeConfig UseSingleton( object singleton )
        {
            creator = new SingletonCreator( singleton );
            return this;
        }

        public TypeConfig UseFactoryLambda( Func<object> factory )
        {
            creator = new FactoryLambdaCreator( factory );
            return this;
        }

        public TypeConfig UseCreator( IResolver creator )
        {
            this.creator = creator;
            return this;
        }


        /// <summary>
        /// Return an object for this Type based on this configuration.
        /// </summary>
        /// <returns>an object for this Type based on this configuration.</returns>
        public object Get( params object[] @params ) => creator.Get( @params );
    }
}
