using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morpheus
{
    /// <summary>
    /// This class was designed as an optimization for the KDTree. It is meant to optimally keep a list (or
    /// maybe a single) of "closest objects" based on a Distance value.
    /// 
    /// It is meant to be used as an "accumulator", where every visited node is "added", but only retained
    /// by this class if it meets the criteria set up by the "mode" of the object and the parameter for that
    /// mode.
    /// </summary>
    /// <remarks>
    /// This class operates in three "modes". Were the design based on the best OO principles, there would be
    /// three separate classes, one for each mode. However, the design is focused on SPEED OF EXECUTION, so 
    /// the implementation of an interface would prevent any inlining of functionality. This is especially
    /// undesirable for the "FindNearest(1)" trivial case.
    /// </remarks>
    internal class ClosestObjects<T>
    //where T : class
    {
        /// <summary>
        /// A simple object that encapsulates a distance and an object, used for the <see cref="EClosestMode.ClosestCount"/>
        /// mode of the CClosestObjects class
        /// </summary>
        private class CNodeDistance : IComparable<CNodeDistance>
        {
            public T Data;
            public double Distance;

            public int CompareTo( CNodeDistance _other )
            {
                if (Distance < _other.Distance)
                    return 1;
                else if (Distance > _other.Distance)
                    return -1;
                else
                    return 0;
            }

            public override string ToString() => string.Format( "{0} = {1:N2}", Data.ToString(), Distance );
        }

        internal enum EClosestMode
        {
            /// <summary>
            /// The object is meant to track only the single closest object. Most trivial case.
            /// </summary>
            ClosestSingle,

            /// <summary>
            /// The object is meant to track the N closest objects. The most complex case.
            /// </summary>
            ClosestCount,

            /// <summary>
            /// The object is meant to track all objects closer than some distance D
            /// </summary>
            CloserThan
        };


        /// <summary>
        /// For <see cref="EClosestMode.ClosestSingle"/>, this value denotes the smallest distance seen,
        /// associated with the m_bestObject field.
        /// 
        /// For <see cref="EClosestMode.CloserThan"/>, this field contains the max distance to retain nodes.
        /// 
        /// For <see cref="EClosestMode.ClosestCount"/>, this field has no semantic value.
        /// </summary>
        /// <remarks>
        /// This is overloaded to minimize the size of this object, to help with L1-3 cache hits.
        /// </remarks>
        private double m_distance = 0;

        /// <summary>
        /// The maximum number of objects to keep track of. This is only useful for the
        /// <see cref="EClosestMode.ClosestCount"/> mode.
        /// </summary>
        private readonly int m_maxCount = 0;

        /// <summary>
        /// For <see cref="EClosestMode.ClosestSingle"/>, this value references the object with the best
        /// (shortest) distance seen so far
        /// </summary>
        private T m_bestObject = default;

        /// <summary>
        /// The list of all objects that are closer than the parameter specified. Used ONLY for mode 
        /// <see cref="EClosestMode.CloserThan"/>
        /// </summary>
        private readonly List<T> m_closeObjects;

        /// <summary>
        /// The set of the N closest objects seen so far. Used ONLY for mode <see cref="EClosestMode.ClosestCount"/>
        /// </summary>
        private readonly PriorityQueue<CNodeDistance> m_bestObjectsSoFar;


        /// <summary>
        /// Constructor specifying how to set up the closest objects collection for <see cref="EClosestMode.ClosestSingle"/>.
        /// The collection will only maintain knowledge of the closest single node seen so far
        /// </summary>
        public ClosestObjects()
        {
            m_closeObjects = null;
            m_bestObjectsSoFar = null;
            m_maxCount = 1;
            m_distance = double.MaxValue;
        }

        /// <summary>
        /// Constructor specifying how to set up the closest objects collection for <see cref="EClosestMode.ClosestCount"/>.
        /// The collection will only maintain knowledge of the closest _closestCount objects seen so far.
        /// </summary>
        public ClosestObjects( int _closestCount )
        {
            m_closeObjects = null;
            m_bestObjectsSoFar = new PriorityQueue<CNodeDistance>( _closestCount );
            m_maxCount = _closestCount;
        }

        /// <summary>
        /// Constructor specifying how to set up the closest objects collection for <see cref="EClosestMode.CloserThan"/>.
        /// The collection will only maintain knowledge of all added objects that are closer than this value.
        /// </summary>
        public ClosestObjects( double _closerThan )
        {
            m_closeObjects = new List<T>();
            m_bestObjectsSoFar = null;
            m_maxCount = -1;
            m_distance = _closerThan * _closerThan;
        }



        /// <summary>
        /// Accumulate an item (with its distance value) into this collection. This method determines the "mode" of the
        /// collection. 
        /// </summary>
        /// <param name="_item">The item to add</param>
        /// <param name="_distance">The item's distance to use for "close" comparison</param>
        /// <returns>The "worst distance so far", used to compare against cross-axis values</returns>
        public double Add( T _item, double _distance )
        {
            // Doesn't matter now if its ClosestCount or ClosestSingle if maxCount == 1
            if (m_maxCount == 1) // Definitely hits "ClosestSingle" regardless
            {
                if (_distance < m_distance)
                {
                    m_distance = _distance;
                    m_bestObject = _item;
                }
                return m_distance;
            }
            else if (m_maxCount == -1) // CloserThan
            {
                if (_distance <= m_distance) // it IS CloserThan
                {
                    m_closeObjects.Add( _item );
                }
                return m_distance; // m_distance IS the "worst possible close distance" so far, even if there's nothing at that distance
            }
            else // ClosestCount
            {
                // IF the queue is full AND the new distance puts this object into the queue
                if (m_bestObjectsSoFar.Count >= m_maxCount && m_bestObjectsSoFar.LowestNode.Distance > _distance)
                    m_bestObjectsSoFar.RemoveLowest();

                m_bestObjectsSoFar.Add( new CNodeDistance() { Data = _item, Distance = _distance } );
                return m_bestObjectsSoFar.LowestNode.Distance;
            }
        }

        /// <summary>
        /// Return the closest single object. If the "CloserThan" mode was chosen, then the first object found that
        /// met the closeness measurement will be returned, which may NOT be the closest.
        /// </summary>
        /// <returns></returns>
        public T GetClosestSingle()
        {
            if (m_maxCount == 1)
                return m_bestObject;
            if (m_maxCount == -1)
                return m_closeObjects[0];
            return m_bestObjectsSoFar.LowestNode.Data;
        }

        /// <summary>
        /// Get the objects in this collection. Does the right thing based on the "mode" of this collection.
        /// </summary>
        /// <returns>An enumeration of objects in this collection</returns>
        public IEnumerable<T> GetClosestObjects()
        {
            if (m_maxCount == 1) // best single OR closest N where N==1
                return new T[] { m_bestObject };
            if (m_maxCount == -1)
                return m_closeObjects;

            return m_bestObjectsSoFar.Select( _nd => _nd.Data );
        }
    }
}

