using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morpheus.EventDispatcher
{
    /// <summary>
    /// A specific-value handler that receives a type-correct <see cref="Dispatcher"/>
    /// parameter. The Type of the event is inferred from the _specficValue parameter.
    /// </summary>
    /// <remarks>
    /// If the handler needs to receive the actual event (because maybe its assigned to
    /// different values), it needs to use <see cref="Handler_LambdaHandlerOnly"/> .
    /// The <see cref="MessageHandler"/> object sent to that handler will contain the value
    /// that triggered the handler's execution.
    /// </remarks>
    /// <typeparam name="TDispatcher">
    /// The specific Type of the dispatcher to send as a parameter.
    /// </typeparam>
    internal class Handler_LambdaDispatcherOnly<TDispatcher> : MessageHandler
        where TDispatcher : Dispatcher
    {
        private readonly Action<TDispatcher> m_handler;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_dispatcher"></param>
        /// <param name="_dispatchMode"></param>
        /// <param name="_handler"></param>
        /// <param name="_specificValue"></param>
        internal Handler_LambdaDispatcherOnly( Dispatcher _dispatcher, EDispatchMode _dispatchMode, Action<TDispatcher> _handler, object _specificValue )
            : base( null, _dispatcher, _dispatchMode )
        {
            if (!typeof( TDispatcher ).IsAssignableFrom( _dispatcher.GetType() ))
            {
                throw new ArgumentException(
                    string.Format( "The Dispatcher specified ({0}) must be assignable from the Dispatcher passed in as the first parameter ({1}).",
                        typeof( TDispatcher ).FullName,
                        _dispatcher.GetType().FullName )
                    );
            }

            EventType = _specificValue.GetType();
            SpecificValueFilter = _specificValue;
            m_handler = _handler;
        }

        public override void HandleEvent( object _event )
        {
            MorpheusLog.Logger?.Debug( $"Handling Dispatcher-only handler: {m_handler.Method.Name}" );
            m_handler( (TDispatcher) Dispatcher );
        }
    }
}
