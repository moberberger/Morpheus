using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morpheus
{
    /// <summary>
    /// A collection of various sorting algorithms that may be faster than the IntroSort implemented from within the
    /// <see cref="System.Array"/> class. (Note: It was QuickSort up until 4.5) (Note 2: Its still not stable)
    /// </summary>
    public static class CSortingAlgorithms
    {
        /// <summary>
        /// Sort the IList using an INSERTION SORT- O(n^2) in complexity, but it is a STABLE sort.
        /// Uses the default comparator for objects of type T
        /// </summary>
        /// <typeparam name="T">The type of the elements in the list</typeparam>
        /// <param name="_list">The list of items to sort</param>
        public static void InsertionSort<T>( this IList<T> _list )
            where T : IComparable => _list.InsertionSort( ( _x, _y ) => _x.CompareTo( _y ) );

        /// <summary>
        /// Sort the IList using an INSERTION SORT- O(n^2) in complexity, but it is a STABLE sort
        /// </summary>
        /// <typeparam name="T">The type of the elements in the list</typeparam>
        /// <param name="_list">The list of items to sort</param>
        /// <param name="_comparer">The comparer for the sort</param>
        public static void InsertionSort<T>( this IList<T> _list, IComparer<T> _comparer ) => _list.InsertionSort( _comparer.Compare );

        /// <summary>
        /// Sort the IList using an INSERTION SORT- O(n^2) in complexity, but it is a STABLE sort
        /// </summary>
        /// <typeparam name="TList">The type of the elements in the list</typeparam>
        /// <typeparam name="TSelector"></typeparam>
        /// <param name="_list">The list of items to sort</param>
        /// <param name="_selector">Selects a member in each TList item to use with the DEFAULT COMPARER</param>
        public static void InsertionSort<TList, TSelector>( this IList<TList> _list, Func<TList, TSelector> _selector ) => _list.InsertionSort( ( _x, _y ) => Comparer.Default.Compare( _selector( _x ), _selector( _y ) ) );

        /// <summary>
        /// Sort the IList using an INSERTION SORT- O(n^2) in complexity, but it is a STABLE sort
        /// </summary>
        /// <typeparam name="T">The type of the elements in the list</typeparam>
        /// <param name="_list">The list of items to sort</param>
        /// <param name="_comparer">The comparer for the sort</param>
        public static void InsertionSort<T>( this IList<T> _list, Comparison<T> _comparer )
        {
            for (var i = 1; i < _list.Count; i++) // start at 1 because item 0 is already sorted
            {
                var val = _list[i];
                int j;
                for (j = i - 1; (j >= 0) && (_comparer( val, _list[j] ) < 0); j--)
                    _list[j + 1] = _list[j];

                _list[j + 1] = val;
            }
        }


        /// <summary>
        /// The QuickSelect algorithm- Split an array such that all items to the "left" of the position requested are "less than"
        /// the position, and all items to the "right" of the position are "greater than" the item in that position. Useful for
        /// placing the median of an array at the middle of the array.
        /// </summary>
        /// <typeparam name="T">The Type of elements in the array</typeparam>
        /// <param name="_array">The array</param>
        /// <param name="_low">Consider this index the "beginning" of the array</param>
        /// <param name="_high">Consider this index the "end" of the array</param>
        /// <param name="_position">The position for which elements to the left are less-than, and the elements to the right are greater than</param>
        /// <param name="_comparer">The comparer to use to compare the elements to each other</param>
        public static void QuickSelect<T>( this T[] _array, int _low, int _high, int _position, IComparer<T> _comparer )
        {
#if DEBUG
            if (_position < _low || _position > _high || _low > _high || _low < 0 || _low >= _array.Length || _high < 0 || _high >= _array.Length)
            {
                var str = string.Format( "Invalid parameters: low={0}, high={1}, position={2}, arrayLength={3}", _low, _high, _position, _array.Length );
                throw new ArgumentOutOfRangeException( str );
            }
#endif

            // Go through a trivial sorting process for just a few elements
            var len = _high - _low + 1;

            // For length = {1,2,3}, simply sort the array fragment using optimized techniques. This is guaranteed to make "position" correct.

            // Array is too small to order - it must be correct
            if (len <= 1)
                return;

            var bad12 = _comparer.Compare( _array[_low], _array[_low + 1] ) > 0;
            // Array is only 2 elements- see if they need to be swapped
            if (len == 2)
            {
                if (bad12)
                    _array.SwapItems( _low, _low + 1 );
                return;
            }

            var mid = ((_high - _low) >> 1) + _low;
            var bad23 = _comparer.Compare( _array[mid], _array[_high] ) > 0; // mid is at least 1 greater than _low now
            bool bad13; // Don't calculate this yet- there's a chance its not needed if len==3 and the ordering is right

            // Optimized ordering for 3 elements
            if (len == 3)
            {
                if (!bad12 && !bad23) // already in order
                    return;
                if (bad12 && bad23) // both 12 and 23 are out of order, then swap 1 and 3
                {
                    _array.SwapItems( _low, _low + 2 );
                    return;
                }

                bad13 = _comparer.Compare( _array[_low], _array[_low + 2] ) > 0;
                if (!bad13)
                {
                    if (bad12)
                        _array.SwapItems( _low, _low + 1 );
                    else
                        _array.SwapItems( _low + 1, _low + 2 );
                }
                else
                {
                    if (bad12)
                    {
                        var tmp = _array[_low];
                        _array[_low] = _array[_low + 1];
                        _array[_low + 1] = _array[_low + 2];
                        _array[_low + 2] = tmp;
                    }
                    else
                    {
                        var tmp = _array[_low + 2];
                        _array[_low + 2] = _array[_low + 1];
                        _array[_low + 1] = _array[_low + 0];
                        _array[_low] = tmp;
                    }
                }
                return;
            }

            // There are 4 or more, so try to pick a pivot by looking at the middle value from _low, _high and mid
            if (len > 4)
                bad12 = _comparer.Compare( _array[_low], _array[mid] ) > 0; // at 5 or more, mid will be greater than low+1, so re-set "bad12"

            bad13 = _comparer.Compare( _array[_low], _array[_high] ) > 0;
            var pivotIdx = mid;
            if (bad12 != bad23) // The "real" pivot idx is either _low or _high
            {
                if (bad13 == bad23)
                    pivotIdx = _low;
                else
                    pivotIdx = _high;
            }
            if (pivotIdx != _high)
                _array.SwapItems( _high, pivotIdx ); // Now the pivot element is in [_high]

            var idx = _low; // index points just past the last lower-than-pivot value found- start at first element
            while (idx < _high && _comparer.Compare( _array[_high], _array[idx] ) > 0) // while the left-most elements are less than pivot value
                idx++; // move idx forward

            // now, idx points to the first element greater than the pivot value
            for (var i = idx + 1; i < _high; i++)
            {
                if (_comparer.Compare( _array[_high], _array[i] ) > 0) // pivot value is greater than this element => swap it down
                {
                    _array.SwapItems( idx, i );
                    idx++;
                }
            }

            // Now that we've gone through all the items, 
            _array.SwapItems( _high, idx ); // swap pivot back into position- idx points to first higher item than pivot

            if (idx < _position) // The pivot value is to the left of the correct position
                QuickSelect( _array, idx + 1, _high, _position, _comparer );
            else if (idx > _position) // the pivot value is to the right of the correct position
                QuickSelect( _array, _low, idx - 1, _position, _comparer );
        }

        /// <summary>
        /// Swap the elements at two indicies in the array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_array"></param>
        /// <param name="_idx1"></param>
        /// <param name="_idx2"></param>
        public static void SwapItems<T>( this T[] _array, int _idx1, int _idx2 )
        {
            var tmp = _array[_idx1];
            _array[_idx1] = _array[_idx2];
            _array[_idx2] = tmp;
        }

    }
}
