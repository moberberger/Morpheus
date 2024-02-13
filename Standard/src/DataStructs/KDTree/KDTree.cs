#nullable disable

namespace Morpheus;


/// <summary>
/// A K-D Tree
/// </summary>
/// <typeparam name="T">
/// The Type of the objects stored in this KD tree. Must be a "Class". Must be able to
/// identify at least one "double" value as a dimensional coordinate.
/// </typeparam>
/// <remarks>
/// The KD Tree does allow duplicate elements. When looking for the nearest element, if
/// there are multiple duplicate elements that are "nearest", there is no guarantee
/// which of the duplicates will be returned.
/// </remarks>
public class KDTree<T> : ICollection<T> where T : notnull, new()
// where T : class
{
    /// <summary>
    /// The Selector- Given an object of type T and an Axis, return the coordinate
    /// </summary>
    protected Func<T, int, double> m_selector = GetMultiDimensionalCoord;

    /// <summary>
    /// The Distance Function- Return the SQUARE of the euclidean distance between two
    /// objects of type T
    /// </summary>
    protected Func<T, T, double> m_distanceFunction;

    /// <summary>
    /// The underlying data structure containing the tree's data
    /// </summary>
    protected SparseLeafBinaryTree<T> m_tree = null;

    /// <summary>
    /// The number of axes that the tree is splitting on
    /// </summary>
    protected int m_axisCount = -1;

    /// <summary>
    /// The number of nodes in the tree
    /// </summary>
    protected int m_count = 0;

    /// <summary>
    /// Can be used by the application to monitor how many nodes are being explored when
    /// a "Find" method is invoked. If ExploredNodes is not-null, then each Find
    /// operation will clear it, then fill it with all nodes explored. This is for
    /// debugging and display purposes only- This list is not used by this class in any
    /// other way.
    /// </summary>
    public List<T> ExploredNodes;


    #region Methods dealing with IMultiDimensionalPoint T objects
    /// <summary>
    /// Assert that an object (of type T) is also implementing
    /// <see cref="IMultiDimensionalPoint"/>
    /// </summary>
    /// <param name="_obj">The object of type T</param>
    /// <returns>
    /// The object represented as an <see cref="IMultiDimensionalPoint"/>
    /// </returns>
    private static IMultiDimensionalPoint AssertIMultiDimensionalPoint( T _obj )
    {
        if (!(_obj is IMultiDimensionalPoint o))
            throw new InvalidOperationException( "The underlying type " + typeof( T ).ToString() + " does not implement IMultidimensionalPoint." );
        return o;
    }

    /// <summary>
    /// Checks to see if the generic type T implements
    /// <see cref="IMultiDimensionalPoint"/>
    /// </summary>
    /// <returns>
    /// TRUE if T implements <see cref="IMultiDimensionalPoint"/> , FALSE if not
    /// </returns>
    private static bool IsTMultiDimensional() => typeof( IMultiDimensionalPoint ).IsAssignableFrom( typeof( T ) );

    /// <summary>
    /// A version of the Func(T, int, double) for <see cref="m_selector"/> that depends
    /// on T implementing <see cref="IMultiDimensionalPoint"/>
    /// </summary>
    /// <param name="_obj">
    /// The object to cast as <see cref="IMultiDimensionalPoint"/> and get an axis
    /// coordinate from
    /// </param>
    /// <param name="_axis">The axis to look at</param>
    /// <returns>The coordinate of the object for the specified axis</returns>
    private static double GetMultiDimensionalCoord( T _obj, int _axis )
    {
        var o = AssertIMultiDimensionalPoint( _obj );
        return o.GetAxisCoordinate( _axis );
    }

    /// <summary>
    /// A version of the Func(T, T, double) that treats the object as an
    /// <see cref="IMultiDimensionalPoint"/> OR it uses <see cref="m_selector"/> to get
    /// the coordinates of both objects. In either case, the distanceSquared between the
    /// two points is returned.
    /// </summary>
    /// <param name="_p1">One of the points</param>
    /// <param name="_p2">The other point</param>
    /// <returns>The square of the distance between the two points</returns>
    private double GetMultiDimensionalDistance( T _p1, T _p2 )
    {
        if (IsTMultiDimensional())
        {
            var xx = _p1 as IMultiDimensionalPoint;
            var yy = _p2 as IMultiDimensionalPoint;
            return xx.GetDistance( yy );
        }
        else
        {
            var total = 0.0;
            for (var i = 0; i < m_axisCount; i++)
            {
                var c1 = m_selector( _p1, i );
                var c2 = m_selector( _p2, i );
                total += (c1 - c2) * (c1 - c2);
            }
            return total;
        }
    }
    #endregion

    #region A class and method to help deal with the construction of the tree from a collection of objects of type T

    /// <summary>
    /// An array of IComparers for this KD tree.
    /// </summary>
    protected IComparer<T>[] m_comparers;

    /// <summary>
    /// Implements IComparer for a particular axis
    /// </summary>
    private class CAxisComparer : IComparer<T>
    {
        public int Axis;
        public Func<T, int, double> Selector;

        public int Compare( T _x, T _y )
        {
            var xv = Selector( _x, Axis );
            var yv = Selector( _y, Axis );

            if (xv < yv)
                return -1;
            if (xv > yv)
                return 1;
            return 0;
        }
    }

    /// <summary>
    /// Builds the m_comparers array for this KD tree
    /// </summary>
    private void BuildComparers()
    {
        var list = new List<IComparer<T>>();
        for (var i = 0; i < m_axisCount; i++)
        {
            list.Add( new CAxisComparer() { Axis = i, Selector = m_selector } );
        }
        m_comparers = list.ToArray();
    }
    #endregion


    /// <summary>
    /// Construct a new K-D Tree, using the default selector for objects. This is a
    /// performance-hindered version, as it relies on
    /// <see cref="IMultiDimensionalPoint"/> .
    /// </summary>
    public KDTree()
    {
        m_distanceFunction = GetMultiDimensionalDistance;
        if (!IsTMultiDimensional())
            throw new InvalidOperationException( "The underlying type " + typeof( T ).ToString() + " does not implement IMultidimensionalPoint and no selector was specified." );
        m_tree = new SparseLeafBinaryTree<T>( 1023 );
    }

    /// <summary>
    /// Construct a new K-D Tree, using the specified selector for objects
    /// </summary>
    public KDTree( int _axisCount, Func<T, int, double> _selector, Func<T, T, double> _distanceFunction = null )
    {
        if (_axisCount < 1)
            throw new ArgumentOutOfRangeException( "Axis Count", _axisCount, "Axis Count must be greater than 1" );
        m_selector = _selector ?? throw new ArgumentNullException( "_selector", "The selector cannot be null" );
        m_axisCount = _axisCount;
        m_distanceFunction = _distanceFunction ?? GetMultiDimensionalDistance;
        m_tree = new SparseLeafBinaryTree<T>( 1023 );
    }

    /// <summary>
    /// Set up the K-D Tree based on an enumeration of elements. If the enumeration is
    /// another KDTree of type T, then a copy constructor <see cref="Clone"/> will be
    /// used.
    /// </summary>
    /// <param name="_collection">The collection of objects</param>
    /// <param name="_axisCount">
    /// The number of dimensions present in this KD Tree... use default if T implements
    /// <see cref="IMultiDimensionalPoint"/>
    /// </param>
    /// <param name="_selector">
    /// The selector to use on the objects- if NULL, relies on the objects to implement
    /// <see cref="IMultiDimensionalPoint"/>
    /// </param>
    /// <param name="_distanceFunction">
    /// A function that determines the distance between two nodes in the tree. Assumes
    /// Distances are measured using double precision floating point scalar values.
    /// </param>
    /// <remarks>
    /// If _axisCount == -1 and/or _selector == null, then T MUST implement
    /// <see cref="IMultiDimensionalPoint"/> .
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if _axisCount is less than 1 AND T is not an implementation of
    /// ArgumentOutOfRangeException
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown if _selector is NULL AND T is not an implementation of
    /// ArgumentOutOfRangeException
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if _axisCount is less than 1 AND there are no elements in the
    /// collection, even if T implements <see cref="IMultiDimensionalPoint"/> .
    /// </exception>
    public KDTree( IEnumerable<T> _collection, int _axisCount = -1, Func<T, int, double> _selector = null, Func<T, T, double> _distanceFunction = null )
    {
        if (_collection is KDTree<T>) // we know the internal storage is already a KD Tree- simply copy it
        {
            CopyConstructor( _collection as KDTree<T> );
            return;
        }

        m_distanceFunction = _distanceFunction ?? GetMultiDimensionalDistance;
        if (!IsTMultiDimensional())
        {
            if (_axisCount < 1)
                throw new ArgumentOutOfRangeException( "Axis Count", _axisCount, "Axis Count must be at least 1" );
            if (_selector == null)
                throw new ArgumentNullException( "_selector", "The selector cannot be null if T doesn't inherit IMultiDimensionalPoint" );
        }

        if (_axisCount > 0)
            m_axisCount = _axisCount;
        if (_selector != null)
            m_selector = _selector;

        var array = _collection.ToArray(); // Construct using an array
        m_tree = new SparseLeafBinaryTree<T>( array.Length );
        m_count = array.Length;

        if (m_count > 0)
        {
            if (m_axisCount < 1)
                m_axisCount = (array[0] as IMultiDimensionalPoint).GetAxisCount();

            BuildComparers();
            ConstructFromArray( array, 0, array.Length - 1, 0, 0 );
        }

        if (m_axisCount < 1)
        {
            throw new ArgumentException( "May not specify an empty collection when an explicit Axis Count has not been specified" );
        }
    }

    /// <summary>
    /// Simple copy constructor that assumes the structure of the existing KD Tree is
    /// intact
    /// </summary>
    /// <param name="_toCopy">The KD Tree to copy</param>
    private void CopyConstructor( KDTree<T> _toCopy )
    {
        m_axisCount = _toCopy.m_axisCount;
        m_selector = _toCopy.m_selector;
        m_count = _toCopy.m_count;
        m_distanceFunction = _toCopy.m_distanceFunction;
        m_tree = _toCopy.m_tree.Clone();
    }


    /// <summary>
    /// Recursive function that will construct a KD Tree from a (subset of an) array of
    /// data.
    /// </summary>
    /// <param name="_array">The array containing the data</param>
    /// <param name="_low">The lowest index to process</param>
    /// <param name="_high">The highest index to process (inclusive)</param>
    /// <param name="_currentAxis">The axis that this is used to compare</param>
    /// <param name="_currentIdx">
    /// The node to populate with the median value from this array subset
    /// </param>
    private void ConstructFromArray( T[] _array, int _low, int _high, int _currentAxis, int _currentIdx )
    {
        var len = _high - _low + 1;

        if (len <= 0) // If there's nothing to process, then do nothing
            return;

        if (len == 1) // A single element to process is easy
        {
            m_tree[_currentIdx] = _array[_low];
            return;
        }

        var comparer = m_comparers[_currentAxis];

        // A 2-element subset of the array is somewhat trival
        if (len == 2)
        {
            if (comparer.Compare( _array[_low], _array[_high] ) <= 0)
            {
                m_tree[_currentIdx] = _array[_low];
                m_tree[m_tree.GetRightIndex( _currentIdx )] = _array[_high];
            }
            else
            {
                m_tree[_currentIdx] = _array[_high];
                m_tree[m_tree.GetRightIndex( _currentIdx )] = _array[_low];
            }
            return;
        }

        // The array is 3 or more in length, so use quickselect to find the median,
        // populate _node, and create the sub-trees from the partitioned data
        var mid = ((_high - _low) >> 1) + _low;
        _array.QuickSelect( _low, _high, mid, comparer );

        var nextAxis = (_currentAxis + 1) % m_axisCount;

        m_tree[_currentIdx] = _array[mid];

        var nextIdx = m_tree.GetLeftIndex( _currentIdx );
        ConstructFromArray( _array, _low, mid - 1, nextAxis, nextIdx );
        ConstructFromArray( _array, mid + 1, _high, nextAxis, nextIdx + 1 );
    }


    /// <summary>
    /// Use this method when a significant number of nodes have been added to the tree
    /// after its construction. This has the effect of re-balancing the tree. This is
    /// not a cheap operation - O(n log n)
    /// </summary>
    public void RebuildTree() => RebuildTree( m_count );

    /// <summary>
    /// Use this method when a significant number of nodes have been added to the tree
    /// after its construction. This has the effect of re-balancing the tree. This is
    /// not a cheap operation - O(n log n)
    /// </summary>
    public void RebuildTree( int _capacity )
    {
        var items = new T[m_count];
        var idx = 0;
        foreach (var item in this)
            items[idx++] = item;

        m_tree = new SparseLeafBinaryTree<T>( _capacity );
        BuildComparers();
        ConstructFromArray( items, 0, items.Length - 1, 0, 0 );
    }

    /// <summary>
    /// Clone this tree. This operation will NOT balance the cloned tree.
    /// </summary>
    /// <returns>
    /// A copy of this KD-Tree, but NOT a deep-copy of the data found in the tree
    /// </returns>
    public KDTree<T> Clone() => new KDTree<T>( this );

    /// <summary>
    /// Allows the application to set the distance function for the KD Tree. Setting
    /// this function will speed up performance. If this function is not set, then the
    /// default mechanism (either using <see cref="IMultiDimensionalPoint"/> or using
    /// the Selector to determine distance in a generic way will be used.
    /// </summary>
    /// <param name="_fn">
    /// The function used to determine the distance between two points of type T. The
    /// distance returned should represent the SQUARE of the euclidean distance.
    /// </param>
    public void SetDistanceFunction( Func<T, T, double> _fn ) => m_distanceFunction = _fn;








    /// <summary>
    /// Find the nearest element in the KD Tree to the specified element.
    /// </summary>
    /// <param name="_item">The item to search for</param>
    /// <returns>The closest node in the KD tree to the node specified</returns>
    public T FindNearest( T _item )
    {
        if (ExploredNodes != null)
            ExploredNodes.Clear();

        var closest = new ClosestObjects<T>();
        FindNearest( _item, 0, 0, closest );
        return closest.GetClosestSingle();
    }

    /// <summary>
    /// Find the nearest N elements in the KD Tree to the specified element.
    /// </summary>
    /// <param name="_item">The item to search for</param>
    /// <param name="_count">The number of closest elements to return</param>
    /// <returns>
    /// An enumeration of the closest N elements to the item specified
    /// </returns>
    public IEnumerable<T> FindNearest( T _item, int _count )
    {
        if (ExploredNodes != null)
            ExploredNodes.Clear();

        var closest = new ClosestObjects<T>( _count );
        FindNearest( _item, 0, 0, closest );
        return closest.GetClosestObjects();
    }

    /// <summary>
    /// Find all items that are within a certain distance of an item
    /// </summary>
    /// <param name="_item">The item to find close other items for</param>
    /// <param name="_maxDistance">The maximum distance from _item to include</param>
    /// <returns>
    /// An enumeration of objects that are closer than _distance to _item
    /// </returns>
    public IEnumerable<T> FindClose( T _item, double _maxDistance )
    {
        if (ExploredNodes != null)
            ExploredNodes.Clear();

        var closest = new ClosestObjects<T>( _maxDistance );
        FindNearest( _item, 0, 0, closest );
        return closest.GetClosestObjects();
    }

    /// <summary>
    /// Internal method for finding the closest N items to some item
    /// </summary>
    /// <param name="_item">The item to search for</param>
    /// <param name="_nodeIdx">The index of the node to start searching</param>
    /// <param name="_axis">The axis of the node at the index specified</param>
    /// <param name="_closest">A collection of close and valid nodes</param>
    private void FindNearest( T _item, int _nodeIdx, int _axis, ClosestObjects<T> _closest )
    {
        var currentItem = m_tree[_nodeIdx];

        // we are below a leaf node, so the parent IS the leaf node to kick off the
        // unwinding of the recursion
        if (currentItem == null)
            return;

        var nextAxis = (_axis + 1) % m_axisCount;
        var itemCoord = m_selector( _item, _axis );
        var currentNodeCoord = m_selector( currentItem, _axis );

        var nextIdx = m_tree.GetLeftIndex( _nodeIdx );
        if (itemCoord > currentNodeCoord) // actually need the...
            nextIdx++; // ... right child

        FindNearest( _item, nextIdx, nextAxis, _closest );

        var thisDist = m_distanceFunction( currentItem, _item );
        var worstClosestDistance = _closest.Add( currentItem, thisDist );

        if (ExploredNodes != null) // for debugging
            ExploredNodes.Add( currentItem );

        var distFromAxis = itemCoord - currentNodeCoord;

        // This deals with the SQUARE of the distances- Could be more flexible
        distFromAxis *= distFromAxis;

        // The item is closer to the axis than the worst closest node in _closest
        if (distFromAxis < worstClosestDistance)
        {
            // so now we need to check the opposite side of the tree at the current node

            // The sibling is either one element after, or one element before, depending
            // on the coordinate relative to "_item"'s coordinate
            var siblingIndex = (itemCoord > currentNodeCoord) ? nextIdx - 1 : nextIdx + 1;

            // ... and go down that side of the tree
            FindNearest( _item, siblingIndex, nextAxis, _closest );
        }
    }

    /// <summary>
    /// Add a new item to this KD-Tree. This is not a balancing operation, so adding
    /// nodes may result in the KD-Tree becoming unbalanced quickly. Use "GetDepthFill"
    /// to determine the fill factor of lower depths.
    /// </summary>
    /// <param name="_item">The item to add to the tree</param>
    public void Add( T _item )
    {
        if (_item == null)
            throw new ArgumentNullException( "Not allowed to add NULL to a KD Tree" );

        m_count++;

        // Empty tree- easy mode
        if (m_tree[0] == null)
        {
            m_tree[0] = _item;
            return;
        }

        var idx = 0;
        var axis = 0;

        // Non-recursive Add loop
        while (true)
        {
            try
            {
                var cItem = m_selector( _item, axis );
                var cNode = m_selector( m_tree[idx], axis );
                axis = (axis + 1) % m_axisCount;

                if (cItem <= cNode)
                {
                    var iLeft = m_tree.GetLeftIndex( idx );
                    if (m_tree[iLeft] == null)
                    {
                        m_tree[iLeft] = _item;
                        return;
                    }
                    idx = iLeft;
                }
                else
                {
                    var iRight = m_tree.GetRightIndex( idx );
                    if (m_tree[iRight] == null)
                    {
                        m_tree[iRight] = _item;
                        return;
                    }
                    idx = iRight;
                }
            }
            // Thrown when the tree gets way to out-of-balance and the indicies get
            // larger than 2^31
            catch (IndexOutOfRangeException)
            {
                // remove the count that was added above
                m_count--;
                // ReBuild the whole tree, which could reduce the depth of the tree drastically.
                RebuildTree( m_count );
                // And add the item back to the rebuilt tree
                Add( _item );

                return;
            }
        }

    }

    /// <summary>
    /// Find the index of the item with the smallest coordinate for a specified axis
    /// </summary>
    /// <param name="_index">The index to start searching on</param>
    /// <param name="_targetAxis">The axis that is interesting</param>
    /// <param name="_axis">The current axis of the search</param>
    /// <returns>
    /// -1 if the index specified doesn't have a node associated with it, otherwise the
    /// index of the item with the smallest coordinate for the specified axis
    /// </returns>
    private int FindMin( int _index, int _targetAxis, int _axis )
    {
        var current = m_tree[_index];
        if (current == null)
            return -1;

        var leftIdx = m_tree.GetLeftIndex( _index );
        var nextAxis = (_axis + 1) % m_axisCount;

        if (_axis == _targetAxis)
        {
            var left = FindMin( leftIdx, _targetAxis, nextAxis );
            if (left == -1)
                left = _index;
            return left;
        }
        else
        {
            var leftMinIdx = FindMin( leftIdx, _targetAxis, nextAxis );
            var rightMinIdx = FindMin( leftIdx + 1, _targetAxis, nextAxis );

            var leftVal = (leftMinIdx > -1) ? m_selector( m_tree[leftMinIdx], _targetAxis ) : double.MaxValue;
            var rightVal = (rightMinIdx > -1) ? m_selector( m_tree[rightMinIdx], _targetAxis ) : double.MaxValue;
            var thisVal = m_selector( m_tree[_index], _targetAxis );

            if (leftVal < rightVal)
            {
                if (leftVal < thisVal)
                    return leftMinIdx;
                else
                    return _index;
            }
            else if (rightVal < thisVal)
            {
                return rightMinIdx;
            }
            else
            {
                return _index;
            }
        }
    }

    /// <summary>
    /// Find the item with the smallest value for a given axis
    /// </summary>
    /// <param name="_axis">The axis to search for the lowest</param>
    /// <returns>
    /// -1 if the tree is empty, otherwise the item with the smallest coordinate for the
    /// specified axis
    /// </returns>
    public T FindMin( int _axis )
    {
        var idx = FindMin( 0, _axis, 0 );
        if (idx == -1)
            return default;
        else
            return m_tree[idx];
    }

    /// <summary>
    /// Find the index of the item with the largest coordinate for a specified axis
    /// </summary>
    /// <param name="_index">The index to start searching on</param>
    /// <param name="_targetAxis">The axis that is interesting</param>
    /// <param name="_axis">The current axis of the search</param>
    /// <returns>
    /// -1 if the index specified doesn't have a node associated with it, otherwise the
    /// index of the item with the largest coordinate for the specified axis
    /// </returns>
    private int FindMax( int _index, int _targetAxis, int _axis )
    {
        var current = m_tree[_index];
        if (current == null)
            return -1;

        var leftIdx = m_tree.GetLeftIndex( _index );
        var nextAxis = (_axis + 1) % m_axisCount;

        if (_axis == _targetAxis)
        {
            var right = FindMax( leftIdx + 1, _targetAxis, nextAxis );
            if (right == -1)
                right = _index;
            return right;
        }
        else
        {
            var leftMinIdx = FindMax( leftIdx, _targetAxis, nextAxis );
            var rightMinIdx = FindMax( leftIdx + 1, _targetAxis, nextAxis );

            var leftVal = (leftMinIdx > -1) ? m_selector( m_tree[leftMinIdx], _targetAxis ) : double.MinValue;
            var rightVal = (rightMinIdx > -1) ? m_selector( m_tree[rightMinIdx], _targetAxis ) : double.MinValue;
            var thisVal = m_selector( m_tree[_index], _targetAxis );

            if (leftVal > rightVal)
            {
                if (leftVal > thisVal)
                    return leftMinIdx;
                else
                    return _index;
            }
            else if (rightVal > thisVal)
            {
                return rightMinIdx;
            }
            else
            {
                return _index;
            }
        }
    }

    /// <summary>
    /// Find the item with the largest value for a given axis
    /// </summary>
    /// <param name="_axis">The axis to search for the largest</param>
    /// <returns>
    /// -1 if the tree is empty, otherwise the item with the largest coordinate for the
    /// specified axis
    /// </returns>
    public T FindMax( int _axis )
    {
        var idx = FindMax( 0, _axis, 0 );
        if (idx == -1)
            return default;
        else
            return m_tree[idx];
    }

    /// <summary>
    /// Remove a node from the KD Tree. This does NOT balance the tree
    /// </summary>
    /// <param name="_item">
    /// The item to remove- This must be an exact reference to the item
    /// </param>
    /// <returns>TRUE if an item was removed, FALSE if no item was found</returns>
    public bool Remove( T _item )
    {//test me
        var idx = FindIndex( _item, 0 );
        if (idx == -1) // the item wasn't found
            return false;

        m_count--;

        while (true) // continue deleting/moving items until a leaf node was found
        {
            // Try to find a replacement node by looking for the MAX for this axis in
            // the left subtree, or the MIN in the right
            var axis = m_tree.GetDepth( idx ) % m_axisCount; // This axis used for determining MIN/MAX
            var nextAxis = (axis + 1) % m_axisCount; // The next axis to begin searching
            var leftIdx = m_tree.GetLeftIndex( idx ); // The index of the left sub-tree

            // Look for the min in the right, or the max in the left (if the right
            // doesn't exist)
            var nextIdx = FindMax( leftIdx, axis, nextAxis );
            if (nextIdx == -1)
                nextIdx = FindMin( leftIdx + 1, axis, nextAxis );

            if (nextIdx == -1) // This was actually a leaf node
            {
                m_tree[idx] = default; // simply remove it
                return true;
            }

            // now, nextIdx contains the index of the value that will replace this
            // deleted node
            m_tree[idx] = m_tree[nextIdx];
            idx = nextIdx; // now, go to the top of the loop to delete the replacement node
        }
    }

    /// <summary>
    /// Clear this KD-Tree. This operation is not recommended, as filling an empty tree
    /// node-by-node can be an expensive operation and lead to largely unbalanced trees.
    /// </summary>
    public void Clear()
    {
        m_count = 0;
        m_tree.Clear();
    }

    /// <summary>
    /// Determine if an item exists in a tree. This is a ReferenceEquals operation, NOT
    /// a comparison of coordinates. This is an O(lgN) operation.
    /// </summary>
    /// <param name="_item">The item to look for</param>
    /// <returns>True of the item exists in the tree, FALSE if not</returns>
    public bool Contains( T _item ) => FindIndex( _item ) != -1;

    /// <summary>
    /// Return the index of a specified item in the tree. This is a ReferenceEquals
    /// operation, NOT a comparison of coordinates. This is an O(lgN) operation.
    /// </summary>
    /// <param name="_item">The item to look for</param>
    /// <param name="_startingIndex">
    /// The index of the node to start searching for the item. Defaults to the root (0)
    /// </param>
    /// <returns>The index of the item, or -1 if the item is not in the tree</returns>
    protected int FindIndex( T _item, int _startingIndex = 0 )
    {
        var idx = _startingIndex;
        var axis = 0;
        var coords = new double[m_axisCount];
        for (var i = 0; i < m_axisCount; i++)
            coords[i] = m_selector( _item, i );

        while (true)
        {
            var current = m_tree[idx];
            if (current == null)
                return -1;
            if (current.Equals( _item ))
                return idx;

            idx = m_tree.GetLeftIndex( idx );
            var currentVal = m_selector( current, axis );

            if (coords[axis] > currentVal) // If the value is greater than this node's value
            {
                idx++; // really need right index
            }
            else if (coords[axis] == currentVal) // If its equal, then we unfortunately need to search both sides
            {
                var altIdx = FindIndex( _item, idx + 1 ); // Search the right side of the node
                if (altIdx != -1) // If the item does exist in the right side, then
                    return altIdx; // we can return that value
            }
            axis = (axis + 1) % m_axisCount;
        }
    }

    /// <summary>
    /// The number of items placed in the tree
    /// </summary>
    public int Count => m_count;

    /// <summary>
    /// Copy the tree elements (in no particular order) to an array
    /// </summary>
    /// <param name="_array">The array to receive the items</param>
    /// <param name="_arrayIndex">The starting index in the array</param>
    public void CopyTo( T[] _array, int _arrayIndex )
    {
        var idx = _arrayIndex;
        foreach (var item in this)
        {
            if (idx >= _array.Length)
                break;
            _array[idx++] = item;
        }
    }

    /// <summary>
    /// The KD-Tree is not read-only
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// An enumerator that returns items in no particular order
    /// </summary>
    /// <returns>An enumeration of items in no particular order</returns>
    public IEnumerator<T> GetEnumerator() => m_tree.GetEnumerator();

    /// <summary>
    /// An enumerator that returns items in no particular order
    /// </summary>
    /// <returns>An enumeration of items in no particular order</returns>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => ((IEnumerable<T>) this).GetEnumerator();


    /// <summary>
    /// Return a string that contains the fill levels of each "layer" of the tree
    /// </summary>
    /// <returns></returns>
    public string GetDepthFillString()
    {
        var counts = m_tree.GetFillLevels();
        var str = new StringBuilder();

        str.Append( "Total Nodes: " ).Append( m_count ).AppendLine();

        for (var i = 0; i < counts.Count; i++)
        {
            var fullCount = 1 << i;
            str.AppendFormat( "[{0}] = {1:N0} / {2:N0}      {3:N6}", i, counts[i], fullCount, (double) counts[i] / fullCount ).AppendLine();
            if (i == m_tree.RootDepth)
                str.AppendLine( "----------------------" );
        }
        return str.ToString();
    }

}
