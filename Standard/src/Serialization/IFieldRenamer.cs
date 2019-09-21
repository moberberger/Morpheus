using System.Reflection;

namespace Morpheus
{
    /// <summary>
    /// This interface is implemented when an application wants to rename fields using some 
    /// externally defined algorithm.
    /// </summary>
    /// <remarks>
    /// The process that changes fields beginning with "m_" uses this interface to accomplish 
    /// that task.
    /// 
    /// This is generally a "global" process. To change the name of specific fields, use the 
    /// <see cref="ASerializedName"/> attribute. This interface is also used by the 
    /// <see cref="AUseFieldRenamer"/> attribute to change the fields on a specific class.
    /// </remarks>
    public interface IFieldRenamer
    {
        /// <summary>
        /// Change the name of a field into some other name. If no change is to occur, simply 
        /// return value passed in.
        /// </summary>
        /// <param name="_fieldName">The name of the field whose name you are changing</param>
        /// <param name="_fieldInfo">The FieldInfo for the field whose name you are changing</param>
        /// <returns>
        /// The "New Name" for the field, or simply return _fieldName if no change is to 
        /// occur. Never return NULL unless NULL is passed in to the routine.
        /// </returns>
        string ConvertFieldName( string _fieldName, FieldInfo _fieldInfo );
    }
}