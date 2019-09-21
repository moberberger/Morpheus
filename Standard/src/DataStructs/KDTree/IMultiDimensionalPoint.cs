using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morpheus
{
    /// <summary>
    /// If an object is to be stored in a KD Tree, it must be able to identify itself as a vector of coordinates. The object 
    /// either needs to implement this interface or the application needs to provide a "selector". If a selector
    /// is provided, it will override the object's implementation of IMultiDimensionalPoint.
    /// </summary>
    /// <remarks>
    /// Implementation of this interface should only be used for non-performance-sensitive applications. The KD tree performs
    /// significantly better when lambdas are provided for both the Selector and the DistanceFunction. 
    /// </remarks>
    public interface IMultiDimensionalPoint
    {
        /// <summary>
        /// Get the number of axes that the point is described in
        /// </summary>
        /// <returns>The number of axes that the point is described in</returns>
        int GetAxisCount();

        /// <summary>
        /// For a given axis, return the coordinate for that axis
        /// </summary>
        /// <param name="_axis">The axis to look at</param>
        /// <returns>The coordinate for the point and that axis</returns>
        double GetAxisCoordinate( int _axis );

        /// <summary>
        /// Return a scalar distance between this point and some other point
        /// </summary>
        /// <param name="_other">The other point</param>
        /// <returns>The scalar distance between the two points</returns>
        double GetDistance( IMultiDimensionalPoint _other );
    }
}
