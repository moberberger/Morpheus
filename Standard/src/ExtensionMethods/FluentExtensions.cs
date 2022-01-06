using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morpheus
{
    public static class FluentExtensions
    {
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
        public static T If<T>( this bool condition, Func<T> trueAction, Func<T> falseAction ) =>
            condition
                ? trueAction()
                : falseAction();

        public static T With<T>( this T obj, Action<T> operation )
        {
            operation( obj );
            return obj;
        }

        public static T Translate<T, U>( this U obj, Func<U, T> operation ) =>
            operation( obj );

        public static IEnumerable<int> AsRange( this int max ) => Enumerable.Range( 0, max );
    }
}
