using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morpheus
{
    /// <summary>
    /// This class should contain all information about the execution of one event handler
    /// for one event. It handles all statistics (performance counters), exception handling,
    /// etc.
    /// </summary>
    public class MessageHandlingInstance
    {
        /// <summary>
        /// The time that the event execution instance was created / queued
        /// </summary>
        public DateTime TimeQueued { get; private set; }

        /// <summary>
        /// The time that the event execution was started
        /// </summary>
        public DateTime TimeExecutionStarted { get; private set; }

        /// <summary>
        /// The time that the event execution was completed
        /// </summary>
        public DateTime TimeExecutionEnded { get; private set; }


        /// <summary>
        /// The Handler for the event
        /// </summary>
        public readonly MessageHandler Handler;

        /// <summary>
        /// The Event to be handled
        /// </summary>
        public readonly object Event;

        /// <summary>
        /// Construct with the handler and the event
        /// </summary>
        /// <param name="_handler">The handler object to be used for this event</param>
        /// <param name="_event">The event to be handled</param>
        public MessageHandlingInstance( MessageHandler _handler, object _event )
        {
            Handler = _handler;
            Event = _event;
            TimeQueued = DateTime.Now;
        }


        /// <summary>
        /// This is the mechanism by which all events should be handled, as it takes care of
        /// accounting and exception handling. The "CEventHandler.HandleEvent" method should
        /// never be called except by this routine.
        /// 
        /// If this routine is overridden in a derived class, it is strongly suggested that
        /// it should call base.HandleEvent() to gain the functionality from this method.
        /// </summary>
        internal virtual void HandleEvent()
        {
            TimeExecutionStarted = DateTime.Now;

            try
            {
                Handler.HandleEvent( Event );
            }
            catch (Exception e)
            {
                if (Event is DispatcherException) // This was already an event exception
                {
                    // Its time to let it die. Event handlers aren't supposed to be critical
                    // enough to warrant interrupting the application.
                    return;
                }

                var eventException = new DispatcherException()
                {
                    Event = Event,
                    Handler = Handler,
                    Exception = e
                };

                // Post the exception event. Allows the application to register to handle
                // CEventExceptions. If the application doesn't care, neither does the Event
                // Dispatcher class.
                Handler.Dispatcher.Post( eventException );

            }
            finally
            {
                TimeExecutionEnded = DateTime.Now;
                Handler.Dispatcher.IncrementEventCount();
            }
        }
    }
}
