namespace Morpheus
{
    /// <summary>
    /// Use to describe the severity of each log level
    /// </summary>
    public enum ELogLevel
    {
        /// <summary>
        /// Not zero, but explicitly Unset
        /// </summary>
        Unset = 1,

        /// <summary>
        /// Has been set, but to nothing recognizable
        /// </summary>
        Nothing,

        /// <summary>
        /// Debug is the most verbose level of logging and may almost certainly affect
        /// performance.
        /// </summary>
        Debug,

        /// <summary>
        /// Full logging. The application should take care to assure that anything logged to
        /// Info will not affect performance on target systems.
        /// </summary>
        Info,

        /// <summary>
        /// Warning implies that something went wrong, but Morpheus was able to deal with it
        /// properly. The fact that it happened should generally mean the message is worthy of
        /// forensic analysis.
        /// </summary>
        Warn,

        /// <summary>
        /// An Error Occurred. The program may or may not terminate. An exception may or may not
        /// have been thrown. If the program does not terminate, at least one function Morpheus
        /// was supposed to perform didn't get performed.
        /// </summary>
        Error
    }
}
