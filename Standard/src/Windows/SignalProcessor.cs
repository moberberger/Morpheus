using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Morpheus
{
    /// <summary>
    /// Handle process signals on a console application.
    /// </summary>
    /// <remarks>
    /// This static class contains methods and events that allow a console application to handle process
    /// signals, like CTRL-C. It does so by exposing a series of static events that applications can 
    /// attach "signal handlers" to. 
    /// <p>
    /// If a signal doesn't have a handler attached to it, the signal will be passed back to the OS. If 
    /// a signal does have a handler, it will NOT be passed back to the OS. For instance, if the application 
    /// registers an <see cref="OnBreak"/> handler, but no <see cref="OnControlC"/> handler, any "break"
    /// events will be handled by the registered handler, but the "ControlC"'s will cause the process to
    /// abort.
    /// </p>
    /// <p>
    /// If the <see cref="OnAnySignal"/> event has registered handlers, then ALL signals will be considered
    /// "handled" and the OS will not process any of them.
    /// </p>
    /// <p>
    /// The <see cref="IgnoreControlC"/> (with corresponding <see cref="ProcessControlC"/>) methods are 
    /// convenient ways to simply ignore the signal generated when the user presses CRTL-C while the 
    /// application is running. No signals are actually generated for <see cref="OnControlC"/> when
    /// the <b>IgnoreControlC</b> method has been called.
    /// </p>
    /// </remarks>
    public static class SignalProcessor
    {
        /// <summary>
        /// This enum is tied directly to the values required by the Win32 call
        /// </summary>
        public enum ESignalType
        {
            /// <summary>
            /// Control-C was pressed
            /// </summary>
            ControlC = 0,
            /// <summary>
            /// "Break" (Control-Pause) was pressed
            /// </summary>
            Break,
            /// <summary>
            /// The window was closed.
            /// </summary>
            Close,
            /// <summary>
            /// The user triggered a log-off
            /// </summary>
            Logoff = 5,
            /// <summary>
            /// Windows is shutting down
            /// </summary>
            Shutdown
        }

        /// <summary>
        /// Simple handler for specific signal processing
        /// </summary>
        public delegate void DSignalHandler();

        /// <summary>
        /// Handler that will multiplex a signal (based on ESignalTypes) however the application wishes.
        /// </summary>
        public delegate bool DSignalMultiplexer( ESignalType signalType );

        /// <summary>
        /// Invoked when Control-C is pressed
        /// </summary>
        public static event DSignalHandler OnControlC;

        /// <summary>
        /// Invoked when "Break" is pressed
        /// </summary>
        public static event DSignalHandler OnBreak;

        /// <summary>
        /// Invoked when the user tries to close the console window
        /// </summary>
        public static event DSignalHandler OnClose;

        /// <summary>
        /// Invoked when the current user logs off
        /// </summary>
        public static event DSignalHandler OnLogoff;

        /// <summary>
        /// Invoked when the system tries to shut down
        /// </summary>
        public static event DSignalHandler OnShutdown;

        /// <summary>
        /// Invoked when any signal is raised.
        /// </summary>
        /// <remarks>This event gets signalled AFTER all other more specific signals get raised. The delegate 
        /// receives information as a parameter detailing what signal raised the event. If this event has delegates
        /// registered with it, all signals will be considered "handled" and never passed through to the OS.
        /// </remarks>
        public static event DSignalMultiplexer OnAnySignal;

        /// <summary>
        /// Declaration of the Win32 function that establishes signal handlers
        /// </summary>
        /// <param name="p_handler">The handler routine</param>
        /// <param name="p_addHandler">TRUE to add the handler, FALSE to remove it.</param>
        /// <returns>N/A</returns>
        [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
        [DllImport( "Kernel32" )]
        private static extern Boolean SetConsoleCtrlHandler( DSignalMultiplexer p_handler, Boolean p_addHandler );

        /// <summary>
        /// Keep a static reference to the delegate used as a handler to the Win32 call. Must keep a reference
        /// to this delegate around, or the GC will collect it and the Win32 handler will fail.
        /// </summary>
        private static DSignalMultiplexer sm_internalDispatcher;

        /// <summary>
        /// Static constructor merely sets up the hander with the Win32 call.
        /// </summary>
        static SignalProcessor()
        {
            sm_internalDispatcher = new DSignalMultiplexer( DispatchSignal );
            SetConsoleCtrlHandler( sm_internalDispatcher, true );
        }

        /// <summary>
        /// The internal signal dispatcher will route the signal to the appropriate static event.
        /// </summary>
        /// <param name="p_signalType">The signal type that was raised</param>
        private static bool DispatchSignal( ESignalType p_signalType )
        {
            bool retval = true;

            switch (p_signalType)
            {
            case ESignalType.ControlC:
                if (OnControlC != null)
                    OnControlC();
                else
                    retval = false;
                break;

            case ESignalType.Break:
                if (OnBreak != null)
                    OnBreak();
                else
                    retval = false;
                break;

            case ESignalType.Close:
                if (OnClose != null)
                    OnClose();
                else
                    retval = false;
                break;

            case ESignalType.Logoff:
                if (OnLogoff != null)
                    OnLogoff();
                else
                    retval = false;
                break;

            case ESignalType.Shutdown:
                if (OnShutdown != null)
                    OnShutdown();
                else
                    retval = false;
                break;
            }

            if (OnAnySignal != null)
            {
                OnAnySignal( p_signalType );
                retval = true;
            }

            return retval;
        }


        /// <summary>
        /// The process will ignore control-C processing regardless of registered signal handlers for CTRL-C
        /// </summary>
        public static void IgnoreControlC()
        {
            SetConsoleCtrlHandler( null, true );
        }

        /// <summary>
        /// The process will abort when control-C is pressed and a corresponding signal handler is not present.
        /// </summary>
        public static void ProcessControlC()
        {
            SetConsoleCtrlHandler( null, false );
        }
    }
}