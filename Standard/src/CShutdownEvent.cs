using System.Threading;

namespace Morpheus
{
    /// <summary>
    /// This class describes a generic "shutdown event".
    /// </summary>
    /// <remarks>
    /// It uses a standard <see cref="ManualResetEvent"/> along with a <see cref="bool"/> value to provide "shutdown" information and functionality
    /// </remarks>
    public class CShutdownEvent
    {
        private readonly ManualResetEvent m_shutdownEvent = new ManualResetEvent( false );

        /// <summary>
        /// Signal the application to shut down.
        /// </summary>
        public void Shutdown()
        {
            m_shutdownEvent.Set();
            IsShutdown = true;
        }

        /// <summary>
        /// Check to see if the application has been shut down.
        /// </summary>
        public bool IsShutdown { get; private set; } = false;

        /// <summary>
        /// Block the thread, waiting on the shutdown to occur.
        /// </summary>
        /// <returns>
        /// TRUE if there was a SHUTDOWN, FALSE if there was a TIMEOUT
        /// </returns>
        public bool WaitForShutdown( int _msTimeout = -1 ) => m_shutdownEvent.WaitOne( _msTimeout );

        /// <summary>
        /// Allow an object of this type to be passed into a "WaitOne" or "WaitAny" call.
        /// </summary>
        /// <param name="_object">The CShutdownEvent object</param>
        /// <returns>The "WaitHandle" part of the ManualResetEvent field of the object</returns>
        public static implicit operator WaitHandle( CShutdownEvent _object ) => _object.m_shutdownEvent;

        /// <summary>
        /// Allow an object of this type to be passed into a "WaitOne" or "WaitAny" call.
        /// </summary>
        /// <param name="_object">The CShutdownEvent object</param>
        /// <returns>The "WaitHandle" part of the ManualResetEvent field of the object</returns>
        public static implicit operator ManualResetEvent( CShutdownEvent _object ) => _object.m_shutdownEvent;
    }
}