using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Morpheus
{
    /// <summary>
    /// A bunch of extensions for <see cref="IEnumerable{T}"/> objects that operate on the
    /// <see cref="Type"/> s of the objects.
    /// </summary>
    public static class CastingExtensions
    {
        /// <summary>
        /// Return all items in the enumeration that are NOT of a specific type
        /// </summary>
        /// <typeparam name="T">The type that we DON'T want</typeparam>
        /// <param name="_items">The collection of items</param>
        /// <returns>
        /// All items in the collection that CANNOT be cast into the specified type
        /// </returns>
        public static IEnumerable<object> NotOfType<T>( this IEnumerable<object> _items )
        {
            foreach (var o in _items)
            {
                if (!(o is T))
                    yield return o;
            }
        }

        /// <summary>
        /// Return all items in an enumeration that are of the type specified- does include
        /// items derived from the type specified
        /// </summary>
        /// <typeparam name="T">The Type of items expected from the enumeration</typeparam>
        /// <param name="_collection">The collection of items</param>
        /// <param name="_type">The Type of items expected from the enumeration</param>
        /// <returns>
        /// Only those items in the enumeration where GetType() returns the type specifed.
        /// </returns>
        public static IEnumerable<T> OfType<T>( this IEnumerable<T> _collection, Type _type )
        {
            foreach (var obj in _collection)
            {
                if (_type.IsInstanceOfType( obj ))
                    yield return obj;
            }
        }

        /// <summary>
        /// Return all items in an enumeration that are exactly the type specified- does not
        /// include items derived from the type specified
        /// </summary>
        /// <typeparam name="T">The Type of items expected from the enumeration</typeparam>
        /// <param name="_collection">The collection of items</param>
        /// <returns>
        /// Only those items in the enumeration where GetType() returns the type specifed.
        /// </returns>
        public static IEnumerable<T> OfTypeExactly<T>( this IEnumerable<T> _collection )
        {
            foreach (var obj in _collection)
            {
                if (obj.GetType() == typeof( T ))
                    yield return obj;
            }
        }

        /// <summary>
        /// Return all items in an enumeration that are exactly the type specified- does not
        /// include items derived from the type specified
        /// </summary>
        /// <typeparam name="T">The Type of items expected from the enumeration</typeparam>
        /// <param name="_collection">The collection of items</param>
        /// <param name="_type">The Type of items expected from the enumeration</param>
        /// <returns>
        /// Only those items in the enumeration where GetType() returns the type specifed.
        /// </returns>
        public static IEnumerable<T> OfTypeExactly<T>( this IEnumerable<T> _collection, Type _type )
        {
            foreach (var obj in _collection)
            {
                if (obj.GetType() == _type)
                    yield return obj;
            }
        }
    }
}
