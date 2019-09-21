namespace Morpheus
{
    /// <summary>
    /// Implement by application to receive log messages from Morpheus
    /// </summary>
    public interface ILogSink
    {
        /// <summary>
        /// Log the most detailed message. This level of logging may affect the performance of
        /// the application.
        /// </summary>
        /// <param name="_message">The message to log.</param>
        /// <returns>
        /// The message, as it was actually logged. If multiple log sinks are present, then this
        /// value may represent any of the configured logger's return value.
        /// </returns>
        string Debug( string _message );

        /// <summary>
        /// Log an informational message.
        /// </summary>
        /// <param name="_message">The message to log.</param>
        /// <returns>
        /// The message, as it was actually logged. If multiple log sinks are present, then this
        /// value may represent any of the configured logger's return value.
        /// </returns>
        string Info( string _message );

        /// <summary>
        /// Log a warning message.
        /// </summary>
        /// <param name="_message">The message to log.</param>
        /// <returns>
        /// The message, as it was actually logged. If multiple log sinks are present, then this
        /// value may represent any of the configured logger's return value.
        /// </returns>
        string Warn( string _message );

        /// <summary>
        /// Log an error message.
        /// </summary>
        /// <param name="_message">The message to log.</param>
        /// <returns>
        /// The message, as it was actually logged. If multiple log sinks are present, then this
        /// value may represent any of the configured logger's return value.
        /// </returns>
        string Error( string _message );
    }
}
