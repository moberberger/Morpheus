using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morpheus.EventDispatcher
{
    /// <summary>
    /// Handle the case where the lambda event handler receives a CEventHandler only
    /// </summary>
    internal class Handler_LambdaHandlerOnly : MessageHandler
    {
        private readonly Action<MessageHandler> m_handler;

        internal Handler_LambdaHandlerOnly( Dispatcher _dispatcher, EDispatchMode _dispatchMode, Action<MessageHandler> _handler, object _specificValue )
            : base( null, _dispatcher, _dispatchMode )
        {
            EventType = _specificValue.GetType();
            SpecificValueFilter = _specificValue;
            m_handler = _handler;
        }

        public override void HandleEvent( object _event )
        {
            MorpheusLog.Logger?.Debug( $"Handling Handler-Only: {m_handler.Method.Name}( unused ==> {_event.GetType().Name} )" );
            m_handler( this );
        }
    }
}
