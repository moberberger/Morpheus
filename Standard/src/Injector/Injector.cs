namespace Morpheus.DependencyInjection;


public class Injector
{
    private object obj;
    private DI di;

    public Injector( object obj, DI di ) => (this.obj, this.di) = (obj, di);

    internal void Inject()
    {
        
    }
}