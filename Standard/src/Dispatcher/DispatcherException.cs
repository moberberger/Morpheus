using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morpheus
{
    /// <summary>
    /// An exception was thrown by an event handler. Post that exception as an event for possible application handling
    /// </summary>
    public class DispatcherException
    {
        /// <summary>
        /// The Event object that was being handled
        /// </summary>
        public object Event { get; internal set; }

        /// <summary>
        /// The handler instance that was handing the event
        /// </summary>
        public MessageHandler Handler { get; internal set; }

        /// <summary>
        /// The exception that was thrown by the handler
        /// </summary>
        public Exception Exception { get; internal set; }

        /// <summary>
        /// Try to provide a description of the event.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            try
            {
                var str = new StringBuilder( "CEventException:" ).AppendLine();

                try
                {
                    str.Append( "Event Type: " )
                        //.AppendLine( (Event == null) ? "NULL" : (Event is CEventException) ? "CEventException" : Event.ToString() );
                        .AppendLine( (Event == null) ? "NULL" : Event.ToString() );
                }
                catch
                {
                    str.AppendLine( "Event.ToString() threw an exception" );
                }

                try
                {
                    str.Append( "   Handler: " )
                        .AppendLine( (Handler == null || Handler.Description == null) ? "NULL" : Handler.Description )
                        .AppendLine();
                }
                catch
                {
                    str.AppendLine( "Handler.ToString() threw an exception." );
                }

                try
                {
                    str.AppendLine( (Exception == null) ? "NULL EXCEPTION" : Exception.ToString() );
                }
                catch
                {
                    str.AppendLine( "Exception.ToString() threw an exception." );
                }

                return str.ToString();
            }
            catch
            {
                try
                {
                    return Exception.ToString();
                }
                catch
                {
                    return "CEventException Can't even turn itself into a string without throwing an exception.";
                }
            }
        }
    }
}
