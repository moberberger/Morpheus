using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Morpheus.DependencyInjection
{
    /// <summary>
    /// When the Injector injects members (think: " <see cref="MemberInfo"/> ") of objects of a
    /// configured <see cref="Type"/> , how should it know what members to use?
    /// </summary>
    public enum EMemberTypes
    {
        /// <summary>
        /// Use only and all <see cref="PropertyInfo"/> objects for the Type
        /// 
        /// <see cref="EMemberTypes.Inherit"/> SHOULD BE THE DEFAULT VALUE for members of this Type.
        /// </summary>
        Properties,
        /// <summary>
        /// Use only and all <see cref="FieldInfo"/> objects for the Type
        /// 
        /// <see cref="EMemberTypes.Inherit"/> SHOULD BE THE DEFAULT VALUE for members of this Type.
        /// </summary>
        Fields,
        /// <summary>
        /// Inherit this value from an ancestor, or use DEFAULTS: Properties then Fields.
        /// 
        /// <see cref="EMemberTypes.Inherit"/> SHOULD BE THE DEFAULT VALUE for members of this Type.
        /// </summary>
        Inherit,
        /// <summary>
        /// d
        /// 
        /// <see cref="EMemberTypes.Inherit"/> SHOULD BE THE DEFAULT VALUE for members of this Type.
        /// </summary>
        SpecificList
    }
}
