using System;
using System.Collections.Generic;
using System.Linq;


namespace Morpheus
{
    /// <summary>
    /// A Sparse Leaf Binary Tree has a compact set of "roots" coupled with a sparse set of
    /// "leaves"
    /// </summary>
    /// <remarks>
    /// The Sparse Leaf Binary Tree is composed of two parts.
    /// 
    /// The "Roots" are implemented as an array of elements that have the general access
    /// methods used in a Binary Heap, forming a 'complete binary tree'
    /// 
    /// The leaves are implemented as a Dictionary, forming the sparse nature of the bottom
    /// of the tree.
    /// 
    /// There are no assumptions made about ordering- therefore, this is NOT a binary heap.
    /// 
    /// The construction of this data structure is what forms the "roots" of the tree.
    /// Elements added after construction can either be added to the roots IFF they fit onto
    /// the last level of the roots, or they are added to the dictionary if they are deeper
    /// than the roots.
    /// 
    /// This data structure should be used when the lg(N) nature of a binary tree is
    /// desired, when a base set of data is available from which to construct the roots, and
    /// newly added elements are not expected to be frequent nor numerous, and may cause the
    /// tree to become (possibly significantly) unbalanced.
    /// 
    /// The implementation of the T[] for the roots saves the extra memory overhead of the
    /// index key in the Dictionary. When you know that there are at least N elements, those
    /// roots can be pre-allocated and compressed.
    /// 
    /// If the tree will be very small, a Dictionary-based solution would be better, as the
    /// memory overhead of the indicies would be negligible. But for a tree with millions or
    /// more nodes, the memory overhead can be reduced with this type of tree.
    /// 
    /// NOTE This may seem like an overly specific data structure- it is! It is meant to
    /// optimize the KDTree for searching
    /// </remarks>
    public class CSparseLeafBinaryTree<T> : IEnumerable<T>
    {
        private T[] m_root;
        private readonly Dictionary<int, T> m_leaves = new Dictionary<int, T>();


        /// <summary>
        /// Construct the binary tree with an initial capacity. This capacity determines how
        /// large the "roots" will be. It will be rounded up to (the nearest power of 2)
        /// minus 1
        /// </summary>
        /// <param name="_capacity">The capacity of the roots</param>
        public CSparseLeafBinaryTree( int _capacity )
        {
            var rootSize = 1 << (_capacity.Log2Int() + 1 - 1);
            rootSize = Math.Max( rootSize, 255 ); // Optimized to provide at least a tree that's 8 deep (255 elements)
            m_root = new T[rootSize];
        }

        /// <summary>
        /// The depth (zero-based) of the roots of the tree. Items will be in the roots down
        /// to this depth.
        /// </summary>
        public int RootDepth => (m_root.Length + 1).Log2Int();

        /// <summary>
        /// Access the elements in the binary tree by index. It doesn't matter if the index
        /// is in the roots or in the leaves.
        /// </summary>
        /// <param name="_index">The index. There are no out-of-range exceptions.</param>
        /// <returns>
        /// The element stored at this index, or NULL if nothing is stored there.
        /// </returns>
        public T this[int _index]
        {
            get
            {
                if (_index < m_root.Length)
                {
                    return m_root[_index];
                }
                else
                {
                    m_leaves.TryGetValue( _index, out var retval );
                    return retval;
                }
            }
            set
            {
                if (_index < m_root.Length)
                    m_root[_index] = value;
                else
                    m_leaves[_index] = value;
            }
        }

        /// <summary>
        /// Clear out all elements of the roots and leaves. This leaves the size of the
        /// roots intact.
        /// </summary>
        /// <remarks>
        /// This is optimized to assume that the m_root array has been aged to a later GC
        /// generation. Were m_root not aged (a short-lived object), this routine would
        /// likely be faster by simply re-allocating the m_root array. Potato-potahto
        /// </remarks>
        public void Clear()
        {
            Array.Clear( m_root, 0, m_root.Length );
            m_leaves.Clear();
        }

        /// <summary>
        /// Get the index of the Left child of a given index
        /// </summary>
        /// <param name="_index"></param>
        /// <returns></returns>
        public int GetLeftIndex( int _index ) => (_index << 1) + 1;

        /// <summary>
        /// Get the Left child of an item at a specific index
        /// </summary>
        /// <param name="_index"></param>
        /// <returns></returns>
        public T GetLeft( int _index )
        {
            var idx = (_index << 1) + 1;
            return this[idx];
        }

        /// <summary>
        /// Get the index of the Right child of a given index
        /// </summary>
        /// <param name="_index"></param>
        /// <returns></returns>
        public int GetRightIndex( int _index ) => (_index + 1) << 1;

        /// <summary>
        /// Get the Right child of an item at a specific index
        /// </summary>
        /// <param name="_index"></param>
        /// <returns></returns>
        public T GetRight( int _index )
        {
            var idx = (_index + 1) << 1;
            return this[idx];
        }

        /// <summary>
        /// Get the index of the Parent of a given index
        /// </summary>
        /// <param name="_index"></param>
        /// <returns></returns>
        public int GetParentIndex( int _index ) => (_index - 1) >> 1;

        /// <summary>
        /// Get the Parent object of an item at a specific index
        /// </summary>
        /// <param name="_index"></param>
        /// <returns></returns>
        public T GetParent( int _index )
        {
            var idx = (_index - 1) >> 1;
            return this[idx];
        }

        /// <summary>
        /// Get the index of a given index's sibling
        /// </summary>
        /// <param name="_index"></param>
        /// <returns></returns>
        public int GetSiblingIndex( int _index )
        {
            if ((_index & 1) == 1) // odd number
                return _index + 1;
            else // even number
                return _index - 1;
        }

        /// <summary>
        /// Get the Sibling object of an item at a specific index
        /// </summary>
        /// <param name="_index"></param>
        /// <returns></returns>
        public T GetSibling( int _index ) => this[GetSiblingIndex( _index )];

        /// <summary>
        /// Get the zero-based depth of a given index
        /// </summary>
        /// <param name="_index"></param>
        /// <returns></returns>
        public int GetDepth( int _index ) => (_index + 1).Log2Int();

        /// <summary>
        /// Get the maximum number of elements that can be in a binary tree of a given
        /// depth. This is invariant to the actual instantiation of any specific tree.
        /// </summary>
        /// <param name="_depth">
        /// The tree-depth you want the maximum size (containing capacity) of
        /// </param>
        /// <returns>
        /// The maxiumum number of items that can be possibly stored in a tree of the given
        /// depth
        /// </returns>
        public int GetSizeForDepth( int _depth ) => 1 << _depth;

        /// <summary>
        /// Returns the depth (zero-based) of deepest element in the tree.
        /// </summary>
        /// <returns></returns>
        public int GetMaxDepth()
        {
            int idx;
            if (m_leaves.Count > 0)
            {
                idx = m_leaves.Keys.Max();
            }
            else
            {
                idx = m_root.Length - 1;
                while (m_root[idx] == null && idx > 0)
                    idx--;
            }
            return GetDepth( idx );
        }

        /// <summary>
        /// Create a shallow copy of this binary tree
        /// </summary>
        /// <returns></returns>
        public CSparseLeafBinaryTree<T> Clone()
        {
            var clone = new CSparseLeafBinaryTree<T>( m_root.Length )
            {
                m_root = (T[]) m_root.Clone()
            };
            foreach (var kv in m_leaves)
                clone.m_leaves[kv.Key] = kv.Value;
            return clone;
        }

        /// <summary>
        /// Get an enumeration of all elements in this binary tree in no particular order
        /// </summary>
        /// <returns>An IEnumerable of all items in this binary tree"/></returns>
        public IEnumerator<T> GetEnumerator() => m_root.Where( _x => _x != null ).Union( m_leaves.Values ).GetEnumerator();

        /// <summary>
        /// Get an enumeration of all elements in this binary tree in no particular order
        /// </summary>
        /// <returns>An IEnumerable of all items in this binary tree"/></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Return an IList of counts representing the number of elements found at each
        /// level of the tree
        /// </summary>
        /// <returns>
        /// An IList of counts representing the number of elements found at each level of
        /// the tree
        /// </returns>
        public IList<int> GetFillLevels()
        {
            var maxDepth = GetMaxDepth();
            var fillLevels = new int[maxDepth + 1];

            for (var i = 0; i < m_root.Length; i++)
            {
                if (m_root[i] != null)
                {
                    var depth = GetDepth( i );
                    fillLevels[depth]++;
                }
            }
            foreach (var i in m_leaves.Keys)
                fillLevels[GetDepth( i )]++;

            return fillLevels;
        }
    }
}
