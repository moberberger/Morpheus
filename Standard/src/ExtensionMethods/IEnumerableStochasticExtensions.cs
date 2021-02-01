using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Morpheus
{
    /// <summary>
    /// All of these extension methods operate with a random component.
    /// </summary>
    public static class IEnumerableStochasticExtensions
    {
        /// <summary>
        /// Return a version of the enumeration with all values re-arranged randomly.
        /// </summary>
        /// <typeparam name="T">Arbitrary datatype</typeparam>
        /// <param name="_items">The collection to shuffle</param>
        /// <param name="_rng">The random number generator to use</param>
        /// <returns>The collection with all elements randomly re-ordered</returns>
        public static IList<T> Shuffled<T>( this IEnumerable<T> _items, Random _rng = null )
        {
            var list = new List<T>( _items );
            list.Shuffle( _rng );
            return list;
        }


        /// <summary>
        /// Retrieve an item from an enumeration based on a weighted "sampling" of all objects
        /// in the enumeration.
        /// </summary>
        /// <typeparam name="T">The type of object being sampled</typeparam>
        /// <param name="_data">The enumeration of objects being sampled</param>
        /// <param name="_selector">
        /// The selector which returns the "Value" for an object in the enumeration- MUST YIELD
        /// POSITIVE VALUES
        /// </param>
        /// <param name="_inverse">
        /// When TRUE, weight the smallest valued objects the most
        /// </param>
        /// <returns>An object from the collection selected based on the _selector</returns>
        public static T Sample<T>( this IEnumerable<T> _data, Func<T, double> _selector, bool _inverse ) => _data.Sample( _selector, Rng.Default.NextDouble(), _inverse );

        /// <summary>
        /// Retrieve an item from an enumeration based on a weighted "sampling" of all objects
        /// in the enumeration.
        /// </summary>
        /// <typeparam name="T">The type of object being sampled</typeparam>
        /// <param name="_data">The enumeration of objects being sampled</param>
        /// <param name="_selector">
        /// The selector which returns the "Value" for an object in the enumeration- MUST YIELD
        /// POSITIVE VALUES
        /// </param>
        /// <param name="_selection">
        /// The selection- a double between 0 and 1 (as if from Random.NextDouble())
        /// </param>
        /// <param name="_inverse">
        /// When TRUE, weight the smallest valued objects the most
        /// </param>
        /// <returns>An object from the collection selected based on the _selector</returns>
        public static T Sample<T>( this IEnumerable<T> _data, Func<T, double> _selector, double _selection, bool _inverse )
        {
            var count = 0;
            double sumValues = 0, sumInverses = 0;

            foreach (var obj in _data)
            {
                var val = _selector( obj );
                if (double.IsNaN( val ) || double.IsInfinity( val ) || val <= 0)
                    throw new XException( $"The SELECTOR returned  [ {val} ] , which is invalid." );

                count++;
                sumValues += val;
                sumInverses += 1.0 / val;
            }

            if (count == 0)
                return default;
            if (count == 1)
                return _data.First();

            double running = 0;
            foreach (var obj in _data)
            {
                var val = _selector( obj );

                var increment = _inverse ? 1.0 / (sumInverses * val) : val / sumValues;
                running += increment;

                if (running >= _selection)
                    return obj;
            }
            throw new XException( "Never should run out of objects in the sampled set" );
        }



        /// <summary>
        /// Select a random element from the collection
        /// </summary>
        /// <typeparam name="T">The type of the elements in the enumeration</typeparam>
        /// <param name="_collection">The elements that could be selected</param>
        /// <returns>A random element in the enumeration</returns>
        public static T SelectRandom<T>( this IEnumerable<T> _collection ) => _collection.SelectRandom( Rng.Default );

        /// <summary>
        /// Select a random element from the collection
        /// </summary>
        /// <typeparam name="T">The type of the elements in the enumeration</typeparam>
        /// <param name="_collection">The elements that could be selected</param>
        /// <param name="_rng">A <see cref="Random"/> object used to select an item</param>
        /// <returns>A random element in the enumeration</returns>
        public static T SelectRandom<T>( this IEnumerable<T> _collection, Random _rng )
        {
            var count = _collection.Count();
            var idx = _rng.Next( count );
            return _collection.ElementAt( idx );
        }


        /// <summary>
        /// Shuffle all elements in an IList to a random ordering
        /// </summary>
        /// <typeparam name="T">The type of the elements in the list</typeparam>
        /// <param name="_list">The list of things to shuffle</param>
        /// <param name="_rng">The random number generator to use</param>
        /// <exception cref="ArgumentNullException">
        /// If used as a static function with a null collection
        /// </exception>
        public static IList<T> Shuffle<T>( this IList<T> _list, Random _rng = null )
        {
            if (_list is null)
                throw new ArgumentNullException( nameof( _list ) );

            _rng = _rng ?? Rng.Default;

            var count = _list.Count;
            for (var i = 0; i < count; i++)
            {
                var idx = _rng.Next( count );
                var tmp = _list[i];
                _list[i] = _list[idx];
                _list[idx] = tmp;
            }
            return _list;
        }
    }
}
