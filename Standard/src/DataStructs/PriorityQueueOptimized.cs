using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morpheus
{
    /// <summary>
    /// If a class implements this interface, it will be able to be used with the Optimized Priority Queue collection
    /// </summary>
    public interface IOptimizedBinaryHeapNode
    {
        /// <summary>
        /// The index within the Binary Heap used by <see cref="PriorityQueueOptimized&lt;T>"/>
        /// </summary>
        int HeapIndex { get; set; }
    }

    /// <summary>
    /// An Optimized Priority Queue requires that all elements stored in the queue implement the
    /// <see cref="IOptimizedBinaryHeapNode"/> interface. This interface allows this version of
    /// the queue to store and retrieve the index within the heap (queue) that each element sits at.
    /// </summary>
    /// <typeparam name="T">Some type that implements IOptimizedBinaryHeapNode</typeparam>
    /// <remarks>
    /// This version offloads the "index" of each object into the object itself. It has the benefit of
    /// having a smaller memory footprint, but the drawback of allowing an object to belong to at most
    /// one COptimizedPriorityQueue. The other optimized version of the priority queue <see cref="PriorityQueueExtended&lt;T>"/>
    /// uses an internal dictionary to maintain these indicies.
    /// </remarks>
    public class PriorityQueueOptimized<T> : PriorityQueue<T>
        where T : IOptimizedBinaryHeapNode
    {
        /// <summary>
        /// Construct a new priority queue, using the default comparator for objects
        /// </summary>
        public PriorityQueueOptimized()
            : base()
        {
        }

        /// <summary>
        /// Construct a new priority queue, using the default comparator for objects if none is specified
        /// </summary>
        /// <param name="_comparer">The comparer to use for collation/ordering</param>
        public PriorityQueueOptimized( IComparer<T> _comparer = null )
            : base( _comparer )
        {
        }

        /// <summary>
        /// Construct a new priority queue with an initialized capacity, optionally specifying a comparator
        /// </summary>
        /// <param name="_initialCapacity">The initial capacity of the heap, used to help reduce memory re-allocation</param>
        /// <param name="_comparer">The comparer to use for collation/ordering</param>
        public PriorityQueueOptimized( int _initialCapacity, IComparer<T> _comparer = null )
            : base( _initialCapacity, _comparer )
        {
        }

        /// <summary>
        /// Construct the priority queue from an existing collection of objects.
        /// </summary>
        /// <param name="_collection">The collection of objects</param>
        /// <param name="_comparer">The comparer to use for collation/ordering</param>
        /// <remarks>
        /// If the collection is a List of T, the capacity of the queue will be set to the capacity of the List. If the 
        /// collection is an Array of T, the capacity of the queue will be set to be 10% greater than the Length of the 
        /// array.
        /// </remarks>
        public PriorityQueueOptimized( IEnumerable<T> _collection, IComparer<T> _comparer = null )
        {
            if (_comparer != null)
                m_comparer = _comparer;

            if (!ConstructFromEnumerable( _collection )) // FALSE implies that I need to create my Dictionary
            {
                for (var i = 0; i < Count; i++)
                    m_heap[i].HeapIndex = i;
            }
        }



        /// <summary>
        /// Helper function to swap two objects in the heap
        /// </summary>
        /// <param name="_index1">First object to swap</param>
        /// <param name="_index2">Second object to swap</param>
        protected override void Swap( int _index1, int _index2 )
        {
            m_heap[_index1].HeapIndex = _index2;
            m_heap[_index2].HeapIndex = _index1;
            base.Swap( _index1, _index2 );
        }

        /// <summary>
        /// Remove all elements from this collection
        /// </summary>
        public override void Clear()
        {
            foreach (var node in m_heap)
                node.HeapIndex = -1;
            base.Clear();
        }

        /// <summary>
        /// Add a new node to the heap, and prioritize it
        /// </summary>
        /// <remarks>
        /// This implements the <see cref="ICollection&lt;T>"/> "Add" function
        /// </remarks>
        /// <param name="_newObject">The object to add</param>
        public override void Add( T _newObject )
        {
            var idx = AddReturnIndex( _newObject );
            _newObject.HeapIndex = idx;
        }

        /// <summary>
        /// Remove the "lowest" node from the heap, adjusting it appropriately
        /// </summary>
        /// <returns>The "lowest" node.</returns>
        public override T RemoveLowest()
        {
            var node = base.RemoveLowest();
            if (node != null)
                node.HeapIndex = -1;
            return node;
        }

        /// <summary>
        /// Remove an element from the queue
        /// </summary>
        /// <param name="_element">The element to remove</param>
        /// <returns>TRUE if the element was removed, FALSE otherwise</returns>
        public override bool Remove( T _element )
        {
            if (_element == null)
                return false;

            var idx = _element.HeapIndex;
            if (idx == -1)
                return false;
            if (idx >= Count)
                return false;

            RemoveAt( idx );
            _element.HeapIndex = -1;
            return true;
        }





        /// <summary>
        /// Does the collection contain this element?
        /// </summary>
        /// <param name="_element">The element to check</param>
        /// <returns>TRUE if the element is in the collection, FALSE if not</returns>
        public override bool Contains( T _element )
        {
            if (_element == null)
                return false;
            var idx = _element.HeapIndex;
            if (idx == -1)
                return false;
            if (idx >= Count)
                return false;

            return object.ReferenceEquals( m_heap[idx], _element );
        }

        /// <summary>
        /// Call when the "value" of a node changes. This will adjust the node's position in the collection based on its new value. If
        /// the node does not exist, it will be added to the collection.
        /// </summary>
        /// <param name="_node">The node whose value has changed.</param>
        public override void Update( T _node )
        {
            if (_node == null)
                throw new ArgumentNullException( "Cannot Update a NULL node" );

            if (!Contains( _node )) // not in the heap, so just Add it
            {
                _node.HeapIndex = AddReturnIndex( _node );
            }
            else // we assume its ordering has changed, thus making it subject to bubbling up or down
            {
                var currentIndex = _node.HeapIndex;
                BubbleUp( currentIndex );
                if (_node.HeapIndex == currentIndex) // it was not moved
                    PushDown( currentIndex );
            }
        }
    }
}
