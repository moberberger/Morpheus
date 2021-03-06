﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Morpheus
{
    /// <summary>
    /// It is the application writer's responsibility to decide whether to use one of these
    /// extension methods or to write the loop herself. There is a chance that using these
    /// extensions on enumerations can impact performance over writing these loops yourself.
    /// 
    /// In summary- If you're really worried about performance on LARGE, tight loops that are
    /// not IO-or-UI-bound in any way, these extensions MAY not be right for you. However, if
    /// I/O and/or User Input is involved, any performance penalty for using these extensions
    /// will almost always be negligible in comparison.
    /// </summary>
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Like <see cref="string.Join(string, object[])"/> , but uses an enumeration instead
        /// of an array
        /// </summary>
        /// <param name="_collection">The collection of objects that you wish to join</param>
        /// <param name="_joinString">
        /// The character(s) placed between elements from _collection
        /// </param>
        /// <param name="_stringizer">
        /// A function that will turn each individual object into a string. Leave NULL to use
        /// the <see cref="object.ToString"/> method.
        /// </param>
        /// <returns>
        /// A string consisting of each of the elements in the collection joined together using
        /// a joining string.
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
        /// <para> This is a straight-forward example using default ToString and Space as a
        /// separator </para>
        /// <code>
        /// var joined = list.JoinAsString( " " );
        /// Assert.AreEqual( "[42] [23]", joined );
        /// </code>
        /// <para> In this example, a custom string-izer is used with a comma separator </para>
        /// <code>
        /// var joined = list.JoinAsString( ",", _obj => $"({_obj.Integer * 2})" );
        /// Assert.AreEqual( "(84),(46)", joined );
        /// </code>
        /// </remarks>
        public static string JoinAsString<T>( this IEnumerable<T> _collection, string _joinString, Func<T, string> _stringizer = null )
        {
            if (_collection is null)
                throw new ArgumentNullException( nameof( _collection ) );
            if (_joinString is null)
                throw new ArgumentNullException( nameof( _joinString ) );

            var str = new StringBuilder();
            foreach (var o in _collection)
            {
                if (str.Length > 0)
                    str.Append( _joinString );
                str.Append( _stringizer?.Invoke( o ) ?? o.ToString() );
            }
            return str.ToString();
        }

        /// <summary>
        /// Return the index of the first element in the collection for which the _selector
        /// returns TRUE.
        /// </summary>
        /// <typeparam name="T">The Type of each element in the population</typeparam>
        /// <param name="_collection">The population of data</param>
        /// <param name="_selector">
        /// Application returns TRUE for an element that should be returned.
        /// </param>
        /// <returns>
        /// The index (ordinal position) of the first element in the collection for which the
        /// selector returned TRUE, or -1 if no element caused the selector to return TRUE
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
        public static int FirstIndexOf<T>( this IEnumerable<T> _collection, Func<T, bool> _selector )
        {
            var idx = 0;
            foreach (var elem in _collection)
            {
                if (_selector( elem ))
                    return idx;
                idx++;
            }
            return -1;
        }


        /// <summary>
        /// Return all elements after a specified element in an enumeration
        /// </summary>
        /// <typeparam name="T">The type of the elements in the enumeration</typeparam>
        /// <param name="_collection">The elements to enumerate over</param>
        /// <param name="_item">The item to find, then return all items after</param>
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
                else if (item.Equals( _item ))
                    found = true;
            }
        }


        /// <summary>
        /// Determine if a condition is met for ANY item in the enumeration. This is literally
        /// an alias for the
        /// <see cref="Enumerable.Any{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>
        /// method.
        /// </summary>
        /// <typeparam name="T">The Type of items in the enumeration</typeparam>
        /// <param name="_items">The items to test for the condition</param>
        /// <param name="_condition">The boolean condition to be tested</param>
        /// <returns>TRUE if the condition is true for any item in the enumeration</returns>
        public static bool Contains<T>( this IEnumerable<T> _items, Func<T, bool> _condition ) => _items.Any( _condition );


        /// <summary>
        /// Simply returns TRUE if the collection is empty, FALSE if it has anything in it.
        /// </summary>
        /// <typeparam name="T">
        /// Some arbitrary type for the elements of the collection
        /// </typeparam>
        /// <param name="_items">The collection of elements</param>
        /// <returns>TRUE if the collection is empty, FALSE if it has anything in it</returns>
        public static bool IsEmpty<T>( this IEnumerable<T> _items )
        {
            using (var enumerator = _items.GetEnumerator())
                return !enumerator.MoveNext();
        }


        /// <summary>
        /// If the enumeration has exactly one element in it, then return that one element.
        /// Otherwise return NULL. This is similar to SingleOrDefault except that this will NOT
        /// throw an exception if there are more than one element in the enumeration.
        /// </summary>
        /// <typeparam name="T">The Type of the elements of the enumeration</typeparam>
        /// <param name="_items">The enumeration</param>
        /// <returns>
        /// NULL unless there is exactly one element in the enumeration, in which case it will
        /// return that singular element
        /// </returns>
        public static T OneOrDefault<T>( this IEnumerable<T> _items )
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
        /// Return the smallest element of the enumeration based on the selector function given
        /// </summary>
        /// <typeparam name="T">The Type of items in the enumeration</typeparam>
        /// <param name="_items">The items in the list to search</param>
        /// <param name="_selector">
        /// The function that determines which "field" of an element is checked for smallness
        /// </param>
        /// <returns>The smallest element in the enumeration</returns>
        public static T Smallest<T>( this IEnumerable<T> _items, Func<T, double> _selector )
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
        /// Return the smallest element of the enumeration based on the selector function given
        /// </summary>
        /// <typeparam name="T">The Type of items in the enumeration</typeparam>
        /// <param name="_items">The items in the list to search</param>
        /// <param name="_selector">
        /// The function that determines which "field" of an element is checked for smallness
        /// </param>
        /// <returns>The smallest element in the enumeration</returns>
        public static T Smallest<T>( this IEnumerable<T> _items, Func<T, IComparable> _selector )
        {
            var bestItem = default( T );
            IComparable bestItemValue = null;
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
        /// Return the largest element of the enumeration based on the selector function given
        /// </summary>
        /// <typeparam name="T">The Type of items in the enumeration</typeparam>
        /// <param name="_items">The items in the list to search</param>
        /// <param name="_selector">
        /// The function that determines which "field" of an element is checked for largeness
        /// </param>
        /// <returns>The largest element in the enumeration</returns>
        public static T Largest<T>( this IEnumerable<T> _items, Func<T, double> _selector )
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
        /// Return the largest element of the enumeration based on the selector function given
        /// </summary>
        /// <typeparam name="T">The Type of items in the enumeration</typeparam>
        /// <param name="_items">The items in the list to search</param>
        /// <param name="_selector">
        /// The function that determines which "field" of an element is checked for largeness
        /// </param>
        /// <returns>The largest element in the enumeration</returns>
        public static T Largest<T>( this IEnumerable<T> _items, Func<T, IComparable> _selector )
        {
            var bestItem = default( T );
            IComparable bestItemValue = null;
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
        /// Like "Select", but if the selector throws an exception, that particular input
        /// element is simply ignored (nothing returned in the output enumeration). This is used
        /// when you expect that some of the transformations performed on the input elements may
        /// throw exceptions and checking for those potential exceptions is difficult or
        /// unwieldy.
        /// </summary>
        /// <typeparam name="T">The Type of the objects in the input collection.</typeparam>
        /// <typeparam name="U">The Type of the transformed objects/items</typeparam>
        /// <param name="_collection">The collection to enumerate</param>
        /// <param name="_selector">
        /// The transformation of input elements to output elements
        /// </param>
        /// <returns>
        /// An enumeration of the transformed values when that transformation didn't cause an
        /// exception to be thrown
        /// </returns>
        public static IEnumerable<U> SelectIgnoreExceptions<T, U>( this IEnumerable<T> _collection, Func<T, U> _selector )
        {
            foreach (var obj in _collection)
            {
                var val = default( U );
                var skip = false;
                try
                {
                    val = _selector( obj );
                }
                catch
                {
                    skip = true;
                }
                if (!skip)
                    yield return val;
            }
        }


        /// <summary>
        /// Iterate through two collections in parallel, returning the element from each
        /// collection at the same ordinal position within the collection. This is called
        /// Collating when applied to the print (paper) industry
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
        /// The first collection, the elements of which shall appear in the first parameter to
        /// the <see cref="Action"/>
        /// </param>
        /// <param name="_otherCollection">
        /// The second collection, the elements of which shall appear in the second parameter to
        /// the <see cref="Action"/>
        /// </param>
        /// <param name="_operation">
        /// The operation that should be performed on each pair of elements from the collections
        /// </param>
        public static void Collate<T1, T2>( this IEnumerable<T1> _collection, IEnumerable<T2> _otherCollection, Action<T1, T2> _operation )
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
        /// Iterate through two collections in parallel, returning the element from each
        /// collection at the same ordinal position within the collection. This is called
        /// Collating when applied to the print (paper) industry
        /// </summary>
        /// <typeparam name="T1">
        /// The <see cref="Type"/> of the elements of the first collection
        /// </typeparam>
        /// <typeparam name="T2">
        /// The <see cref="Type"/> of the elements in the second collection
        /// </typeparam>
        /// <param name="_collection">
        /// The first collection, the elements of which shall appear in the first item in the
        /// returned <see cref="Tuple"/> s
        /// </param>
        /// <param name="_otherCollection">
        /// The second collection, the elements of which shall appear in the second item in the
        /// returned <see cref="Tuple"/> s
        /// </param>
        /// <returns>Tuples containing the collated items from each collection</returns>
        public static IEnumerable<Tuple<T1, T2>> Collate<T1, T2>( this IEnumerable<T1> _collection, IEnumerable<T2> _otherCollection )
        {
            var otherIter = _otherCollection.GetEnumerator();
            foreach (var item in _collection)
            {
                if (!otherIter.MoveNext())
                    break;

                yield return new Tuple<T1, T2>( item, otherIter.Current );
            }
        }



        #region Really Specific Enumerations
#if _KERNEL32_OK_
        /// <summary>
        /// Turn an enumeration into a <see cref="CSortableBindingList&lt;T> "/>
        /// </summary>
        /// <typeparam name="T">The Type of items in the enumeration</typeparam>
        /// <param name="_items">
        /// The items to put into a <see cref="CSortableBindingList&lt;T> "/>
        /// </param>
        /// <returns>
        /// A new <see cref="CSortableBindingList&lt;T> "/> containing the elements of this
        /// enumeration
        /// </returns>
        public static CSortableBindingList<T> ToSortableBindingList<T>( this IEnumerable<T> _items )
        {
            return new CSortableBindingList<T>( _items );
        }
#endif

        #endregion
    }
}
