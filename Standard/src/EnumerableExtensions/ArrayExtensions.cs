#nullable disable

namespace Morpheus;



/// <summary>
/// A bunch of extensions for <see cref="IEnumerable{T}"/> objects.
/// </summary>
public static class ArrayExtensions
{
    /// <summary>
    /// Return the last item in a strongly typed array.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of the array</typeparam>
    /// <param name="_array">The array whose last item is to be returned</param>
    /// <returns>The last item in the array</returns>
    /// <exception cref="ArgumentNullException">
    /// If used as a static function with a null collection
    /// </exception>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown if the array has zero length.
    /// </exception>
    /// <remarks>
    /// <para> Sample Usage: </para>
    /// <code>
    /// var arr = new int[] { 1, 2, 3 };
    /// var x = arr.LastItem();
    /// Assert.AreEqual(arr[2], x);
    /// </code>
    /// </remarks>
    public static T Last<T>( this T[] _array )
    {
        if (_array is null)
            throw new ArgumentNullException( nameof( _array ) );
        // Throw an IndexOutOfRangeException if the list is empty
        return _array[_array.Length - 1];
    }

    /// <summary>
    /// Return the last element of a strongly typed list.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list</typeparam>
    /// <param name="_list">The list to get the last element of</param>
    /// <returns>The last element in the list.</returns>
    /// <exception cref="ArgumentNullException">
    /// If used as a static function with a null collection
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the List is empty</exception>
    /// <remarks>
    /// <code>
    /// var arr = new List&lt;int> { 1, 2, 3 };
    /// var x = arr.Last();
    /// Assert.AreEqual( arr[2], x );
    /// </code>
    /// </remarks>
    public static T Last<T>( this IList<T> _list )
    {
        if (_list is null)
            throw new ArgumentNullException( nameof( _list ) );
        // Throw an ArgumentOutOfRangeException if the list is empty
        return _list[_list.Count - 1];
    }

    /// <summary>
    /// Simply remove the last item from a List. Throws an exception if the list is empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list</typeparam>
    /// <param name="_list">The list to remove the last element of</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the List is empty</exception>
    /// <exception cref="ArgumentNullException">
    /// If used as a static function with a null collection
    /// </exception>
    /// <remarks>
    /// <code>
    /// const int lastItem = 42;
    /// var list = new List&lt;int> { 1, 2, 3, lastItem };
    /// int beforeLen = list.Count;
    /// var x = list.RemoveLastItem();
    /// Assert.AreEqual( lastItem, x );
    /// Assert.AreEqual( beforeLen - 1, list.Count );
    /// </code>
    /// </remarks>
    public static T RemoveLastItem<T>( this IList<T> _list )
    {
        if (_list is null)
            throw new ArgumentNullException( nameof( _list ) );

        var idx = _list.Count - 1;
        var retval = _list[idx];
        _list.RemoveAt( idx );
        return retval;
    }

    /// <summary>
    /// Simply remove the first item from a List. Throws an exception if the list is empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list</typeparam>
    /// <param name="_list">The list to remove the first element of</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the List is empty</exception>
    /// <exception cref="ArgumentNullException">
    /// If used as a static function with a null collection
    /// </exception>
    /// <remarks>
    /// <code>
    /// const int firstItem = 42;
    /// var list = new List&lt;int> { firstItem, 1, 2, 3 };
    /// int beforeLen = list.Count;
    /// var x = list.RemoveFirstItem();
    /// Assert.AreEqual( firstItem, x );
    /// Assert.AreEqual( beforeLen - 1, list.Count );
    /// </code>
    /// </remarks>
    public static T RemoveFirstItem<T>( this IList<T> _list )
    {
        if (_list is null)
            throw new ArgumentNullException( nameof( _list ) );

        var retval = _list[0];
        _list.RemoveAt( 0 );
        return retval;
    }


    /// <summary>
    /// Remove all elements of an enumeration from a list. Doesn't care if the elements are in the list or not.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_list"></param>
    /// <param name="toRemove"></param>
    public static void RemoveRange<T>( this IList<T> _list, IEnumerable<T> toRemove )
    {
        for (int i = 0; i < _list.Count;)
        {
            if (toRemove.Contains( _list[i] ))
                _list.RemoveAt( i );
            else
                i++;
        }
    }

    /// <summary>
    /// List helper to Set the value at any index in the list to something. Will "grow" the list
    /// if its currently too small to handle the index specified.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the list</typeparam>
    /// <param name="_list">The list to set a value in</param>
    /// <param name="_index">
    /// The index of the value- may be larger than list.Count. May not be negative.
    /// </param>
    /// <param name="_value">The value to put into the list</param>
    /// <exception cref="ArgumentNullException">
    /// If used as a static function with a null collection
    /// </exception>
    /// <exception cref="IndexOutOfRangeException">Thrown if the index is negative.</exception>
    /// <returns>The item that was passed in</returns>
    /// <remarks>
    /// <code>
    /// var list = new List&lt;int> { 1, 2, 3 };
    /// Assert.AreEqual( 3, list.Count );
    /// 
    /// list.Set( 10, 42 );
    /// 
    /// Assert.AreEqual( 11, list.Count );
    /// Assert.AreEqual( 0, list[9] );
    /// Assert.AreEqual( 42, list[10] );
    /// </code>
    /// </remarks>
    public static T Set<T>( this IList<T> _list, int _index, T _value )
    {
        if (_list is null)
            throw new ArgumentNullException( nameof( _list ) );
        if (_index < 0)
            throw new IndexOutOfRangeException( $"May not pass in negative ({_index}) index" );

        if (_index < _list.Count)
        {
            _list[_index] = _value;
        }
        else
        {
            for (var i = _list.Count; i < _index; i++)
                _list.Add( default );
            _list.Add( _value );
        }

        return _value;
    }


    /// <summary>
    /// Swap two elements in any IList collection
    /// </summary>
    /// <typeparam name="T">The type of the elements in the list</typeparam>
    /// <param name="_list">The list that we're swapping elements in</param>
    /// <param name="_index1">The index of the first element to swap</param>
    /// <param name="_index2">The index of the second element to swap</param>
    /// <exception cref="ArgumentNullException">
    /// If used as a static function with a null collection
    /// </exception>
    /// <exception cref="IndexOutOfRangeException">
    /// If either index is negative or past the end of the list
    /// </exception>
    public static void SwapElements<T>( this IList<T> _list, int _index1, int _index2 )
    {
        if (_list is null)
            throw new ArgumentNullException( nameof( _list ) );

        var tmp = _list[_index1];
        _list[_index1] = _list[_index2];
        _list[_index2] = tmp;
    }


    /// <summary>
    /// Check to see if a Dictionary contains an item for a given key. If it does, simply return
    /// that item. If it doesn't, create a new item and add it to the dictionary, returning the
    /// new item.
    /// </summary>
    /// <typeparam name="Tkey">The Type of the Keys in the Dictionary</typeparam>
    /// <typeparam name="Tval">The Type of the Values in the Dictionary</typeparam>
    /// <param name="_dictionary">The Dictionary</param>
    /// <param name="_key">The Key to look for</param>
    /// <param name="_generator">
    /// If this is not NULL, this method will use this function to generate a new item when one
    /// does not already exist.
    /// </param>
    /// <returns>
    /// A new Item for the Key- Either the one present in the Dictionary prior to calling this
    /// method, or a new Value if one didn't exist prior to calling this method.
    /// </returns>
    /// <remarks>
    /// <code>
    /// var dict = new Dictionary&lt;int, string>
    /// {
    ///     { 50, "mike" },
    ///     { 42, "everything" },
    /// };
    /// 
    /// var fifty = dict.GetOrAdd( 50, _x => "fif50" );
    /// Assert.AreEqual( "mike", fifty );
    /// Assert.AreEqual( "mike", dict[50] );
    /// 
    /// var nine = dict.GetOrAdd( 9, _x => "nine" );
    /// Assert.AreEqual( "nine", nine );
    /// Assert.AreEqual( "nine", dict[9] );
    /// </code>
    /// </remarks>
    public static Tval GetOrAdd<Tkey, Tval>( this IDictionary<Tkey, Tval> _dictionary, Tkey _key, Func<Tkey, Tval> _generator = null )
        where Tval : class
    {
        if (!_dictionary.TryGetValue( _key, out var retval )) // not there
        {
            if (_generator == null) // use default constructor
                retval = Activator.CreateInstance<Tval>();
            else
                retval = _generator( _key );

            _dictionary[_key] = retval;
        }
        return retval;
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="lookup"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool TryGetValue<TKey, TValue>( this ILookup<TKey, TValue> lookup, TKey key, out IEnumerable<TValue> value )
    {
        bool contains = lookup.Contains( key );
        value = contains ? lookup[key] : null;
        return contains;
    }

    public static IEnumerable<T> Backwards<T>( this IList<T> list )
    {
        for (int i = list.Count - 1; i >= 0; i--)
            yield return list[i];
    }
}
