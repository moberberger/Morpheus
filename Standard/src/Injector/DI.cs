using Morpheus.DependencyInjection;

namespace Morpheus;


/// <summary>
/// A Dependency Injection Scope.
/// </summary>
public class DI
{
    /// <summary>
    /// AppDomain Default DI scope. All DI scopes will eventually link down to this scope.
    /// </summary>
    public static DI Default { get; } = new DI( null );

    /// <summary>
    /// Load Morpheus Defaults into the Default scope.
    /// </summary>
    static DI()
    {
        Default.For<Rng>().UseNewInstance<Xoshiro>();
    }



    /// <summary>
    /// internal lookup table
    /// </summary>
    private readonly Dictionary<Type, TypeConfig> m_typeLookup = new();

    /// <summary>
    /// The parent DI scope. If null, then this is the root scope.
    /// </summary>
    public DI? Parent { get; }

    /// <summary>
    /// Construct with a parent. Must be called by <see cref="DI.New"/> - App cannot
    /// construct directly.
    /// </summary>
    private DI( DI? parent ) => Parent = parent;

    /// <summary>
    /// Create a new DI scope that is a child of this scope.
    /// </summary>
    public DI New() => new DI( this );

    /// <summary>
    /// Does this DI or its parents know about a given Type?
    /// </summary>
    public bool KnowsAbout( Type type ) =>
        m_typeLookup.ContainsKey( type ) || (Parent?.KnowsAbout( type ) ?? false);

    /// <summary>
    /// Does this DI or its parents know about a given Type?
    /// </summary>
    public bool KnowsAbout<T>() => KnowsAbout( typeof( T ) );

    /// <summary>
    /// Lookup Type Metadata and create a proxy if none exists.
    /// </summary>
    public TypeConfig For<T>() where T : notnull => GetTypeConfig( typeof( T ), false );


    /// <summary>
    /// Get the DI <see cref="TypeConfig"/> for a given <see cref="Type"/>
    /// </summary>
    internal TypeConfig GetTypeConfig( Type _type, bool referenceOk )
    {
        if (m_typeLookup.ContainsKey( _type ))
            return m_typeLookup[_type];

        if (Parent?.KnowsAbout( _type ) ?? false)
        {
            var parentRef = Parent.GetTypeConfig( _type, referenceOk );
            if (referenceOk)
                return parentRef;
            m_typeLookup[_type] = new TypeConfig( parentRef );
        }
        else
            m_typeLookup[_type] = new TypeConfig( _type, this );

        return m_typeLookup[_type];
    }

    /// <summary>
    /// Get an object for a Type
    /// </summary>
    public T Get<T>( params object[] @params ) where T : notnull =>
        (T)Get( typeof( T ), @params );

    /// <summary>
    /// Get an object for a Type
    /// </summary>
    public object Get( Type type, params object[] @params )
    {
        var cfg = GetTypeConfig( type, true );
        var obj = cfg.Get( @params );
        Inject( obj );
        return obj;
    }

    /// <summary>
    /// For each non-primitive in the object, attempt to set it using configured types.
    /// </summary>
    public void Inject( object obj )
    {
        var injector = new Injector( obj, this );
        injector.Inject();
    }
}
