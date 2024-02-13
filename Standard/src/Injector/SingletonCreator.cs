namespace Morpheus.DependencyInjection;


public class SingletonCreator : IResolver
{
    object singleton;
    public SingletonCreator( object singleton ) => this.singleton = singleton
        ?? throw new NullReferenceException( "Singleton objects must be established by caller" );
    public object Get( object[] @params ) => singleton;
}
