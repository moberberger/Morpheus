using System;
using System.Collections.Generic;
using System.Text;

namespace Morpheus
{
    /// <summary>
    /// When implemented, objects can be valued relative to each other.
    /// </summary>
    public interface IEvaluate
    {
        /// <summary>
        /// Return the value, as a <see cref="Double"/> , of the object. This is meant to
        /// provide ranking such that one object of this type can be compared relative to
        /// another object.
        /// 
        /// This does not imply or require any particular algorithm to be used.
        /// 
        /// This should be distinguished from a HashCode because two objects whose values are
        /// equal are not necessarily the same object.
        /// </summary>
        /// <returns>
        /// The Value of the object, based on whichever algorithm the application wishes to use
        /// </returns>
        double GetValue();
    }
}
