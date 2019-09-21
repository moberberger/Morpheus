using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morpheus.EventDispatcher
{
    /// <summary>
    /// An event handler specified by a Lambda function. This version works to filter events by event type. The handler accepts zero parameters.
    /// </summary>
    internal class Handler_LambdaNoParams<TEvent> : MessageHandler
    {
        private readonly Action m_handler;

        internal Handler_LambdaNoParams( Dispatcher _dispatcher, EDispatchMode _dispatchMode, Action _handler )
            : base( typeof( TEvent ), _dispatcher, _dispatchMode )
        {
            m_handler = _handler;
        }

        public override void HandleEvent( object _event ) => m_handler();
    }

    /// <summary>
    /// An event handler specified by a Lambda function. This version works to filter events by specific value. The handler accepts zero parameters.
    /// </summary>
    internal class CEventHandler_LambdaNoParams : MessageHandler
    {
        private readonly Action m_handler;

        internal CEventHandler_LambdaNoParams( Dispatcher _dispatcher, EDispatchMode _dispatchMode, Action _handler, object _specificValue )
            : base( _specificValue.GetType(), _dispatcher, _dispatchMode )
        {
            m_handler = _handler;
            SpecificValueFilter = _specificValue;
        }

        public override void HandleEvent( object _event )
        {
            MorpheusLog.Logger?.Debug( $"No-Parameter Handler: {m_handler.Method.Name}( unused ==> {_event.GetType().Name} )" );
            m_handler();
        }
    }
}
