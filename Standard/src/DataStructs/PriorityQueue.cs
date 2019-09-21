using System;
using System.Collections;
using System.Collections.Generic;

namespace Morpheus
{
    /// <summary>
    /// CPriorityQueue is a binary heap based priority queue that supports rapid adding and
    /// removal of highest priority items, but slow removal of "any" object in the queue.
    /// 
    /// The concept of "Priority" for this class states that the LOWEST value is the HIGHEST
    /// priority. In other words, and by example, the value 1 has a higher priority than the
    /// value 5. The name "Abe" has a higher priority than the name "Ben".
    /// 
    /// Use a different comparer (specified in the constructors) to change this behavior, or to
    /// apply a "selector" on object data. Also reference <see cref="LambdaComparer&lt;T> "/>
    /// to specify a lambda function as a comparer instead of an object that implements
    /// <see cref="IComparer&lt;T> "/>.
    /// </summary>
    /// <remarks><code>
    /// Operation   Big-O 
    /// ---------------------------------------------- 
    /// Add         lg(N)
    /// RemoveAny   N
    /// RemoveFront lg(N)
    /// PeekFront   1
    /// Sort        N*lg(N)
    /// Find/Contains N
    /// UpdateKey   N Count 1
    /// Enumerate   N*lg(N)
    /// </code></remarks>
    public class PriorityQueue<T> : ICollection<T>, IEnumerable<T>
    {
        /// <summary>
        /// The binary heap used to store all the data in the tree
        /// </summary>
        protected List<T> m_heap;

        /// <summary>
        /// The comparer to use when determining collation order
        /// </summary>
        protected IComparer<T> m_comparer = Comparer<T>.Default;

        /// <summary>
        /// Construct a new priority queue, using the default comparator for objects
        /// </summary>
        public PriorityQueue()
        {
            Allocate( 0 );
        }

        /// <summary>
        /// Construct a new priority queue, using the specified comparator for objects
        /// </summary>
        public PriorityQueue( IComparer<T> _comparer )
        {
            if (_comparer != null)
                m_comparer = _comparer;
            Allocate( 0 );
        }

        /// <summary>
        /// Construct a new priority queue with an initialized capacity, optionally specifying a
        /// comparator
        /// </summary>
        /// <param name="_initialCapacity">
        /// The initial capacity of the heap, used to help reduce memory re-allocation
        /// </param>
        /// <param name="_comparer">The comparer to use for collation/ordering</param>
        public PriorityQueue( int _initialCapacity, IComparer<T> _comparer = null )
        {
            if (_comparer != null)
                m_comparer = _comparer;
            Allocate( _initialCapacity );
        }

        /// <summary>
        /// Construct the priority queue from an existing collection of objects.
        /// </summary>
        /// <param name="_collection">The collection of objects</param>
        /// <param name="_comparer">The comparer to use for collation/ordering</param>
        /// <remarks>
        /// If the collection is a List of T, the capacity of the queue will be set to the
        /// capacity of the List. If the collection is an Array of T, the capacity of the queue
        /// will be set to be 10% greater than the Length of the array.
        /// </remarks>
        public PriorityQueue( IEnumerable<T> _collection, IComparer<T> _comparer = null )
        {
            if (_comparer != null)
                m_comparer = _comparer;
            ConstructFromEnumerable( _collection );
        }

        /// <summary>
        /// Set up the heap based on an enumeration of elements. This does some optimizing based
        /// on the type of enumeration given to the Priority Queue. It includes a "copy
        /// constructor", an optimized List-of-T and an Array-of-T optimization. This also
        /// "stores" the comparer, if its not null. Any class deriving from this should use this
        /// helper method in the constructor that accepts an enumeration.
        /// </summary>
        /// <param name="_collection">
        /// The enumeration of objects to construct the PriorityQueue from
        /// </param>
        /// <returns>
        /// TRUE if the "Add" method was used to add a collection to this priority queue, FALSE
        /// if the base-class heap was constructed without using "Add", thereby requiring
        /// deriving classes to establish their own internal data structures.
        /// </returns>
        protected bool ConstructFromEnumerable( IEnumerable<T> _collection )
        {
            if (_collection is PriorityQueue<T>) // we know the internal array is already in good heap order- simply copy it
            {
                var otherPQ = _collection as PriorityQueue<T>;
                if (otherPQ.m_comparer == m_comparer)
                {
                    Allocate( otherPQ.m_heap.Capacity );
                    for (var i = 0; i < otherPQ.m_heap.Count; i++) // Copying the other heap will preserve the heap-order
                        m_heap.Add( otherPQ.m_heap[i] );
                    return false;
                }
                _collection = otherPQ.m_heap; // should trigger "List-of-T" condition below
            }
            if (_collection is List<T>) // The other array is a List, so it has a capacity which we will use
            {
                var otherList = _collection as List<T>;
                Allocate( otherList.Capacity );
                for (var i = 0; i < otherList.Count; i++)
                    m_heap.Add( otherList[i] );
                m_heap.Sort( m_comparer ); // The internal Sort method is likely much faster than our "Heapify"
                return false;
            }
            if (_collection is T[])
            {
                var otherArray = _collection as T[];
                Allocate( -1 ); // allow deriving classes to allocate, but we'll explicitly allocate here
                m_heap = new List<T>( otherArray ); // This does the copy- We don't know about "extra" capacity
                m_heap.Sort( m_comparer ); // The internal Sort method is likely much faster than our "Heapify"
                return false;
            }

            // We aren't doing anything special or optimized, so just allocate and Add
            Allocate( 0 );
            foreach (var x in _collection)
                Add( x );

            return true;
        }

        /// <summary>
        /// Allocate memory for data structures. Assures derived classes allocate memory at the
        /// right time.
        /// </summary>
        /// <param name="_capacity">
        /// Negative: Don't allocate here; 0: Allocate without capacity hints
        /// </param>
        protected virtual void Allocate( int _capacity )
        {
            if (_capacity == 0)
                m_heap = new List<T>();
            else if (_capacity > 0)
                m_heap = new List<T>( _capacity );
        }

        /// <summary>
        /// Gets or sets the total number of elements the internal data structure can hold
        /// without resizing.
        /// </summary>
        public virtual int Capacity
        {
            get => m_heap.Capacity;
            set => m_heap.Capacity = value;
        }


        /// <summary>
        /// The IComparer object used to prioritize objects in the priority queue
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The comparer was set after there were more than 1 element added to the data
        /// structure
        /// </exception>
        public IComparer<T> Comparer
        {
            get => m_comparer;
            set
            {
                if (Count > 1)
                    throw new InvalidOperationException( "Not allowed to change the comparer if there are more than 1 elements in the queue." );
                if (value == null)
                    m_comparer = Comparer<T>.Default;
                else
                    m_comparer = value;
            }
        }

        /// <summary>
        /// Set the comparer for the priority queue using a lambda function.
        /// </summary>
        /// <param name="_comparer">
        /// The lambda function which prioritizes items in the queue
        /// </param>
        public void SetComparisonFunction( Func<T, T, int> _comparer ) => Comparer = LambdaComparer<T>.FromFunc( _comparer );




        /// <summary>
        /// Helper function to retrieve the "Left Child" using the properties of the binary heap
        /// </summary>
        /// <param name="_current">The "parent" to find the child for</param>
        /// <returns>The index of the Left Child</returns>
        protected int GetLeftChild( int _current ) => _current * 2 + 1;

        /// <summary>
        /// Helper function to retrieve the "parent" using the properties of the binary heap
        /// </summary>
        /// <param name="_current">The "child" to find the parent for</param>
        /// <returns>The index of the Parent</returns>
        protected int GetParent( int _current ) => (_current - 1) / 2;

        /// <summary>
        /// Helper function to swap two objects in the heap
        /// </summary>
        /// <param name="_index1">First object to swap</param>
        /// <param name="_index2">Second object to swap</param>
        protected virtual void Swap( int _index1, int _index2 )
        {
            var tmp = m_heap[_index1];
            m_heap[_index1] = m_heap[_index2];
            m_heap[_index2] = tmp;
        }

        /// <summary>
        /// Add a new node to the heap, and prioritize it
        /// </summary>
        /// <param name="_newObject">The object to add</param>
        /// <returns>The index in the heap where the node was added</returns>
        protected int AddReturnIndex( T _newObject )
        {
            m_heap.Add( _newObject );
            var last = m_heap.Count - 1;

            return BubbleUp( last );
        }

        /// <summary>
        /// Designed to move an element "up" the heap (towards the front) after we assume its
        /// "value" has changed.
        /// </summary>
        /// <param name="_index">The index of the element in the heap</param>
        protected int BubbleUp( int _index )
        {
            int parent;
            for (var idx = _index; idx > 0; idx = parent)
            {
                parent = GetParent( idx );
                if (m_comparer.Compare( m_heap[idx], m_heap[parent] ) >= 0)
                    return idx;
                Swap( idx, parent );
            }
            return 0;
        }

        /// <summary>
        /// Push an element "down" the list, away from the top of the list, until it is in an
        /// acceptable position
        /// </summary>
        /// <param name="_index">The index of the element to push down</param>
        /// <returns>The new index of the element within the heap</returns>
        protected int PushDown( int _index )
        {
            var lastIndex = m_heap.Count - 1;
            var moveTo = _index;
            var current = _index;

            while (true)
            {
                var leftChild = GetLeftChild( current );
                if (leftChild > lastIndex)
                    break;

                if (m_comparer.Compare( m_heap[current], m_heap[leftChild] ) >= 0)
                    moveTo = leftChild;

                var rightChild = leftChild + 1;
                if (rightChild <= lastIndex)
                {
                    if (m_comparer.Compare( m_heap[moveTo], m_heap[rightChild] ) >= 0)
                        moveTo = rightChild;
                }

                if (moveTo == current)
                    break;

                Swap( current, moveTo );
                current = moveTo;
            }
            return current;
        }

        /// <summary>
        /// Remove an element at a particular index
        /// </summary>
        /// <param name="_index">The index of the element to remove</param>
        protected void RemoveAt( int _index )
        {
            Swap( _index, m_heap.Count - 1 );
            m_heap.RemoveAt( m_heap.Count - 1 );
            PushDown( _index );
        }











        /// <summary>
        /// Implements Count from ICollection- Return the number of objects in the queue
        /// </summary>
        public int Count => m_heap.Count;

        /// <summary>
        /// A reference to the lowest node. Sort of a "peek" function.
        /// </summary>
        public T LowestNode => (Count > 0) ? m_heap[0] : default;

        /// <summary>
        /// Remove all elements from this collection
        /// </summary>
        public virtual void Clear() => m_heap.Clear();

        /// <summary>
        /// Add a new node to the heap, and prioritize it
        /// </summary>
        /// <remarks>This implements the <see cref="ICollection"/> "Add" function</remarks>
        /// <param name="_newObject">The object to add</param>
        public virtual void Add( T _newObject ) => AddReturnIndex( _newObject );

        /// <summary>
        /// Add a collection of elements to this queue
        /// </summary>
        /// <param name="_collection">
        /// A collection of elements to add to this priority queue
        /// </param>
        public void AddRange( IEnumerable<T> _collection )
        {
            foreach (var x in _collection)
                Add( x );
        }

        /// <summary>
        /// Remove the "lowest" node from the heap, adjusting it appropriately
        /// </summary>
        /// <returns>The "lowest" node.</returns>
        public virtual T RemoveLowest()
        {
            if (m_heap.Count == 0)
                return default;

            var returnValue = m_heap[0];
            if (m_heap.Count == 1)
            {
                m_heap.Clear();
            }
            else
            {
                Swap( 0, m_heap.Count - 1 );
                m_heap.RemoveAt( m_heap.Count - 1 );

                if (m_heap.Count > 1)
                    PushDown( 0 );
            }

            return returnValue;
        }

        /// <summary>
        /// Remove a specific node from the collection
        /// </summary>
        /// <param name="_element">The element to remove</param>
        /// <returns>TRUE if the node was removed, FALSE if not found</returns>
        public virtual bool Remove( T _element )
        {
            for (var i = 0; i < m_heap.Count; i++)
            {
                if (m_comparer.Compare( m_heap[i], _element ) == 0)
                {
                    RemoveAt( i );
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Deetermine if the priority queue contains a given element. This is an O(n)
        /// operation.
        /// </summary>
        /// <param name="_element">The element to check for</param>
        /// <returns>TRUE if the queue contains the element, FALSE if not</returns>
        public virtual bool Contains( T _element ) => m_heap.Contains( _element );

        /// <summary>
        /// When the Priority of an element changes, this method will re-organize the queue
        /// considering the new priority for the element. This will adjust the node's position
        /// in the collection based on its new value. If the node does not exist, it will be
        /// added to the priority queue.
        /// </summary>
        /// <param name="_element">The node whose value has changed.</param>
        public virtual void Update( T _element )
        {
            if (_element == null)
                throw new ArgumentNullException( "Cannot Update a NULL element" );

            var idx = m_heap.IndexOf( _element );  // O(N) operation!
            if (idx == -1) // the element is not in the queue
            {
                AddReturnIndex( _element );
            }
            else
            {
                var newIdx = BubbleUp( idx );
                if (newIdx == idx) // it didn't bubble up, so...
                    PushDown( idx ); // push it down instead
            }
        }

        /// <summary>
        /// Implements CopyTo from ICollection
        /// </summary>
        /// <param name="_targetArray">The destination array</param>
        /// <param name="_index">The base index in the destination array</param>
        public void CopyTo( T[] _targetArray, int _index )
        {
            foreach (var obj in this)
            {
                if (_index >= _targetArray.Length)
                    return;
                _targetArray[_index++] = obj;
            }
        }

        /// <summary>
        /// Returns FALSE for this implementation- the priority queue is never read-only
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Get the enumerator for the base heap. This will return an "ordered" list.
        /// </summary>
        /// <returns>An Enumerator</returns>
        public IEnumerator<T> GetEnumerator()
        {
            var useQueue = new PriorityQueue<T>( this, Comparer );
            while (useQueue.Count > 0)
                yield return useQueue.RemoveLowest();
        }

        /// <summary>
        /// Other enumerator from base IEnumerator- This is also an ordered list
        /// </summary>
        /// <returns>An enumerator over the heap</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
