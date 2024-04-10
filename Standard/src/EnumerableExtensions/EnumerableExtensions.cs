using System.Collections;

namespace Morpheus;


/// <summary>
/// It is the application writer's responsibility to decide whether to use one
/// of these extension methods or to write the loop herself. There is a chance
/// that using these extensions on enumerations can impact performance over
/// writing these loops yourself.
/// 
/// In summary- If you're really worried about performance on LARGE, tight loops
/// that are not IO-or-UI-bound in any way, these extensions MAY not be right
/// for you. However, if I/O and/or User Input is involved, any performance
/// penalty for using these extensions will almost always be negligible in
/// comparison.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Like <see cref="string.Join(string, object[])"/> , but uses an
    /// enumeration instead of an array
    /// </summary>
    /// <param name="_collection">
    /// The collection of objects that you wish to join
    /// </param>
    /// <param name="_joinString">
    /// The character(s) placed between elements from _collection
    /// </param>
    /// <param name="_stringizer">
    /// A function that will turn each individual object into a string. Leave
    /// NULL to use the <see cref="object.ToString"/> method.
    /// </param>
    /// <returns>
    /// A string consisting of each of the elements in the collection joined
    /// together using a joining string.
    /// </returns>
    /// <remarks>
    /// <para> This class is used for these examples: </para>
    /// <code>
    /// class ToJoin
    /// {
    ///     public int Integer { get; set; }
    ///     public override string ToString() => $"[{Integer}]";
    /// }
    /// 
    /// var list = new ToJoin[]
    /// {
    ///     new ToJoin{ Integer = 42 },
    ///     new ToJoin{ Integer = 23 },
    /// };
    /// </code>
    /// <para> This is a straight-forward example using default ToString and
    /// Space as a separator </para>
    /// <code>
    /// var joined = list.JoinAsString( " " );
    /// Assert.AreEqual( "[42] [23]", joined );
    /// </code>
    /// <para> In this example, a custom string-izer is used with a comma
    /// separator </para>
    /// <code>
    /// var joined = list.JoinAsString( ",", _obj => $"({_obj.Integer * 2})" );
    /// Assert.AreEqual( "(84),(46)", joined );
    /// </code>
    /// </remarks>
    public static string JoinAsString<T>( this IEnumerable<T> _collection, string _joinString = "", Func<T, string>? _stringizer = null )
    {
        var str = new StringBuilder();
        foreach (var o in _collection)
        {
            if (str.Length > 0)
                str.Append( _joinString );
            str.Append( _stringizer?.Invoke( o ) ?? o?.ToString() ?? "" );
        }
        return str.ToString();
    }

    /// <summary>
    /// Return the index of the first element in the collection for which the
    /// _selector returns TRUE.
    /// </summary>
    /// <typeparam name="T">
    /// The Type of each element in the population
    /// </typeparam>
    /// <param name="_collection">The population of data</param>
    /// <param name="predicate">
    /// Application returns TRUE for an element that should be returned.
    /// </param>
    /// <returns>
    /// The index (ordinal position) of the first element in the collection for
    /// which the selector returned TRUE, or -1 if no element caused the
    /// selector to return TRUE
    /// </returns>
    /// <remarks>
    /// <para> This data is used for these two examples: </para>
    /// <code>
    /// class HasAnInt
    /// {
    ///     public int Integer { get; set; }
    ///     public override string ToString() => $"[{Integer}]";
    /// }
    /// 
    /// var list = new HasAnInt[]
    /// {
    ///     new HasAnInt{ Integer = 12 },
    ///     new HasAnInt{ Integer = 23 },
    ///     new HasAnInt{ Integer = 34 },
    ///     new HasAnInt{ Integer = 23 },
    ///     new HasAnInt{ Integer = 12 },
    /// };
    /// </code>
    /// <para> This is a standard usage: </para>
    /// <code>
    /// int x = list.FirstIndexOf( _x => _x.Integer == 23 );
    /// Assert.AreEqual( 1, x );
    /// </code>
    /// <para> In this example, there are no objects which match... </para>
    /// <code>
    /// int x = list.FirstIndexOf( _x => _x.Integer == 234 );
    /// Assert.AreEqual( -1, x );
    /// </code>
    /// </remarks>
    public static int FirstIndexOf<T>( this IEnumerable<T> _collection, Func<T, bool> predicate )
    {
        var idx = 0;
        foreach (var elem in _collection)
        {
            if (predicate( elem ))
                return idx;
            idx++;
        }
        return -1;
    }

    public static int FirstIndexOf<T>( this IEnumerable<T> _collection, T compareTo )
    {
        var idx = 0;
        foreach (var elem in _collection)
        {
            if (elem is not null && elem.Equals( compareTo ))
                return idx;
            idx++;
        }
        return -1;
    }


    /// <summary>
    /// Return all elements after a specified element in an enumeration
    /// </summary>
    /// <typeparam name="T">
    /// The type of the elements in the enumeration
    /// </typeparam>
    /// <param name="_collection">The elements to enumerate over</param>
    /// <param name="_item">
    /// The item to find, then return all items after
    /// </param>
    /// <returns>All items after the specified item in the enumeration</returns>
    /// <remarks>
    /// If the item is not found, then this is simply an empty enumeration.
    /// </remarks>
    public static IEnumerable<T> AllAfter<T>( this IEnumerable<T> _collection, T _item )
    {
        var found = false;
        foreach (var item in _collection)
        {
            if (found)
                yield return item;
            else if (item is not null && item.Equals( _item ))
                found = true;
        }
    }


    /// <summary>
    /// Determine if a condition is met for ANY item in the enumeration. This is
    /// literally an alias for the
    /// <see cref="Enumerable.Any{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>
    /// method.
    /// </summary>
    /// <typeparam name="T">The Type of items in the enumeration</typeparam>
    /// <param name="_items">The items to test for the condition</param>
    /// <param name="_condition">The boolean condition to be tested</param>
    /// <returns>
    /// TRUE if the condition is true for any item in the enumeration
    /// </returns>
    public static bool Contains<T>( this IEnumerable<T> _items, Func<T, bool> _condition )
        => _items.Any( _condition );


    /// <summary>
    /// Simply returns TRUE if the collection is empty, FALSE if it has anything
    /// in it.
    /// </summary>
    /// <typeparam name="T">
    /// Some arbitrary type for the elements of the collection
    /// </typeparam>
    /// <param name="_items">The collection of elements</param>
    /// <returns>
    /// TRUE if the collection is empty, FALSE if it has anything in it
    /// </returns>
    public static bool IsEmpty<T>( this IEnumerable<T> _items )
    {
        using (var enumerator = _items.GetEnumerator())
            return !enumerator.MoveNext();
    }


    /// <summary>
    /// Simply returns TRUE if the collection is empty, FALSE if it has anything
    /// in it.
    /// </summary>
    /// <typeparam name="T">
    /// Some arbitrary type for the elements of the collection
    /// </typeparam>
    /// <param name="_items">The collection of elements</param>
    /// <returns>
    /// TRUE if the collection is empty, FALSE if it has anything in it
    /// </returns>
    public static bool IsNotEmpty<T>( this IEnumerable<T> _items )
    {
        using (var enumerator = _items.GetEnumerator())
            return enumerator.MoveNext();
    }


    /// <summary>
    /// If the enumeration has exactly one element in it, then return that one
    /// element. Otherwise return NULL. This is similar to SingleOrDefault
    /// except that this will NOT throw an exception if there are more than one
    /// element in the enumeration.
    /// </summary>
    /// <typeparam name="T">
    /// The Type of the elements of the enumeration
    /// </typeparam>
    /// <param name="_items">The enumeration</param>
    /// <returns>
    /// NULL unless there is exactly one element in the enumeration, in which
    /// case it will return that singular element
    /// </returns>
    public static T? OneOrDefault<T>( this IEnumerable<T> _items )
    {
        if (_items == null)
            return default;

        if (_items is IList<T> list)
            return list.Count == 1 ? list[0] : default;

        using (var enumerator = _items.GetEnumerator())
        {
            if (!enumerator.MoveNext())
                return default;

            var current = enumerator.Current;
            if (!enumerator.MoveNext())
                return current;

            return default;
        }
    }



    /// <summary>
    /// Return the smallest element of the enumeration based on the selector
    /// function given
    /// </summary>
    /// <typeparam name="T">The Type of items in the enumeration</typeparam>
    /// <param name="_items">The items in the list to search</param>
    /// <param name="_selector">
    /// The function that determines which "field" of an element is checked for
    /// smallness
    /// </param>
    /// <returns>The smallest element in the enumeration</returns>
    public static T? Smallest<T>( this IEnumerable<T> _items, Func<T, double> _selector )
    {
        var best = double.MaxValue;
        var bestItem = default( T );

        foreach (var item in _items)
        {
            var x = _selector( item );
            if (x < best)
            {
                best = x;
                bestItem = item;
            }
        }

        return bestItem;
    }

    /// <summary>
    /// Return the smallest element of the enumeration based on the selector
    /// function given
    /// </summary>
    /// <typeparam name="T">The Type of items in the enumeration</typeparam>
    /// <param name="_items">The items in the list to search</param>
    /// <param name="_selector">
    /// The function that determines which "field" of an element is checked for
    /// smallness
    /// </param>
    /// <returns>The smallest element in the enumeration</returns>
    public static T? Smallest<T>( this IEnumerable<T> _items, Func<T, IComparable> _selector )
    {
        var bestItem = default( T );
        IComparable? bestItemValue = null;
        var notSet = true;

        foreach (var item in _items)
        {
            if (notSet)
            {
                bestItem = item;
                bestItemValue = _selector( bestItem );
                notSet = false;
            }
            else
            {
                var x = _selector( item );
                if (x.CompareTo( bestItemValue ) < 0)
                {
                    bestItem = item;
                    bestItemValue = _selector( bestItem );
                }
            }
        }

        return bestItem;
    }

    /// <summary>
    /// Return the largest element of the enumeration based on the selector
    /// function given
    /// </summary>
    /// <typeparam name="T">The Type of items in the enumeration</typeparam>
    /// <param name="_items">The items in the list to search</param>
    /// <param name="_selector">
    /// The function that determines which "field" of an element is checked for
    /// largeness
    /// </param>
    /// <returns>The largest element in the enumeration</returns>
    public static T? Largest<T>( this IEnumerable<T> _items, Func<T, double> _selector )
    {
        var best = double.MinValue;
        var bestItem = default( T );

        foreach (var item in _items)
        {
            var x = _selector( item );
            if (x > best)
            {
                best = x;
                bestItem = item;
            }
        }

        return bestItem;
    }


    /// <summary>
    /// Return the largest element of the enumeration based on the selector
    /// function given
    /// </summary>
    /// <typeparam name="T">The Type of items in the enumeration</typeparam>
    /// <param name="_items">The items in the list to search</param>
    /// <param name="_selector">
    /// The function that determines which "field" of an element is checked for
    /// largeness
    /// </param>
    /// <returns>The largest element in the enumeration</returns>
    public static T? Largest<T>( this IEnumerable<T> _items, Func<T, IComparable> _selector )
    {
        var bestItem = default( T );
        IComparable? bestItemValue = null;
        var notSet = true;

        foreach (var item in _items)
        {
            if (notSet)
            {
                bestItem = item;
                bestItemValue = _selector( bestItem );
                notSet = false;
            }
            else
            {
                var x = _selector( item );
                if (x.CompareTo( bestItemValue ) > 0)
                {
                    bestItem = item;
                    bestItemValue = _selector( bestItem );
                }
            }
        }

        return bestItem;
    }



    /// <summary>
    /// Like "Select", but if the selector throws an exception, that particular
    /// input element is simply ignored (nothing returned in the output
    /// enumeration). This is used when you expect that some of the
    /// transformations performed on the input elements may throw exceptions and
    /// checking for those potential exceptions is difficult or unwieldy.
    /// </summary>
    /// <typeparam name="T">
    /// The Type of the objects in the input collection.
    /// </typeparam>
    /// <typeparam name="U">
    /// The Type of the transformed objects/items
    /// </typeparam>
    /// <param name="_collection">The collection to enumerate</param>
    /// <param name="_selector">
    /// The transformation of input elements to output elements
    /// </param>
    /// <returns>
    /// An enumeration of the transformed values when that transformation didn't
    /// cause an exception to be thrown
    /// </returns>
    public static IEnumerable<U?> SelectIgnoreExceptions<T, U>( this IEnumerable<T> _collection, Func<T, U> _selector )
    {
        foreach (var obj in _collection)
        {
            var val = default( U );
            var doYield = true;
            try
            {
                val = _selector( obj );
            }
            catch
            {
                doYield = false;
            }
            if (doYield)
                yield return val;
        }
    }


    /// <summary>
    /// Iterate through two collections in parallel, returning the element from
    /// each collection at the same ordinal position within the collection. This
    /// is called Collating when applied to the print (paper) industry
    /// </summary>
    /// <remarks>
    /// For sets A and B, this operation produces the following:
    /// <code>
    /// A = { 7, 12, 39 }
    /// B = { "cat", "dog", "pig" }
    /// * = { (7,"cat"), (12,"dog"), (39,"pig") }
    /// </code>
    /// </remarks>
    /// <typeparam name="T1">
    /// The <see cref="Type"/> of the elements of the first collection
    /// </typeparam>
    /// <typeparam name="T2">
    /// The <see cref="Type"/> of the elemnets in the second collection
    /// </typeparam>
    /// <param name="_collection">
    /// The first collection, the elements of which shall appear in the first
    /// parameter to the <see cref="Action"/>
    /// </param>
    /// <param name="_otherCollection">
    /// The second collection, the elements of which shall appear in the second
    /// parameter to the <see cref="Action"/>
    /// </param>
    /// <param name="_operation">
    /// The operation that should be performed on each pair of elements from the
    /// collections
    /// </param>
    public static void Zip<T1, T2>( this IEnumerable<T1> _collection, IEnumerable<T2> _otherCollection, Action<T1, T2> _operation )
    {
        var otherIter = _otherCollection.GetEnumerator();
        foreach (var item in _collection)
        {
            if (!otherIter.MoveNext())
                break;

            _operation( item, otherIter.Current );
        }
    }

    /// <summary>
    /// Iterate through two collections in parallel, returning the element from
    /// each collection at the same ordinal position within the collection. This
    /// is called Collating when applied to the print (paper) industry
    /// </summary>
    /// <typeparam name="T1">
    /// The <see cref="Type"/> of the elements of the first collection
    /// </typeparam>
    /// <typeparam name="T2">
    /// The <see cref="Type"/> of the elements in the second collection
    /// </typeparam>
    /// <param name="_collection">
    /// The first collection, the elements of which shall appear in the first
    /// item in the returned <see cref="Tuple"/> s
    /// </param>
    /// <param name="_otherCollection">
    /// The second collection, the elements of which shall appear in the second
    /// item in the returned <see cref="Tuple"/> s
    /// </param>
    /// <returns>
    /// Tuples containing the collated items from each collection
    /// </returns>
    public static IEnumerable<Tuple<T1, T2>> Interleave<T1, T2>( this IEnumerable<T1> _collection, IEnumerable<T2> _otherCollection )
    {
        var otherIter = _otherCollection.GetEnumerator();
        foreach (var item in _collection)
        {
            if (!otherIter.MoveNext())
                break;

            yield return new Tuple<T1, T2>( item, otherIter.Current );
        }
    }


    /// <summary>
    /// Take items from an IList based on an enumeration of indicies. This is
    /// useful when you want to grab a set of indexed items from a list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="indicies"></param>
    /// <returns></returns>
    public static IEnumerable<T> Gather<T>( this IList<T> list, IEnumerable<int> indicies )
    {
        foreach (var ix in indicies)
            yield return list[ix];
    }



    public static int Count( this IEnumerable stuff )
    {
        int count = 0;
        foreach (var item in stuff) count++;
        return count;
    }


    /// <summary>
    /// For each string in the input, apply a Regex that MUST HAVE A GROUP
    /// specified. The value of the group is returned for each match- an empty
    /// string is returned when a match didn't occur for an element.
    /// </summary>
    /// <param name="list"></param>
    /// <param name="regex"></param>
    /// <returns></returns>
    public static IEnumerable<string> SelectWithRegex( this IEnumerable<string> list, string regex )
    {
        var r = new Regex( regex );
        foreach (string s in list)
            yield return r.Match( s ).Groups[1].Value;
    }

    /// <summary>
    /// For each string in the input, apply a Regex to determine if the string
    /// matches the pattern. If it does, return the string. If not, skip it.
    /// </summary>
    public static IEnumerable<string> WhereWithRegex( this IEnumerable<string> list, string regex, RegexOptions regexOptions = RegexOptions.None )
    {
        var r = new Regex( regex, regexOptions );
        return list.Where( s => r.IsMatch( s ) );
    }

    /// <summary>
    /// AddRange for any ICollection- seems like this should be part of LINQ
    /// </summary>
    public static void AddRange<T>( this ICollection<T> collection, IEnumerable<T> items )
    {
        foreach (var item in items)
            collection.Add( item );
    }

    /// <summary>
    /// Remove every element of the IList that matches the given element.
    /// </summary>
    public static void RemoveAll<T>( this IList<T> list, T toRemove )
    {
        if (toRemove is null) throw new ArgumentNullException( "toRemove" );

        int destIdx = 0;
        for (int i = 0; i < list.Count; i++)
        {
            if (!toRemove.Equals( list[i] ))
                list[destIdx++] = list[i];
        }
        while (list.Count > destIdx)
            list.RemoveAt( list.Count - 1 );
    }

    /// <summary>
    /// Remove every element of the IList that matches the given element.
    /// </summary>
    /// <returns>The count of elements removed.</returns>
    public static void RemoveAll<T>( this IList<T> list, Func<T, bool> predicate )
    {
        int destIdx = 0;
        for (int i = 0; i < list.Count; i++)
        {
            if (!predicate( list[i] ))
                list[destIdx++] = list[i];
        }
        while (list.Count > destIdx)
            list.RemoveAt( list.Count - 1 );
    }

    /// <summary>
    /// Return elements of the collection that are next to each other. The first
    /// tuple returned has .Item1 == default(T) and .Item2 == the first element
    /// of the collection. The next tuple returned has .Item1 == the first
    /// element of the collection and .Item2 == the second element of the
    /// collection.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns>A sequence of pairs of elements from the enumeration</returns>
    public static IEnumerable<(T?, T)> Pairwise<T>( this IEnumerable<T> source )
    {
        using (var iterator = source.GetEnumerator())
        {
            if (!iterator.MoveNext())
                yield break;

            var previous = iterator.Current;
            yield return (default( T ), previous);

            while (iterator.MoveNext())
            {
                yield return (previous, iterator.Current);
                previous = iterator.Current;
            }
        }
    }

    /// <summary>
    /// Similar to Python "enumerate" function. Equivalent to source.Select( (
    /// x, i ) => (x, i) ) Returns a tuple of the element and its index.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IEnumerable<(T, int)> WithIndex<T>( this IEnumerable<T> source )
    {
        int idx = 0;
        foreach (var item in source)
            yield return (item, idx++);
    }


    /// <summary>
    /// Return the index of the smallest element in the collection using
    /// IComparable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns>the index of the smallest element</returns>
    public static int ArgMin<T>( this IEnumerable<T> source )
        where T : IComparable<T> => source.ArgBest( x => x, true );

    /// <summary>
    /// Return the index of the largest element in the collection using
    /// IComparable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns>the index of the largest element</returns>
    public static int ArgMax<T>( this IEnumerable<T> source )
        where T : IComparable<T> => source.ArgBest( x => x, false );

    /// <summary>
    /// Return the index of the smallest element in the collection according to
    /// a selector function
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns>the index of the smallest element</returns>
    public static int ArgMin<T, Tselected>( this IEnumerable<T> source, Func<T, Tselected> selector )
        where Tselected : IComparable<Tselected> => source.ArgBest( selector, true );

    /// <summary>
    /// Return the index of the largest element in the collection according to a
    /// selector function
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns>the index of the largest element</returns>
    public static int ArgMax<T, Tselected>( this IEnumerable<T> source, Func<T, Tselected> selector )
        where Tselected : IComparable<Tselected> => source.ArgBest( selector, false );


    private static int ArgBest<T, Tselected>(
        this IEnumerable<T> source,
        Func<T, Tselected> selector,
        bool argMin )
        where Tselected : IComparable<Tselected>
    {
        Tselected? bestVal = default;
        int bestIdx = -1;
        int factor = argMin ? 1 : -1;

        foreach (var (x, i) in source.WithIndex())
        {
            var selected = selector( x );
            if (bestIdx == -1 || selected.CompareTo( bestVal ) * factor < 0)
            {
                bestVal = selected;
                bestIdx = i;
            }
        }

        return bestIdx;
    }

    /// <summary>
    /// Like <see cref="Enumerable.Repeat{TResult}(TResult, int)"/> , but allows
    /// each value to be generated by a creator function"/>
    /// </summary>
    /// <returns>
    /// An enumeration created by repeatedly calling a generator function
    /// </returns>
    public static IEnumerable<T> Repeat<T>( long count, Func<long, T> creator )
    {
        for (int i = 0; i < count; i++)
            yield return creator( i );
    }

    /// <summary>
    /// Like <see cref="Enumerable.Repeat{TResult}(TResult, int)"/> , but allows
    /// each value to be generated by a creator function"/>
    /// </summary>
    /// <returns>
    /// An enumeration created by repeatedly calling a generator function
    /// </returns>
    public static IEnumerable<T> Repeat<T>( long count, Func<T> creator )
    {
        for (int i = 0; i < count; i++)
            yield return creator();
    }

    public static IEnumerable<T> Union<T>( T head, IEnumerable<T> tail )
    {
        yield return head;
        foreach (var item in tail)
            yield return item;
    }

    public static IEnumerable<T> Union<T>( this IEnumerable<T> head, T tail )
    {
        foreach (var item in head)
            yield return item;
        yield return tail;
    }
}

