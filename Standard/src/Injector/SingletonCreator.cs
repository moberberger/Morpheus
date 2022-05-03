namespace Morpheus.DependencyInjection
{
    public class SingletonCreator : IResolver
    {
        object singleton;
        public SingletonCreator( object singleton ) => this.singleton = singleton;
        public object Get( object[] @params ) => singleton;
    }
}
