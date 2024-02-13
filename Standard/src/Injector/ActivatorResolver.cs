namespace Morpheus.DependencyInjection;


/// <summary>
/// The most basic of activators- Simply use
/// <see cref="Activator.CreateInstance"/> with the parameters passed.
/// </summary>
public class ActivatorResolver : IResolver
{
    Type activationType;

    /// <summary>
    /// Create using the specified Type. Assumes any validation has been
    /// pre-done.
    /// </summary>
    /// <param name="activationType">
    /// The <see cref="System.Type"/> of the object to create/return
    /// </param>
    internal ActivatorResolver( Type activationType ) => this.activationType = activationType;

    /// <summary>
    /// Create a new object using <see cref="Activator.CreateInstance"/> .
    /// </summary>
    /// <param name="params">
    /// The parameters to pass to <see cref="Activator.CreateInstance"/>
    /// </param>
    /// <returns></returns>
    public object Get( object[] @params ) =>
        Activator.CreateInstance( activationType, @params );
}
