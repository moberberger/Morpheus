namespace Morpheus
{
    /// <summary>
    /// Ignores all log output.
    /// </summary>
    public class NullLogSink : ILogSink
    {
        /// <summary>
        /// Log the most detailed message.
        /// </summary>
        /// <param name="_message">The message to log.</param>
        /// <returns>
        /// The message, as it was actually logged. If multiple log sinks are present, then this
        /// value may represent any of the configured logger's return value.
        /// </returns>
        public string Debug( string _message ) => "";

        /// <summary>
        /// Log an informational message.
        /// </summary>
        /// <param name="_message">The message to log.</param>
        /// <returns>
        /// The message, as it was actually logged. If multiple log sinks are present, then this
        /// value may represent any of the configured logger's return value.
        /// </returns>
        public string Info( string _message ) => "";

        /// <summary>
        /// Log a warning message.
        /// </summary>
        /// <param name="_message">The message to log.</param>
        /// <returns>
        /// The message, as it was actually logged. If multiple log sinks are present, then this
        /// value may represent any of the configured logger's return value.
        /// </returns>
        public string Warn( string _message ) => "";

        /// <summary>
        /// Log an error message.
        /// </summary>
        /// <param name="_message">The message to log.</param>
        /// <returns>
        /// The message, as it was actually logged. If multiple log sinks are present, then this
        /// value may represent any of the configured logger's return value.
        /// </returns>
        public string Error( string _message ) => "";
    }
}
