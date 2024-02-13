#nullable disable

namespace Morpheus;


/// <summary>
/// Class for handling all Morpheus logging.
/// 
/// <para> This is in no way meant to be a replacement for a logging framework, such as
/// Log4Net. </para>
/// 
/// <para> By writing an <see cref="ILogSink"/> implementation, the application can
/// re-direct Morpheus logs to anywhere, including both <see cref="Console.Out"/> or just to
/// the bit bucket. </para>
/// 
/// <para> The application is NOT MEANT TO LOG TO THIS! Again, this is for Morpheus logging
/// to get into the application, not the other way. </para>
/// </summary>
public static class MorpheusLog
{
    /// <summary>
    /// The logger that will handle all output requests
    /// </summary>
    public static LogFunction Logger { get; private set; } = new LogFunction( new NullLogSink(), ELogLevel.Error );

    /// <summary>
    /// Remove the logger. Allows null-conditional operator to bypass all logging string
    /// formatting.
    /// </summary>
    public static void Delete() => Logger = null;

    /// <summary>
    /// Create a new logger with a specified Sink and a specified Minimum Log Level. Both
    /// may be null.
    /// </summary>
    /// <param name="_sink"></param>
    /// <param name="_level"></param>
    public static void Create( ILogSink _sink = null, ELogLevel _level = ELogLevel.Debug ) => Logger = new LogFunction( _sink, _level );

    /// <summary>
    /// Shortcut to use the Console to output data
    /// </summary>
    /// <param name="_level"></param>
    public static void UseConsole( ELogLevel _level = ELogLevel.Debug ) => Create( new ConsoleLogSink(), _level );

    /// <summary>
    /// Long-hand way of avoiding a null-conditional.
    /// </summary>
    public static bool Enabled => Logger != null;

    /// <summary>
    /// This class will output a string to a ILogSink provided by the application. Sinks are
    /// provided for NULL and Console output. Levels are provided to allow the application
    /// to cull low-level messaging.
    /// </summary>
    public class LogFunction
    {
        /// <summary>
        /// Create a new LogFunction with
        /// </summary>
        /// <param name="_sink"></param>
        /// <param name="_level"></param>
        public LogFunction( ILogSink _sink = null, ELogLevel _level = ELogLevel.Debug )
        {
            Sink = _sink;
            Level = _level;
        }

        /// <summary>
        /// The lowest level logs to actually send to the sink. When UnSet, everything will
        /// always be logged.
        /// </summary>
        public ELogLevel Level { get; set; } = ELogLevel.Debug;

        /// <summary>
        /// May be set by the application to provide a destination for logs. In not set,
        /// then logging will not happen.
        /// </summary>
        public ILogSink Sink
        {
            get => m_sink;
            set => m_sink = value ?? new NullLogSink();
        }
        private ILogSink m_sink = new NullLogSink();

        /// <summary>
        /// Internal helper to do the dispatching of messages
        /// </summary>
        /// <param name="_minLevel"></param>
        /// <param name="_outFunc"></param>
        /// <param name="_message"></param>
        /// <returns>The string that was output</returns>
        private string Output( ELogLevel _minLevel, Func<string, string> _outFunc, string _message )
        {
            if ((int) Level > (int) _minLevel) return null;
            return _outFunc( _message );
        }

        /// <summary>
        /// Send message to the <see cref="Sink"/> 's method Debug
        /// </summary>
        /// <param name="_message">
        /// The message to send. Various sinks may add to this message.
        /// </param>
        /// <returns>The actual message that was logged by the sink.</returns>
        public string Debug( string _message ) => Output( ELogLevel.Debug, Sink.Debug, _message );

        /// <summary>
        /// Send message to the <see cref="Sink"/> 's method Info
        /// </summary>
        /// <param name="_message">
        /// The message to send. Various sinks may add to this message.
        /// </param>
        /// <returns>The actual message that was logged by the sink.</returns>
        public string Info( string _message ) => Output( ELogLevel.Info, Sink.Info, _message );

        /// <summary>
        /// Send message to the <see cref="Sink"/> 's method Warn
        /// </summary>
        /// <param name="_message">
        /// The message to send. Various sinks may add to this message.
        /// </param>
        /// <returns>The actual message that was logged by the sink.</returns>
        public string Warn( string _message ) => Output( ELogLevel.Warn, Sink.Warn, _message );

        /// <summary>
        /// Send message to the <see cref="Sink"/> 's method Error
        /// </summary>
        /// <param name="_message">
        /// The message to send. Various sinks may add to this message.
        /// </param>
        /// <returns>The actual message that was logged by the sink.</returns>
        public string Error( string _message ) => Output( ELogLevel.Error, Sink.Error, _message );
    }
}
