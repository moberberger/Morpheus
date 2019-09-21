using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morpheus.EventDispatcher
{
    /// <summary>
    /// An event handler specified by a Lambda function. The handler must accept a <see cref="MessageHandler"/>
    /// as the second parameter
    /// </summary>
    internal class Handler_LambdaEventHandler<TEvent> : MessageHandler
    {
        private readonly Action<TEvent, MessageHandler> m_handler;

        internal Handler_LambdaEventHandler( Dispatcher _dispatcher, EDispatchMode _dispatchMode, Action<TEvent, MessageHandler> _handler )
            : base( typeof( TEvent ), _dispatcher, _dispatchMode )
        {
            m_handler = _handler;
        }

        public override void HandleEvent( object _event )
        {
            MorpheusLog.Logger?.Debug( $"Handling Event+Handler: {m_handler.Method.Name}( {_event.GetType().Name} )" );
            m_handler( (TEvent) _event, this );
        }
    }
}
