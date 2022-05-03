namespace Morpheus.DependencyInjection
{
    public interface IResolver
    {
        object Get( object[] @params );
    }
}
