namespace Morpheus.DependencyInjection
{
    /// <summary>
    /// This is the default creator associated with each Type in a new DI context. Its purpose
    /// is to "forward" all requests for resolution to the parent DI.
    /// 
    /// If there is no parent, then the default operation is a simple
    /// <see cref="Activator.CreateInstance"/> .
    /// </summary>
    public class OverrideCreator : IResolver
    {
        protected readonly Type m_type;
        protected readonly DI m_owner;
        public OverrideCreator( Type type, DI owner )
        {
            m_type = type ?? throw new ArgumentNullException( "type" );
            m_owner = owner;
        }

        public object Get( object[] @params )
        {
            if (m_owner?.Contains( m_type ) ?? false)
                return m_owner.Get( m_type, @params );
            else
                return Activator.CreateInstance( m_type, @params );
        }
    }
}
