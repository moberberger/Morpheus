using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morpheus.EventDispatcher
{
    /// <summary>
    /// An event handler specified by a Lambda function. The handler must accept only the Event as the first parameter
    /// </summary>
    internal class Handler_LambdaEventOnly<TEvent> : MessageHandler
    {
        private readonly Action<TEvent> m_handler;

        internal Handler_LambdaEventOnly( Dispatcher _dispatcher, EDispatchMode _dispatchMode, Action<TEvent> _handler )
            : base( typeof( TEvent ), _dispatcher, _dispatchMode )
        {
            m_handler = _handler;
        }

        public override void HandleEvent( object _event )
        {
            MorpheusLog.Logger?.Debug( $"Handling Event Only: {m_handler.Method.Name}( {_event.GetType().Name} )" );
            m_handler( (TEvent) _event );
        }
    }
}
