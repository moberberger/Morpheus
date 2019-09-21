using System;

namespace Morpheus
{
    /// <summary>
    /// Because Console is part of .NET, this helper has been implemented to output Morpheus log
    /// info to the console
    /// </summary>
    public class ConsoleLogSink : ILogSink
    {
        private string ConsoleOut( string _message, string _level )
        {
            Console.WriteLine( $"{DateTime.Now.ToLongTimeString()} {_level}: {_message}" );
            return _message;
        }

        /// <summary>
        /// Log the most detailed message.
        /// </summary>
        /// <param name="_message">The message to log.</param>
        /// <returns>
        /// The message, as it was actually logged. If multiple log sinks are present, then this
        /// value may represent any of the configured logger's return value.
        /// </returns>
        public string Debug( string _message ) => ConsoleOut( _message, "DEBUG" );

        /// <summary>
        /// Log an informational message.
        /// </summary>
        /// <param name="_message">The message to log.</param>
        /// <returns>
        /// The message, as it was actually logged. If multiple log sinks are present, then this
        /// value may represent any of the configured logger's return value.
        /// </returns>
        public string Info( string _message ) => ConsoleOut( _message, " INFO" );

        /// <summary>
        /// Log a warning message.
        /// </summary>
        /// <param name="_message">The message to log.</param>
        /// <returns>
        /// The message, as it was actually logged. If multiple log sinks are present, then this
        /// value may represent any of the configured logger's return value.
        /// </returns>
        public string Warn( string _message ) => ConsoleOut( _message, " WARN" );

        /// <summary>
        /// Log an error message.
        /// </summary>
        /// <param name="_message">The message to log.</param>
        /// <returns>
        /// The message, as it was actually logged. If multiple log sinks are present, then this
        /// value may represent any of the configured logger's return value.
        /// </returns>
        public string Error( string _message ) => ConsoleOut( _message, "ERROR" );
    }
}
