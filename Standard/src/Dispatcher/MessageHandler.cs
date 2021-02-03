using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morpheus
{
    /// <summary>
    /// This class encapsulates information about an event handler. These handlers are specified by the application.
    /// Typically, these will be either a lambda/delegate or a method discovered through attributes and signature convention.
    /// This class also takes care of statistics (performance counters) for event handling.
    /// Each instance of this class corresponds to exactly one method-event pair. There is a one-to-many relationship between
    /// the handler and the event, in that an event may be associated with zero or more handlers, but a handler will always be
    /// associated with exactly one event.
    /// </summary>
    public abstract class MessageHandler
    {
        /// <summary>
        /// The <see cref="System.Type"/> of the event to handle.
        /// </summary>
        public Type EventType { get; protected set; }

        /// <summary>
        /// The <see cref="Dispatcher"/> that this handler is associated with
        /// </summary>
        public Dispatcher Dispatcher { get; protected set; }

        /// <summary>
        /// The DispatchMode for this handler. May be EDispatchMode.NotAssigned.
        /// </summary>
        public EDispatchMode DispatchMode { get; protected set; }

        /// <summary>
        /// Only call the handler when the event matches this specific value. If NULL, do not use specific value checking.
        /// </summary>
        public object SpecificValueFilter { get; protected set; }

        /// <summary>
        /// Can be set by the application to describe the handler- Can be any string value.
        /// </summary>
        public string Description;

        /// <summary>
        /// Construct the handler information
        /// </summary>
        /// <param name="_eventType">The Type to handle</param>
        /// <param name="_dispatcher">The Dispatcher that created this handler</param>
        /// <param name="_dispatchMode">The DispatchMode for this event- This value, if set, will override all other mode settings.</param>
        protected MessageHandler( Type _eventType, Dispatcher _dispatcher, EDispatchMode _dispatchMode )
        {
            EventType = _eventType;
            Dispatcher = _dispatcher;
            DispatchMode = _dispatchMode;
        }

        /// <summary>
        /// Handle the event. This method should never be called except from within <see cref="MessageHandlingInstance"/>
        /// </summary>
        /// <param name="_event">The event to handle</param>
        public abstract void HandleEvent( object _event );
    }
}
