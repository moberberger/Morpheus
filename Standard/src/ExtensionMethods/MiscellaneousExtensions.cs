using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Morpheus
{
    /// <summary>
    /// Some extension methods for collections of weird things.
    /// </summary>
    public static class MiscellaneousExtensions
    {
        /// <summary>
        /// Given an enumeration of threads, "Join" each thread before returning.
        /// </summary>
        /// <param name="_threads">An enumeration of threads</param>
        public static void JoinAll( this IEnumerable<Thread> _threads )
        {
            foreach (var t in _threads)
                t.Join();
        }

        /// <summary>
        /// Provide an infinite number of items by "re-playing" the items in the specified
        /// collection over and over again.
        /// </summary>
        /// <typeparam name="T">The Type of items in the enumeration</typeparam>
        /// <param name="_items">The items to loop through indefinitely</param>
        /// <returns>A never ending enumeration of items from _items</returns>
        /// <remarks>
        /// <code>
        /// var items = new int[] { 1, 2, 3 };
        /// int count = 0;
        /// int sum = 0;
        /// 
        /// foreach (var x in items.LoopInfinitely())
        /// {
        ///     count++;
        ///     sum += x;
        /// 
        ///     if (count == 5) break;
        /// }
        /// 
        /// // 1 + 2 + 3 + 1 + 2 = 9
        /// Assert.AreEqual( 9, sum );
        /// </code>
        /// </remarks>
        public static IEnumerable<T> LoopInfinitely<T>( this IEnumerable<T> _items )
        {
            while (true)
            {
                foreach (var item in _items)
                    yield return item;
            }
        }

        /// <summary>
        /// Strongly Typed version of <see cref="Interlocked.Exchange(ref object, object)"/> .
        /// This is just a thin wrapper around that method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_this">
        /// The object from the variable that needs to get the other value
        /// </param>
        /// <param name="_other">
        /// [in] The value to return; [out] The reference to "this" object
        /// </param>
        /// <returns>
        /// Return value is the value of the _other parameter that was passed in, while the
        /// _other value is replaced with "this" object.
        /// </returns>
        /// <remarks>
        /// <code>
        /// string x = "Alpha";
        /// string y = "Beta";
        /// 
        /// x = x.Swap( ref y );
        /// 
        /// Assert.AreEqual( "Beta", x );
        /// Assert.AreEqual( "Alpha", y );
        /// </code>
        /// <para> Review of <see cref="Interlocked.Exchange(ref object, object)"/> is
        /// encouraged to help understand the usage pattern. </para>
        /// </remarks>
        public static T Swap<T>( this object _this, ref T _other )
            where T : class
            => Interlocked.Exchange( ref _other, (T)_this );


        /// <summary>
        /// Continuously call a generator function until the value returned passes the
        /// _predicate test provided.
        /// </summary>
        /// <typeparam name="T">Any Type</typeparam>
        /// <param name="_generator">Generator of objects</param>
        /// <param name="_predicate">
        /// The predicate determining when to stop generating and the value that caused
        /// generation to stop.
        /// </param>
        /// <returns>
        /// The generated value that first caused the _predicate to return TRUE
        /// </returns>
        public static T GenerateUntil<T>( this Func<T> _generator, Func<T, bool> _predicate )
        {
            while (true)
            {
                var val = _generator();
                if (_predicate( val ))
                    return val;
            }
        }


        public static StringBuilder AppendIf( this StringBuilder sb, bool condition, string ifTrue, string ifFalse = "" )
            => sb.Append( condition ? ifTrue : ifFalse );

        public static StringBuilder AppendLines( this StringBuilder sb, IEnumerable objects )
        {
            foreach (var obj in objects)
                sb.AppendLine( obj.ToString() );
            return sb;
        }

        public static StringBuilder Append( this StringBuilder sb, string s, int count )
        {
            while (count-- > 0) sb.Append( s );
            return sb;
        }

        public static StringBuilder AppendPadded( this StringBuilder sb, string text, int width, GridAlignments alignment = GridAlignments.Left )
        {
            text = text.Trim();

            if ( text.Length >= width )
            {
                text = text[..width];
                sb.Append( text );
            }
            else
            {
                int padding = width - text.Length;

                // Add the left padding
                if (alignment == GridAlignments.Center)
                    sb.Append( ' ', padding / 2 );
                else if (alignment == GridAlignments.Right)
                    sb.Append( ' ', padding );

                // Add the string
                sb.Append( text );

                // Add the right padding
                if (alignment == GridAlignments.Center)
                    sb.Append( ' ', padding - padding / 2 );
                else if (alignment == GridAlignments.Left)
                    sb.Append( ' ', padding );
            }

            return sb;
        }
    }
}
