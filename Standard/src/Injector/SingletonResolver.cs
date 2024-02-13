namespace Morpheus.DependencyInjection;


public class SingletonResolver : IResolver
{
    object singleton;
    internal SingletonResolver( object singleton ) => 
        this.singleton = singleton
        ?? throw new NullReferenceException( "Singleton objects must be established by caller" );
    public object Get( object[] @params ) => singleton;
}
