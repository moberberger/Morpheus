using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morpheus
{
    public static class FluentExtensions
    {
        /// <summary>
        /// Change the "current" object, but make sure that if the current object is enumerable,
        /// that enumeration is "run" using a foreach statement
        /// </summary>
        /// <typeparam name="T">The new type for Current</typeparam>
        /// <typeparam name="U">The type of "current"</typeparam>
        /// <returns>A new object as the "current" fluent object</returns>
        public static T NowUse<T, U>( this U current, T newObj )
        {
            if (current is IEnumerable && !(
                    current is string ||
                    current is StringBuilder ||
                    current is Array))
            {
                foreach (var _ in current as IEnumerable) ;
            }

            return newObj;
        }

        /// <summary>
        /// Fluent form of canonical condition
        /// </summary>
        public static T If<T>( this bool condition, Func<T> trueAction, Func<T> falseAction ) =>
            condition
                ? trueAction()
                : falseAction();

        /// <summary>
        /// Execute an operation on "current", but return "current" as the operation has no
        /// return value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        public static T With<T>( this T obj, Action<T> operation )
        {
            operation( obj );
            return obj;
        }

        public static IEnumerable<T> With<T>( this IEnumerable<T> list, Action<T> action )
        {
            foreach (var x in list)
            {
                action( x );
                yield return x;
            }
        }

        /// <summary>
        /// <see cref="Enumerable.Select{}"/> but for objects instead of enumerations
        /// </summary>
        public static T Translate<T, U>( this U obj, Func<U, T> operation ) =>
            operation( obj );

        /// <summary>
        /// Thin wrapper around <see cref="Enumerable.Range(int, int)"/> that only considers one
        /// value (the max value), relying on optimizations found in the Enumerable version
        /// </summary>
        public static IEnumerable<int> AsRange( this int max ) => Enumerable.Range( 0, max );
    }
}
