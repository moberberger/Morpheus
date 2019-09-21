using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Morpheus
{
    using _Delegate = Func<object, object[], object>;

    /// <summary>
    /// This class allows extremely fast invocation for a method in a class identified by a
    /// <see cref="MethodInfo"/> object.
    /// </summary>
    /// <remarks>
    /// Using this class's <see cref="CreateInvoker(MethodInfo)"/> method for a slightly more
    /// performant version of the proxy.
    /// </remarks>
    public class MethodProxy
    {
        /// <summary>
        /// The MethodInfo used to create this object
        /// </summary>
        public MethodInfo MethodInfo { get; private set; }

        /// <summary>
        /// A general lambda form for invoking any method
        /// </summary>
        protected _Delegate m_invoker;

        /// <summary>
        /// Create a new proxy for a method identified by a <see cref="MethodInfo"/> object.
        /// </summary>
        /// <param name="_methodInfo">
        /// The <see cref="FieldInfo"/> object used to construct this proxy
        /// </param>
        public MethodProxy( MethodInfo _methodInfo )
        {
            MethodInfo = _methodInfo;
            m_invoker = CreateInvoker( _methodInfo );
        }

        /// <summary>
        /// Invoke the method via this proxy on an instance object with the supplied parameters.
        /// </summary>
        /// <param name="_instanceObject">
        /// The instance object (the "this") that the method will be invoked on.
        /// </param>
        /// <param name="_params">
        /// The parameters that are to be passed to the method, as an array
        /// </param>
        /// <returns>The value that was returned from the method.</returns>
        public object Invoke( object _instanceObject, params object[] _params ) => m_invoker( _instanceObject, _params );

        /// <summary>
        /// Given a <see cref="MethodInfo"/> object, create a generalized Invoker that can
        /// provide faster Invocation than Reflection will.
        /// </summary>
        /// <param name="_methodInfo">
        /// The <see cref="MethodInfo"/> object representing the method to invoke.
        /// </param>
        /// <returns>
        /// A generalized delegate representing the proxy to the supplied
        /// <see cref="MethodInfo"/>
        /// </returns>
        /// <remarks>
        /// Using the return value from this static function directly provides a more performant
        /// invocation of the method.
        /// </remarks>
        public static _Delegate CreateInvoker( MethodInfo _methodInfo )
        {
            if (_methodInfo == null) throw new ArgumentNullException();

            var exThisParam = Expression.Parameter( typeof( object ), "_this" );
            var exParams = Expression.Parameter( typeof( object[] ), "_params" );

            var exConvertedThis = Expression.Convert( exThisParam, _methodInfo.DeclaringType );
            var exConvertedParams = _methodInfo
                .GetParameters()
                .Select( ( _p, _idx ) =>
                    Expression.Convert(
                        Expression.ArrayIndex( exParams, Expression.Constant( _idx ) ),
                        _p.ParameterType )
                    );

            var exCall = Expression.Call( exConvertedThis, _methodInfo, exConvertedParams.ToArray() );

            Expression body;
            if (_methodInfo.ReturnType == typeof( void ))
            {
                body = Expression.Block
                (
                    exCall,
                    Expression.Constant( null )
                );
            }
            else
            {
                body = Expression.Convert( exCall, typeof( object ) );
            }

            var exLambda = Expression.Lambda<_Delegate>( body, exThisParam, exParams );
            return exLambda.Compile();
        }
    }
}
