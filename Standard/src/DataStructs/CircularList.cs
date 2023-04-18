using System.Collections;

namespace Morpheus;



/// <summary>
/// A Circular list is fixed size and indexing operations adjust the index to fit inside the
/// list. For example, for a collection of 5 elements, index 6 would reference the element at
/// location 1 (5 % 6) in the list. Similarly, Index -1 would reference the last element of the
/// list, representing a "wrap-around" semantic.
/// </summary>
public class CircularList<T> : IList<T>
{
    private IList<T> _list;

    /// <summary>
    /// Constructing with an IList passes the IList behavior to this class's implementation
    /// </summary>
    /// <param name="list"></param>
    public CircularList( IList<T> list ) => _list = list;

    /// <summary>
    /// Constructing with an enumeration creates a fixed-size implementation which will throw an
    /// exception if a list size modifying action (i.e. Add, Remove) is attempted.
    /// </summary>
    /// <param name="collection"></param>
    public CircularList( IEnumerable<T> collection ) => _list = collection.ToArray();

    public T this[int index]
    {
        get => _list[SafeIndex( index )];
        set => _list[SafeIndex( index )] = value;
    }

    public int SafeIndex( int index )
    {
        var x = index % Count;
        if (x < 0) x += Count;
        return x;
    }

    public int Count => _list.Count;

    public bool IsReadOnly => _list.IsReadOnly;

    public int IndexOf( T item ) => _list.IndexOf( item );

    public bool Contains( T item ) => _list.Contains( item );

    public void CopyTo( T[] array, int arrayIndex ) => _list.CopyTo( array, arrayIndex );

    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();



    public void Add( T item ) => _list.Add( item );

    public void Clear() => _list.Clear();

    public void Insert( int index, T item ) => _list.Insert( index, item );

    public bool Remove( T item ) => _list.Remove( item );

    public void RemoveAt( int index ) => _list.RemoveAt( index );
}
