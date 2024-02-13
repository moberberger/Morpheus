namespace Morpheus;


/// <summary>
/// An optimized Priority Queue that maintains internal data structures allowing
/// more efficient operations than <see cref="PriorityQueue&lt;T> "/> without
/// introducing external dependencies on the data type stored like
/// <see cref="PriorityQueueOptimized&lt;T> "/> does.
/// 
/// This class requires more than twice the memory overhead of the
/// <see cref="PriorityQueue&lt;T> "/> class, so it should only be used if the
/// faster operations will be relied upon.
/// </summary>
/// <typeparam name="T">The Type of the data stored in the queue</typeparam>
/// <remarks> This version of the <see cref="PriorityQueue&lt;T> "/> maintains a
/// separate dictionary that allows quick lookup of the index within the binary
/// heap for an object. This provides rapid access to a few other operations of
/// the data structure.
/// 
/// <code>Operation Big-O Improvement on CPriorityQueue
/// ------------------------------------------------------------- 
/// Add         lg(N)
/// RemoveAny   lg(N)
/// RemoveFront lg(N)
/// PeekFront   1
/// Sort        N*lg(N)
/// Find/Contains 1
/// UpdateKey   lg(N)
/// Count       1
/// Enumerate   N*lg(N)
/// </code></remarks>
public class PriorityQueueExtended<T> : PriorityQueue<T>
{
    private Dictionary<T, int> m_lookup;

    /// <summary>
    /// Construct a new priority queue, using the default comparator for objects
    /// </summary>
    public PriorityQueueExtended()
        : base()
    {
    }

    /// <summary>
    /// Construct a new priority queue, using the default comparator for objects
    /// if none is specified
    /// </summary>
    /// <param name="_comparer">
    /// The comparer to use for collation/ordering
    /// </param>
    public PriorityQueueExtended( IComparer<T> _comparer )
        : base( _comparer )
    {
    }

    /// <summary>
    /// Construct a new priority queue with an initialized capacity, optionally
    /// specifying a comparator
    /// </summary>
    /// <param name="_initialCapacity">
    /// The initial capacity of the heap, used to help reduce memory
    /// re-allocation
    /// </param>
    /// <param name="_comparer">
    /// The comparer to use for collation/ordering
    /// </param>
    public PriorityQueueExtended( int _initialCapacity, IComparer<T> _comparer = null )
        : base( _initialCapacity, _comparer )
    {
    }

    /// <summary>
    /// Construct the priority queue from an existing collection of objects.
    /// </summary>
    /// <param name="_collection">The collection of objects</param>
    /// <param name="_comparer">
    /// The comparer to use for collation/ordering
    /// </param>
    /// <remarks>
    /// Relies heavily on the base-class implementation of
    /// "ConstructFromEnumerable" to determine an optimal method of constructing
    /// a queue from a collection.
    /// </remarks>
    public PriorityQueueExtended( IEnumerable<T> _collection, IComparer<T> _comparer = null )
    {
        if (_comparer != null)
            m_comparer = _comparer;

        if (!ConstructFromEnumerable( _collection )) // FALSE implies that I need to create my Dictionary
        {
            for (var i = 0; i < Count; i++)
                m_lookup[m_heap[i]] = i;
        }
    }

    /// <summary>
    /// Allocate the dictionary
    /// </summary>
    /// <param name="_capacity">
    /// The capacity hint: Positive, specify an initial Dictionary capacity,
    /// otherwise don't
    /// </param>
    protected override void Allocate( int _capacity )
    {
        base.Allocate( _capacity );

        if (_capacity > 0)
            m_lookup = new Dictionary<T, int>( _capacity );
        else
            m_lookup = new Dictionary<T, int>();
    }

    /// <summary>
    /// Helper function to swap two objects in the heap. All base class
    /// operations that could affect the index of an element in the heap MUST
    /// use the "Swap" function to maintain any helper data structures with
    /// optimization
    /// </summary>
    /// <param name="_index1">First object to swap</param>
    /// <param name="_index2">Second object to swap</param>
    protected override void Swap( int _index1, int _index2 )
    {
        var o1 = m_heap[_index1];
        var o2 = m_heap[_index2];

        m_lookup[o1] = _index2;
        m_lookup[o2] = _index1;

        base.Swap( _index1, _index2 );
    }

    /// <summary>
    /// Remove all elements from this collection
    /// </summary>
    public override void Clear()
    {
        m_lookup.Clear();
        base.Clear();
    }

    /// <summary>
    /// Add a new node to the heap, and prioritize it
    /// </summary>
    /// <remarks>
    /// This implements the <see cref="ICollection&lt;T> "/> "Add" function
    /// </remarks>
    /// <param name="_newObject">The object to add</param>
    public override void Add( T _newObject )
    {
        var idx = AddReturnIndex( _newObject );
        m_lookup[_newObject] = idx;
    }

    /// <summary>
    /// Remove the "lowest" node from the heap, adjusting it appropriately
    /// </summary>
    /// <returns>The "lowest" node.</returns>
    public override T RemoveLowest()
    {
        var node = base.RemoveLowest();
        if (node != null)
            m_lookup.Remove( node );

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

        if (!m_lookup.TryGetValue( _element, out var idx ))
            return false;

        RemoveAt( idx );
        m_lookup.Remove( _element );
        return true;
    }

    /// <summary>
    /// Does the collection contain this element?
    /// </summary>
    /// <param name="_element">The element to check</param>
    /// <returns>
    /// TRUE if the element is in the collection, FALSE if not
    /// </returns>
    public override bool Contains( T _element )
    {
        if (_element == null)
            return false;
        return m_lookup.ContainsKey( _element );
    }

    /// <summary>
    /// When the Priority of an element changes, this method will re-organize
    /// the queue considering the new priority for the element. This will adjust
    /// the node's position in the collection based on its new value. If the
    /// node does not exist, it will be added to the priority queue.
    /// </summary>
    /// <param name="_element">The node whose value has changed.</param>
    public override void Update( T _element )
    {
        if (_element == null)
            throw new ArgumentNullException( "Cannot Update a NULL element" );

        if (!Contains( _element )) // not in the heap, so just Add it
        {
            m_lookup[_element] = AddReturnIndex( _element );
        }
        else // we assume its ordering has changed, thus making it subject to bubbling up or down
        {
            var currentIndex = m_lookup[_element];
            BubbleUp( currentIndex ); // try bubbling it up
            if (m_lookup[_element] == currentIndex) // it was not moved
                PushDown( currentIndex ); // It didn't bubble up, so move it down
        }
    }

}
