using System;

namespace Morpheus
{
    /// <summary>
    /// Use this attribute to rename a field for purposes of serialization. This is a very 
    /// specific form of renaming, as opposed to the bulk-renaming accomplished with 
    /// IFieldRenamer.
    /// </summary>
    /// <remarks>
    /// This attribute will override any class-specific <see cref="IFieldRenamer"/> uses, 
    /// which are set up using the <see cref="AUseFieldRenamer"/> attribute. 
    /// 
    /// This attribute WILL NOT override "global" renamers, as these are assumed to be useful 
    /// based on run-time configuration and need to deal with things like this. However, the 
    /// name given by this attribute to a field WILL be the name sent to the global renamer.
    /// </remarks>
    [AttributeUsage( AttributeTargets.Field, AllowMultiple = false )]
    public class ASerializedName : Attribute
    {
        /// <summary>
        /// This attribute must be constructed using a string representing the new name
        /// </summary>
        /// <param name="_newName"></param>
        public ASerializedName( string _newName )
        {
            NewName = _newName;
        }

        /// <summary>
        /// The field is to be known by this name when it is serialized, or it is to be populated from 
        /// the Xml node named this when it is deserialized
        /// </summary>
        public string NewName { get; } = null;
    }
}