using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morpheus.EventDispatcher
{
    /// <summary>
    /// An event handler specified by a Lambda function. The handler must accept a
    /// <see cref="Dispatcher"/> as the second parameter
    /// </summary>
    internal class Handler_LambdaEventDispatcher<TEvent, TDispatcher> : MessageHandler
        where TDispatcher : Dispatcher
    {
        private readonly Action<TEvent, TDispatcher> m_handler;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_dispatcher"></param>
        /// <param name="_dispatchMode"></param>
        /// <param name="_handler"></param>
        internal Handler_LambdaEventDispatcher( Dispatcher _dispatcher, EDispatchMode _dispatchMode, Action<TEvent, TDispatcher> _handler )
            : base( typeof( TEvent ), _dispatcher, _dispatchMode )
        {
            if (!typeof( TDispatcher ).IsAssignableFrom( _dispatcher.GetType() ))
            {
                throw new ArgumentException(
                    string.Format( "The Dispatcher specified ({0}) must be assignable from the Dispatcher passed in as the first parameter ({1}).",
                        typeof( TDispatcher ).FullName,
                        _dispatcher.GetType().FullName )
                    );
            }

            m_handler = _handler;
        }

        public override void HandleEvent( object _event )
        {
            MorpheusLog.Logger?.Debug( $"Handling Dispatcher+Event: {m_handler.Method.Name}( {_event.GetType().Name} )" );
            m_handler( (TEvent) _event, (TDispatcher) Dispatcher );
        }
    }
}
