using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Collections;

#pragma warning disable IDE1006 // Naming Styles

namespace Morpheus
{
    /// <summary>
    /// An enumerator used to return all possible combinations of numbers between 1 and N taken
    /// K at a time.
    /// </summary>
    public static class Combinations
    {
        /// <summary>
        /// An enumerator used to return all possible combinations of numbers between 1 and N
        /// taken K at a time.
        /// </summary>
        /// <remarks>
        /// It is critical to note that the returned array SHOULD NOT BE MODIFIED! It represents
        /// the state of the generator. If its modified, then the generator will not work
        /// correctly.
        /// </remarks>
        /// <returns>
        /// A set of _K unique integers from the set of [0.N). Caller SHOULD NOT MODIFY ANY
        /// ELEMENTS OF THIS ARRAY. To achieve performance, this returned array is also the
        /// curent state of the generator.
        /// </returns>
        /// <remarks>
        /// <code>
        /// var expected = new HashSet&lt;string> { "01", "12", "02" };
        /// 
        /// // Should return all combinations of {0, 1, 2}
        /// foreach (var x in Combinations.Integers( 3, 2 ))
        /// {
        ///     int lower = Math.Min( x[0], x[1] );
        ///     int higher = Math.Max( x[0], x[1] );
        ///     var str = $"{lower}{higher}";
        /// 
        ///     if (expected.Contains( str ))
        ///         expected.Remove( str );
        ///     else
        ///         Assert.Fail( $"{str} was not in the set when it should be" );
        /// }
        /// 
        /// Assert.AreEqual( 0, expected.Count );
        /// </code>
        /// </remarks>
        public static IEnumerable<int[]> Integers( int _N, int _K )
        {
            var current = new int[_K];
            var i = 0;

            do
            {
                while (++i < _K)
                    current[i] = current[i - 1] + 1;

                yield return current;

                for (i = _K - 1; i >= 0; i--)
                {
                    current[i]++;
                    // A successful increment at index i means its OK to stop looking backwards
                    // in "current"
                    if (current[i] < (_N - (_K - i - 1)))
                        break;
                }

                if (i < 0) // deal with the last decrement if it occured
                    i = 0; // breaks first "while" loop if not handled.

            } while (current[0] <= _N - _K);
        }

        /// <summary>
        /// Return each of the combinations of elements from each of a set of enumerations.
        /// </summary>
        /// <param name="lists"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IEnumerable<object[]> ObjectLists( params IEnumerable[] lists )
        {
            foreach (var obj in ObjectLists( lists, new object[lists.Length], 0 ))
                yield return obj;
        }

        private static IEnumerable<object[]> ObjectLists( IEnumerable[] lists, object[] current, int index )
        {
            if (lists == null) throw new ArgumentNullException( "lists cannot be null" );

            int dim = lists.Length;
            if (current == null)
            {
                current = new object[dim];
                index = 0;
            }

            if (index == dim)
            {
                yield return current;
            }
            else
            {
                foreach (object item in lists[index])
                {
                    current[index] = item;
                    foreach (var obj in ObjectLists( lists, current, index + 1 ))
                        yield return obj;
                }
            }
        }
    }

    /// <summary>
    /// A fast way of returning all permutations of _K integers at a time from a set of _N
    /// integers.
    /// </summary>
    /// <remarks>
    /// It is critical to note that the returned array SHOULD NOT BE MODIFIED! It represents the
    /// state of the permutation generator. If its modified, then the generator will not work
    /// correctly.
    /// <code>
    /// var expected = new HashSet&lt;string>
    /// {
    ///     "01", "12", "02",
    ///     "10", "21", "20",
    /// };
    /// 
    /// // Should return all combinations of {0, 1, 2}
    /// foreach (var x in Permutations.Integers( 3, 2 ))
    /// {
    ///     int first = x[0];
    ///     int second = x[1];
    ///     var str = $"{first}{second}";
    /// 
    ///     if (expected.Contains( str ))
    ///         expected.Remove( str );
    ///     else
    ///         Assert.Fail( $"{str} was not in the set when it should be" );
    /// }
    /// 
    /// Assert.AreEqual( 0, expected.Count );
    /// </code>
    /// </remarks>
    public static class Permutations
    {
        /// <summary>
        /// A fast way of returning all permutations of _K integers at a time from a set of _N
        /// integers.
        /// </summary>
        /// <remarks>
        /// It is critical to note that the returned array SHOULD NOT BE MODIFIED! It represents
        /// the state of the permutation generator. If its modified, then the generator will not
        /// work correctly.
        /// </remarks>
        /// <param name="_N">The highest of the integers to return. Returns [0.._N)</param>
        /// <param name="_K">The number of integers to return each time.</param>
        /// <returns>
        /// A set of _K unique integers from the set of [0.N). Caller SHOULD NOT MODIFY ANY
        /// ELEMENTS OF THIS ARRAY. To achieve performance, this returned array is also the
        /// curent state of the generator.
        /// </returns>
        public static IEnumerable<int[]> Integers( int _N, int _K )
        {
            var permutationArray = new int[_K];
            var c = new int[_K];

            foreach (var combinationArray in Combinations.Integers( _N, _K ))
            {
                yield return combinationArray;

                Array.Copy( combinationArray, permutationArray, _K );
                for (var i = 0; i < _K;) // no increment here
                {
                    if (c[i] < i)
                    {
                        if ((i & 1) == 0) // even
                            permutationArray.SwapElements( 0, i );
                        else
                            permutationArray.SwapElements( c[i], i );
                        yield return permutationArray;
                        c[i]++;
                        i = 0;
                    }
                    else
                    {
                        c[i++] = 0;
                    }
                }
            }
        }
    }
}
#pragma warning restore IDE1006 // Naming Styles
