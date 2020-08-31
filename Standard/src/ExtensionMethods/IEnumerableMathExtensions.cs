using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Morpheus
{
    /// <summary>
    /// Some extension methods that operate on numbers.
    /// </summary>
    public static class MathRelatedExtensions
    {
        #region Data Types

        /// <summary>
        /// Base class for number statistics
        /// </summary>
        public abstract class NumberStats
        {
            /// <summary>
            /// The number of values considered
            /// </summary>
            public long Count = 0;

            /// <summary>
            /// The average of all values (not an integer)
            /// </summary>
            public double Average = double.NaN;
        }

        /// <summary>
        /// Simple Statistics for data of type Int32
        /// </summary>
        public class LongStats : NumberStats
        {
            /// <summary>
            /// The smallest value found
            /// </summary>
            public long Minimum = long.MaxValue;
            /// <summary>
            /// The largest value found
            /// </summary>
            public long Maximum = long.MinValue;
            /// <summary>
            /// The sum of all values
            /// </summary>
            public long Sum;

            /// <summary>
            /// The range of values- Basically the difference between Maximum and Minimum
            /// </summary>
            public long Range => Maximum - Minimum + 1;

            /// <summary>
            /// Accumulate a value into this stats object
            /// </summary>
            /// <param name="_value"></param>
            public void Accumulate( long _value )
            {
                if (_value < Minimum) Minimum = _value;
                if (_value > Maximum) Maximum = _value;
                Sum += _value;
                Count++;
                Average = (double)Sum / Count;
            }
        }

        /// <summary>
        /// Simple Statistics for data of type Double
        /// </summary>
        public class DoubleStats : NumberStats
        {
            /// <summary>
            /// The smallest value found
            /// </summary>
            public double Minimum = double.MaxValue;
            /// <summary>
            /// The largest value found
            /// </summary>
            public double Maximum = double.MinValue;
            /// <summary>
            /// The sum of all values
            /// </summary>
            public double Sum;

            /// <summary>
            /// The range of values- Basically the difference between Maximum and Minimum
            /// </summary>
            public double Range => Maximum - Minimum;

            /// <summary>
            /// Accumulate a value into this stats object
            /// </summary>
            /// <param name="_value"></param>
            public void Accumulate( double _value )
            {
                if (_value < Minimum) Minimum = _value;
                if (_value > Maximum) Maximum = _value;
                Sum += _value;
                Count++;
                Average = Sum / Count;
            }
        }

        /// <summary>
        /// Simple Statistics for data of type DateTime
        /// </summary>
        public class DateStats
        {
            /// <summary>
            /// The Earliest date found in the enumeration
            /// </summary>
            public DateTime Earliest;
            /// <summary>
            /// The Latest date found in the enumeration
            /// </summary>
            public DateTime Latest;
            /// <summary>
            /// The number of dates considered
            /// </summary>
            public long Count;
            /// <summary>
            /// The difference between the <see cref="Latest"/> and <see cref="Earliest"/> dates
            /// </summary>
            public TimeSpan Range => Latest - Earliest;

            /// <summary>
            /// Accumulate a value into this stats object
            /// </summary>
            /// <param name="_value"></param>
            public void Accumulate( DateTime _value )
            {
                if (_value < Earliest) Earliest = _value;
                if (_value > Latest) Latest = _value;
                Count++;
            }
        }
        #endregion

        /// <summary>
        /// Return the standard deviation (not the sample standard deviation) of an enumeration
        /// of doubles. The generalized form, using selectors to convert from your data to a
        /// double, are found below.
        /// </summary>
        /// <param name="_population">The data to get the standard deviation for</param>
        /// <returns>The standard deviation for the collection of double values</returns>
        public static double StandardDeviation( this IEnumerable<double> _population )
        {
            var average = _population.Average();
            return _population.StandardDeviation( average );
        }

        /// <summary>
        /// Return the standard deviation (not the sample standard deviation) of an enumeration
        /// of doubles. The generalized form, using selectors to convert from your data to a
        /// double, are found below. This version assumes that you already know the average of
        /// the population of data.
        /// </summary>
        /// <param name="_population">The data to get the standard deviation for</param>
        /// <param name="_preComputedAverage">
        /// The average of the data, calculated from outside this method
        /// </param>
        /// <returns>The standard deviation for the collection of double values</returns>
        public static double StandardDeviation( this IEnumerable<double> _population, double _preComputedAverage )
        {
            double sum = 0;
            var count = 0;

            foreach (var x in _population)
            {
                count++;
                var xx = x - _preComputedAverage;
                sum += xx * xx;
            }

            return Math.Sqrt( sum / count );
        }

        /// <summary>
        /// Return the standard deviation (not the sample standard deviation) of an enumeration
        /// of doubles.
        /// </summary>
        /// <typeparam name="T">The Type of each element in the population</typeparam>
        /// <param name="_population">The population of data</param>
        /// <param name="_selector">
        /// Transform each element of the population into a "double" value
        /// </param>
        /// <returns>The standard deviation of the population</returns>
        public static double StandardDeviation<T>( this IEnumerable<T> _population, Func<T, double> _selector )
        {
            var average = _population.Average( _selector );
            return _population.StandardDeviation( _selector, average );
        }


        /// <summary>
        /// Return the standard deviation (not the sample standard deviation) of an enumeration
        /// of doubles. This version assumes that you already know the average of the population
        /// of data.
        /// </summary>
        /// <typeparam name="T">The Type of each element in the population</typeparam>
        /// <param name="_population">The population of data</param>
        /// <param name="_selector">
        /// Transform each element of the population into a "double" value
        /// </param>
        /// <param name="_preComputedAverage">
        /// The average of the population, computed prior to calling this routine
        /// </param>
        /// <returns>The standard deviation of the population</returns>
        public static double StandardDeviation<T>( this IEnumerable<T> _population, Func<T, double> _selector, double _preComputedAverage )
        {
            double sum = 0;
            var count = 0;

            foreach (var x in _population)
            {
                count++;
                var xx = _selector( x ) - _preComputedAverage;
                sum += xx * xx;
            }

            return Math.Sqrt( sum / count );
        }


        /// <summary>
        /// Return a set of stats (Min, Max, Avg, Total, Count) on a collection of numbers.
        /// </summary>
        /// <param name="_data">The data to analyse</param>
        /// <returns>The stats on the data</returns>
        public static LongStats GetStats( this IEnumerable<long> _data ) => _data.GetStats( _x => _x );

        /// <summary>
        /// Return a set of stats (Min, Max, Avg, Total, Count) on a collection of numbers.
        /// </summary>
        /// <typeparam name="T">The Type of the data in the collection</typeparam>
        /// <param name="_data">The data to analyse</param>
        /// <param name="_selector">The selctor to get an INT from the data</param>
        /// <returns>The stats on the data</returns>
        public static LongStats GetStats<T>( this IEnumerable<T> _data, Func<T, long> _selector )
        {
            var retval = new LongStats();

            foreach (var row in _data)
            {
                var val = _selector( row );
                retval.Accumulate( val );
            }

            return retval;
        }


        /// <summary>
        /// Return a set of stats (Min, Max, Avg, Total, Count) on a collection of numbers.
        /// </summary>
        /// <param name="_data">The data to analyse</param>
        /// <returns>The stats on the data</returns>
        public static DoubleStats GetStats( this IEnumerable<double> _data ) => _data.GetStats( _x => _x );

        /// <summary>
        /// Return a set of stats (Min, Max, Avg, Total, Count) on a collection of numbers.
        /// </summary>
        /// <typeparam name="T">The Type of the data in the collection</typeparam>
        /// <param name="_data">The data to analyse</param>
        /// <param name="_selector">The selctor to get an INT from the data</param>
        /// <returns>The stats on the data</returns>
        public static DoubleStats GetStats<T>( this IEnumerable<T> _data, Func<T, double> _selector )
        {
            var retval = new DoubleStats();

            foreach (var row in _data)
            {
                var val = _selector( row );
                retval.Accumulate( val );
            }

            return retval;
        }

        /// <summary>
        /// Return a set of stats (Earliest, Latest, Range, Count) on a collection of dates.
        /// </summary>
        /// <param name="_data">The data to analyse</param>
        /// <returns>The stats on the data</returns>
        public static DateStats GetStats( this IEnumerable<DateTime> _data ) => _data.GetStats( _x => _x );

        /// <summary>
        /// Return a set of stats (Earliest, Latest, Range, Count) on a collection of dates.
        /// </summary>
        /// <typeparam name="T">The Type of the data in the collection</typeparam>
        /// <param name="_data">The data to analyse</param>
        /// <param name="_selector">The selctor to get a DateTime from the data</param>
        /// <returns>The stats on the data</returns>
        public static DateStats GetStats<T>( this IEnumerable<T> _data, Func<T, DateTime> _selector )
        {
            var retval = new DateStats();

            foreach (var row in _data)
            {
                var val = _selector( row );
                retval.Accumulate( val );
            }

            return retval;
        }

        /// <summary>
        /// Return all items in an enumeration that are "above average", where a selector is
        /// used to determine what is used to define "average"
        /// </summary>
        /// <typeparam name="T">The Type of items in the enumeration</typeparam>
        /// <param name="_items">The items in the list to search</param>
        /// <param name="_selector">
        /// The function that determines which "field" of an element is used to determine
        /// "average"
        /// </param>
        /// <returns>
        /// All items in the enumeration that are strictly larger than the average
        /// </returns>
        public static IEnumerable<T> AboveAverage<T>( this IEnumerable<T> _items, Func<T, double> _selector )
        {
            var average = _items.Average( _selector );
            return _items.Where( _item => _selector( _item ) > average );
        }

        /// <summary>
        /// Return all items in an enumeration that are "below average", where a selector is
        /// used to determine what is used to define "average"
        /// </summary>
        /// <typeparam name="T">The Type of items in the enumeration</typeparam>
        /// <param name="_items">The items in the list to search</param>
        /// <param name="_selector">
        /// The function that determines which "field" of an element is used to determine
        /// "average"
        /// </param>
        /// <returns>
        /// All items in the enumeration that are strictly smaller than the average
        /// </returns>
        public static IEnumerable<T> BelowAverage<T>( this IEnumerable<T> _items, Func<T, double> _selector )
        {
            var average = _items.Average( _selector );
            return _items.Where( _item => _selector( _item ) < average );
        }

        /// <summary>
        /// Return the sum of the pairwise-products of elements of both arrays.
        /// 
        /// Optimized for use on double[]. Use Collate for general purpose objects or with
        /// <see cref="IEnumerable{T}"/> collections
        /// 
        /// Also known as the "SUMPRODUCT" in Excel
        /// </summary>
        /// <param name="left">
        /// Length determines how many pairwise-products will be summed
        /// </param>
        /// <param name="right">Must contain same number or more elements than -left-</param>
        /// <returns></returns>
        public static double DotProduct( this double[] left, double[] right )
        {
            double sum = 0;
            for (int i = 0; i < left.Length; i++)
            {
                sum += left[i] * right[i];
            }
            return sum;
        }

        /// <summary>
        /// Return the sum of the pairwise-products of elements of both arrays.
        /// 
        /// Optimized for use on decimal[]. Use Collate for general purpose objects or with
        /// <see cref="IEnumerable{T}"/> collections
        /// 
        /// Also known as the "SUMPRODUCT" in Excel
        /// </summary>
        /// <param name="left">
        /// Length determines how many pairwise-products will be summed
        /// </param>
        /// <param name="right">Must contain same number or more elements than -left-</param>
        /// <returns></returns>
        public static decimal DotProduct( this decimal[] left, decimal[] right )
        {
            decimal sum = 0;
            for (int i = 0; i < left.Length; i++)
            {
                sum += left[i] * right[i];
            }
            return sum;
        }
    }
}
