namespace Morpheus.DependencyInjection;


public class FactoryResolver0<T> : IResolver where T : notnull
{
    readonly Func<T> factory;
    internal FactoryResolver0( Func<T> factory ) => this.factory = factory;
    public object Get( object[] @params ) => factory();
}

public class FactoryResolver1<T, P1> : IResolver where T : notnull
{
    readonly Func<P1, T> factory;
    internal FactoryResolver1( Func<P1, T> factory ) => this.factory = factory;
    public object Get( object[] @params ) => factory( (P1)@params[0] );
}

public class FactoryResolver2<T, P1, P2> : IResolver where T : notnull
{
    readonly Func<P1, P2, T> factory;
    internal FactoryResolver2( Func<P1, P2, T> factory ) => this.factory = factory;
    public object Get( object[] @params ) => factory( (P1)@params[0], (P2)@params[1] );
}

public class FactoryResolver3<T, P1, P2, P3> : IResolver where T : notnull
{
    readonly Func<P1, P2, P3, T> factory;
    internal FactoryResolver3( Func<P1, P2, P3, T> factory ) => this.factory = factory;
    public object Get( object[] @params ) => factory( (P1)@params[0], (P2)@params[1], (P3)@params[2] );
}
