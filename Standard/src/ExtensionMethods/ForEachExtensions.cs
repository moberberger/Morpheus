using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Morpheus
{
    /// <summary>
    /// Collection of extension methods for the <see cref="IEnumerable{T}"/> classes
    /// </summary>
    public static class ForEachExtensions
    {
        /// <summary>
        /// An Extension to <see cref="IEnumerable{T}"/> to allow "some arbitrary operation" to
        /// be performed on each element of an enumeration.
        /// </summary>
        /// <typeparam name="T">
        /// The implied <see cref="Type"/> of the elements in the enumeration
        /// </typeparam>
        /// <param name="_set">The enumeration of items</param>
        /// <param name="_operation">
        /// The operation to be performed on each item of the enumeration
        /// </param>
        /// <remarks>
        /// <code>
        /// var col = new int[] { 1, 3, 5, 7 };
        ///
        /// int sum1 = 0;
        /// col.ForEach(_number => sum1 += _number );
        ///
        /// int sum2 = col.Sum();
        /// Assert.AreEqual(sum1, sum2);
        /// </code>
        /// </remarks>
        public static void ForEach<T>( this IEnumerable<T> _set, Action<T> _operation )
        {
            foreach (var item in _set)
                _operation( item );
        }


        /// <summary>
        /// An Extension to <see cref="IEnumerable{T}"/> to allow "some arbitrary operation" to
        /// be performed on each element of the set IF AND ONLY IF the element passes the
        /// filtering criteria
        /// </summary>
        /// <typeparam name="T">
        /// The <see cref="Type"/> of the elements in the enumeration
        /// </typeparam>
        /// <param name="_set">The set of items of type T</param>
        /// <param name="_filter">
        /// The filter that determines which elements the operation is performed on
        /// </param>
        /// <param name="_operation">
        /// The operation to be performed on each item in the set
        /// </param>
        /// <remarks>
        /// <code>
        /// var col = new int[] { 1, 2, 3, 4, 5, 6 };
        ///
        /// int sum1 = 0;
        /// col.ForEach(_number => (_number &amp; 1) == 0, _number => sum1 += _number );
        ///
        ///     Assert.AreEqual( 12, sum1 );
        /// </code>
        /// </remarks>
        public static void ForEach<T>( this IEnumerable<T> _set, Func<T, bool> _filter, Action<T> _operation )
        {
            foreach (var item in _set)
            {
                if (_filter( item ))
                    _operation( item );
            }
        }


        /// <summary>
        /// An Extension to <see cref="IEnumerable{T}"/> to allow "some arbitrary operation" to
        /// be performed on each element of an enumeration, passing both the item and its index
        /// to the operation.
        /// </summary>
        /// <typeparam name="T">
        /// The implied <see cref="Type"/> of the elements in the enumeration
        /// </typeparam>
        /// <param name="_items">The enumeration of items</param>
        /// <param name="_operationWithIndex">
        /// The index of the item within the enumeration, zero-based
        /// </param>
        /// <remarks>
        /// <code>
        /// var col = new int[] { 1, 2, 3, 4, 5, 6 };
        /// 
        /// col.ForEach((_item, _index) =>
        /// {
        ///     Assert.AreEqual(_index + 1, _item, $"At Index {_index}" );
        /// } );
        /// </code>
        /// </remarks>
        public static void ForEach<T>( this IEnumerable<T> _items, Action<T, int> _operationWithIndex )
        {
            var index = 0;
            foreach (var item in _items)
                _operationWithIndex( item, index++ );
        }


        /// <summary>
        /// An Extension to IEnumerable to allow "some arbitrary operation" to be performed on
        /// each element of the set.
        /// </summary>
        /// <param name="_set">The set of items of any type</param>
        /// <param name="_operation">
        /// The operation to be performed on each item in the set
        /// </param>
        /// <remarks>
        /// <code>
        /// var stuff = new object[] { "hello", 42, DateTime.Now };
        ///
        /// stuff.ForEach(_obj =>
        /// {
        ///     switch (_obj)
        ///     {
        ///     case string _:
        ///         Assert.AreEqual(stuff[0], _obj);
        ///         break;
        ///     case int _:
        ///         Assert.AreEqual(stuff[1], _obj);
        ///         break;
        ///     case DateTime _:
        ///         Assert.AreEqual(stuff[2], _obj);
        ///         break;
        ///     default:
        ///         Assert.Fail( "No Matching Object Type" );
        ///         break;
        ///     }
        /// } );
        /// </code>
        /// </remarks>
        public static void ForEach( this IEnumerable _set, Action<object> _operation )
        {
            foreach (var item in _set)
                _operation( item );
        }


        /// <summary>
        /// Makes sure the enumeration is actually "run". This is a no-overhead way of making
        /// something like a ".ToArray()" or a "foreach" STATEMENT (not an operator) not
        /// necessary.
        /// </summary>
        /// <param name="stuff"></param>
        public static void Run( this IEnumerable stuff )
        {
            foreach (var _ in stuff) ;
        }
    }
}
