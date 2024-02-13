#nullable disable

namespace Morpheus;


/// <summary>
/// This attribute allows a class to designate a specific field renamer to use on all 
/// fields in the class.
/// </summary>
[AttributeUsage( AttributeTargets.Class, AllowMultiple = false )]
public class AUseFieldRenamer : Attribute
{
    /// <summary>
    /// Expose the Type to the framework
    /// </summary>
    public Type RenamerType { get; } = null;

    /// <summary>
    /// When this is TRUE, the Type specified may change the way it transforms names during run-time, thus
    /// requiring the framework to call the renamer each and every time a field is asked for its name. When
    /// its FALSE, the renamer will ALWAYS rename fields using the same algorithm, thus allowing the framework
    /// to call it a single time and then store the results for future uses (and as a byproduct, it can release
    /// the reference to the instantiated object)
    /// </summary>
    public bool DynamicRenaming { get; set; } = false;

    /// <summary>
    /// This attribute must be constructed using the Type which implements an IFieldRenamer
    /// </summary>
    /// <param name="_typeImplementingIFieldRenamer">The System.Type of the object that 
    /// can rename the fields in this class.</param>
    public AUseFieldRenamer( Type _typeImplementingIFieldRenamer )
    {
        RenamerType = _typeImplementingIFieldRenamer;
    }
}