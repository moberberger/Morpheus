﻿namespace Morpheus.DependencyInjection;


/// <summary>
/// This is the default creator associated with each Type in a new DI context.
/// Its purpose is to "forward" all requests for resolution to the parent DI.
/// 
/// If there is no parent, then the default operation is a simple
/// <see cref="Activator.CreateInstance"/> .
/// </summary>
public class OverrideCreator : IResolver
{
    protected Type Type { get; }

    /// <summary>
    /// If null, then the default operation is a simple
    /// <see cref="Activator.CreateInstance"/> . This happens when the DI is the
    /// root DI.
    /// </summary>
    protected DI? Owner { get; }

    /// <summary>
    /// Construct with a type and a parent DI. If the parent DI is null, then
    /// the default operation is simply <see cref="Activator.CreateInstance"/>
    /// </summary>
    /// <param name="type"></param>
    /// <param name="owner"></param>
    /// <exception cref="ArgumentNullException">The Type cannot be null</exception>
    internal OverrideCreator( Type type, DI? owner )
    {
        Type = type;
        Owner = owner;
    }

    public object Get( object[] @params ) =>
        Owner?.Get( Type, @params ) ??
        Activator.CreateInstance( Type, @params )!;
}
