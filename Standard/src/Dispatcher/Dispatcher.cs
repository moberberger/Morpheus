using Morpheus.EventDispatcher;

namespace Morpheus;


/// <summary>
/// This class will handle the dispatch of "events" to registered handlers.
/// "Events" are merely objects of any type.
/// 
/// There is NO guarantee of execution of events. A power-lost state,
/// premature disposal of this object, or any number of other edge cases can
/// cause events not to get executed. However, this class will never
/// knowingly and/or purposefully discard an event.
/// </summary>
/// <remarks>
/// Features: * Handler Execution Context- Application may select inline,
/// threadpool or batched execution of event handlers. * Handler Discovery-
/// The dispatcher class can use reflection to search the loaded assemblies
/// for event handlers, using a combination of attributes and method
/// signatures to find message handlers. * Mix-and-Match- This class can
/// handle any combination of the above mentioned features
/// 
/// The handler registration process has the option (not obligation) to
/// specifiy which dispatch mode (inline, batch, threadpool) to use. This
/// election (at registration time) has the highest priority. When not set
/// explicitly at registration time, the parameter specified in the Post
/// method call will be used. If no mode is explicitly set in the Post
/// method call parameters, then the mode specified by the
/// <see cref="DefaultDispatchMode"/> property of the Dispatcher will be
/// used.
/// 
/// In most if not all cases, long running event handlers should be
/// registered for the ThreadPool dispatch mode. This makes sure that the
/// "main thread" (game thread, apartment thread, etc) is not affected
/// significantly. However, this may not be possible if the event handler
/// must share the same thread as a UI thread, as is the case with WinForms,
/// Unity, etc. In these cases, batching may help, but can still delay
/// program execution for poorly written or long-running event handlers.
/// 
/// It is perfectly acceptable to register an event handler that must
/// perform a round-trip to an internet server on a threadpool thread. Async
/// completion of that Internet-based IO is still preferred (there are only
/// a set number of possible simultaneously executing threadpool threads).
/// Another option for threadpool registration is an event that must perform
/// disk IO.
/// 
/// Exception Handling: The Dispatcher operates under the assumption that
/// event handling is not critical, and should not be depended on for
/// altering control flow of the application. To this effect, exceptions
/// thrown within an event handler have a single chance to be "handled" by
/// the application. By registering a Handler for the CEventException event,
/// the application can look at the Event, the Handler and the Exception to
/// try to recover and/or fix the issue. However, any further exception
/// thrown within the handler for the CEventException will be dropped
/// unceremoniously. In other words- Exceptions in Event Handlers will be
/// Posted as a CEventException UNLESS the Event itself was already a
/// CEventException, in which case its lost.
/// </remarks>
public class Dispatcher
{
    /// <summary>
    /// The sync object for the dispatcher
    /// </summary>
    public object SyncObject { get; } = new object();

    /// <summary>
    /// The main database of how events are to be dispatched
    /// </summary>
    private readonly Dictionary<Type, List<MessageHandler>> m_handlersForTypes = new Dictionary<Type, List<MessageHandler>>();

    /// <summary>
    /// When nothing has been explicitly specified, use this dispatch mode
    /// for all event dispatching.
    /// </summary>
    public EDispatchMode DefaultDispatchMode
    {
        get => m_defaultDispatchMode;
        set
        {
            if (value == EDispatchMode.NotAssigned)
                throw new InvalidOperationException( "Not allowed to set the Default Dispatch Mode to 'NotAssigned')" );
            m_defaultDispatchMode = value;
        }
    }
    private EDispatchMode m_defaultDispatchMode = EDispatchMode.Inline;

    /// <summary>
    /// A queue of event+handler objects that is to be executed by the next
    /// Batch Execution operation
    /// </summary>
    private readonly Queue<MessageHandlingInstance> m_batch = new Queue<MessageHandlingInstance>();

    /// <summary>
    /// A queue for inline handler executions. This is a field (instead of
    /// using automatic storage) because there's no reason to GC this after
    /// every Post operation.
    /// </summary>
    private readonly Queue<MessageHandlingInstance> m_inlineQueue = new Queue<MessageHandlingInstance>();

    /// <summary>
    /// The number of event+handler entries that are outstanding in the
    /// ThreadPool
    /// </summary>
    private int m_outstandingThreadpoolItems = 0;

    /// <summary>
    /// The number of events that have been executed by the dispatcher in
    /// any of the modes
    /// </summary>
    public int EventExecutionCount => m_eventExecutionCount;
    private int m_eventExecutionCount = 0;

    /// <summary>
    /// Set the EventExecutionCount to zero
    /// </summary>
    /// <returns></returns>
    public int ResetExecutionCount() => Interlocked.Exchange( ref m_eventExecutionCount, 0 );

    /// <summary>
    /// Internal method to update the number of events executed. Should
    /// really only be called by the CEventHandler class.
    /// </summary>
    internal void IncrementEventCount() => Interlocked.Increment( ref m_eventExecutionCount );

    /// <summary>
    /// Private method used by threadpool execution to signal that an item
    /// has completed.
    /// </summary>
    private void IncrementThreadpoolItems() => Interlocked.Increment( ref m_outstandingThreadpoolItems );

    /// <summary>
    /// Internal method used by threadpool execution to signal that an item
    /// has completed.
    /// </summary>
    internal void DecrementThreadpoolItems() => Interlocked.Decrement( ref m_outstandingThreadpoolItems );

    /// <summary>
    /// Returns TRUE if there are outstanding events to be handled by either
    /// the threadpool or the batch.
    /// </summary>
    public bool HasPendingEvents() => m_batch.Count > 0 || m_outstandingThreadpoolItems > 0;




    /// <summary>
    /// Register a handler for a given event type. The handler will be in
    /// the form of a lambda or delegate.
    /// </summary>
    /// <typeparam name="TEvent">The Type of event to handle</typeparam>
    /// <param name="_handler">
    /// The delegate/lambda to be called with the event.
    /// </param>
    /// <param name="_dispatchMode">
    /// The DispatchMode for events of this Type. This value, if set, will
    /// override all other mode selections.
    /// </param>
    /// <returns>
    /// Returns the handler object for this event and handler
    /// </returns>
    public MessageHandler RegisterHandler<TEvent>( Action<TEvent, MessageHandler> _handler, EDispatchMode _dispatchMode = EDispatchMode.NotAssigned )
    {
        var h = new Handler_LambdaEventHandler<TEvent>( this, _dispatchMode, _handler );
        var typ = typeof( TEvent );

        AddHandlerToDatabase( typ, h );

        return h;
    }

    /// <summary>
    /// Register a handler for a given event type. The handler will be in
    /// the form of a lambda or delegate.
    /// </summary>
    /// <typeparam name="TEvent">The Type of event to handle</typeparam>
    /// <param name="_handler">
    /// The delegate/lambda to be called with the event.
    /// </param>
    /// <param name="_dispatchMode">
    /// The DispatchMode for events of this Type. This value, if set, will
    /// override all other mode selections.
    /// </param>
    /// <returns>
    /// Returns the handler object for this event and handler
    /// </returns>
    public MessageHandler RegisterHandler<TEvent>( Action<TEvent> _handler, EDispatchMode _dispatchMode = EDispatchMode.NotAssigned )
    {
        var h = new Handler_LambdaEventOnly<TEvent>( this, _dispatchMode, _handler );
        var typ = typeof( TEvent );

        AddHandlerToDatabase( typ, h );

        return h;
    }

    /// <summary>
    /// Register a handler for a given event type. The handler will be in
    /// the form of a lambda or delegate that takes no parameters.
    /// </summary>
    /// <typeparam name="TEvent">The Type of event to handle</typeparam>
    /// <param name="_handler">
    /// The handler, which takes zero parameters
    /// </param>
    /// <param name="_dispatchMode">
    /// The DispatchMode for events of this Type. This value, if set, will
    /// override all other mode selections.
    /// </param>
    /// <returns>
    /// Returns the handler object for this event and handler
    /// </returns>
    public MessageHandler RegisterHandler<TEvent>( Action _handler, EDispatchMode _dispatchMode = EDispatchMode.NotAssigned )
    {
        var h = new Handler_LambdaNoParams<TEvent>( this, _dispatchMode, _handler );
        var typ = typeof( TEvent );

        AddHandlerToDatabase( typ, h );

        return h;
    }

    /// <summary>
    /// Register a handler for a given event type. The handler will be in
    /// the form of a lambda or delegate.
    /// </summary>
    /// <typeparam name="TEvent">The Type of event to handle</typeparam>
    /// <typeparam name="TDispatcher">
    /// The exact Type of the Dispatcher
    /// </typeparam>
    /// <param name="_handler">
    /// The delegate/lambda to be called with the event.
    /// </param>
    /// <param name="_dispatchMode">
    /// The DispatchMode for events of this Type. This value, if set, will
    /// override all other mode selections.
    /// </param>
    /// <returns>
    /// Returns the handler object for this event and handler
    /// </returns>
    public MessageHandler RegisterHandler<TEvent, TDispatcher>( Action<TEvent, TDispatcher> _handler, EDispatchMode _dispatchMode = EDispatchMode.NotAssigned )
        where TDispatcher : Dispatcher
    {
        var h = new Handler_LambdaEventDispatcher<TEvent, TDispatcher>( this, _dispatchMode, _handler );
        var typ = typeof( TEvent );

        AddHandlerToDatabase( typ, h );

        return h;
    }





    /// <summary>
    /// Register a handler for an event of a specific value, as opposed to
    /// an event of a particular type.
    /// </summary>
    /// <remarks>
    /// When registering for a specific value, the dispatcher assumes that
    /// the handler knows about the value its expecting. Therefore, there's
    /// no override to pass the "event value" into the handler. If its
    /// important for the handler know the actual event value, the override
    /// to use would be the one for Action(CEventHandler), as the
    /// <see cref="MessageHandler.SpecificValueFilter"/> will contain the
    /// exact value of the event used to trigger the handler. It will also
    /// contain the <see cref="Dispatcher"/> reference, but not the
    /// convenience of a strongly typed Dispatcher.
    /// </remarks>
    /// <param name="_specificValue">The specific value to handle</param>
    /// <param name="_handler">
    /// The handler, accepting a TDispatcher (a subclass of
    /// <see cref="Dispatcher"/> ) as the only parameter
    /// </param>
    /// <param name="_dispatchMode">
    /// The DispatchMode for events of this Type. This value, if set, will
    /// override all other mode selections.
    /// </param>
    /// <returns>
    /// Returns the handler object for this event and handler
    /// </returns>
    public MessageHandler RegisterHandler( object _specificValue, Action<MessageHandler> _handler, EDispatchMode _dispatchMode = EDispatchMode.NotAssigned )
    {
        var h = new Handler_LambdaHandlerOnly( this, _dispatchMode, _handler, _specificValue );
        var typ = _specificValue.GetType();

        AddHandlerToDatabase( typ, h );

        return h;
    }


    /// <summary>
    /// Register a handler for an event of a specific value, as opposed to
    /// an event of a particular type.
    /// </summary>
    /// <remarks>
    /// When registering for a specific value, the dispatcher assumes that
    /// the handler knows about the value its expecting. Therefore, there's
    /// no override to pass the "event value" into the handler. If its
    /// important for the handler know the actual event value, the override
    /// to use would be the one for Action(CEventHandler), as the
    /// <see cref="MessageHandler.SpecificValueFilter"/> will contain the
    /// exact value of the event used to trigger the handler. It will also
    /// contain the <see cref="Dispatcher"/> reference, but not the
    /// convenience of a strongly typed Dispatcher.
    /// </remarks>
    /// <typeparam name="TDispatcher">
    /// The exact Type of the dispatcher object sent to the handler
    /// </typeparam>
    /// <param name="_specificValue">The specific value to handle</param>
    /// <param name="_handler">
    /// The handler, accepting a TDispatcher (a subclass of
    /// <see cref="Dispatcher"/> ) as the only parameter
    /// </param>
    /// <param name="_dispatchMode">
    /// The DispatchMode for events of this Type. This value, if set, will
    /// override all other mode selections.
    /// </param>
    /// <returns>
    /// Returns the handler object for this event and handler
    /// </returns>
    public MessageHandler RegisterHandler<TDispatcher>( object _specificValue, Action<TDispatcher> _handler, EDispatchMode _dispatchMode = EDispatchMode.NotAssigned )
        where TDispatcher : Dispatcher
    {
        var h = new Handler_LambdaDispatcherOnly<TDispatcher>( this, _dispatchMode, _handler, _specificValue );
        var typ = _specificValue.GetType();

        AddHandlerToDatabase( typ, h );

        return h;
    }


    /// <summary>
    /// Register a handler for an event of a specific value, as opposed to
    /// an event of a particular type.
    /// </summary>
    /// <remarks>
    /// When registering for a specific value, the dispatcher assumes that
    /// the handler knows about the value its expecting. Therefore, there's
    /// no override to pass the "event value" into the handler. If its
    /// important for the handler know the actual event value, the override
    /// to use would be the one for Action(CEventHandler), as the
    /// <see cref="MessageHandler.SpecificValueFilter"/> will contain the
    /// exact value of the event used to trigger the handler. It will also
    /// contain the <see cref="Dispatcher"/> reference, but not the
    /// convenience of a strongly typed Dispatcher.
    /// </remarks>
    /// <param name="_specificValue">The specific value to handle</param>
    /// <param name="_handler">
    /// The handler, accepting no parameters for this version of the method
    /// </param>
    /// <param name="_dispatchMode">
    /// The DispatchMode for events of this Type. This value, if set, will
    /// override all other mode selections.
    /// </param>
    /// <returns>
    /// Returns the handler object for this event and handler
    /// </returns>
    public MessageHandler RegisterHandler( object _specificValue, Action _handler, EDispatchMode _dispatchMode = EDispatchMode.NotAssigned )
    {
        var h = new CEventHandler_LambdaNoParams( this, _dispatchMode, _handler, _specificValue );
        var typ = _specificValue.GetType();

        AddHandlerToDatabase( typ, h );

        return h;
    }

















    /// <summary>
    /// Given a <see cref="MessageHandler"/> , remove it from the list of
    /// handlers.
    /// </summary>
    /// <param name="_handler">The handler to remove</param>
    /// <returns>TRUE if the handler was removed, FALSE if not</returns>
    public bool DeregisterHandler( MessageHandler _handler )
    {
        if (_handler == null)
            throw new ArgumentNullException( "_handler cannot be null" );

        lock (SyncObject)
        {
            foreach (var kv in m_handlersForTypes)
            {
                var list = kv.Value;
                for (var i = 0; i < list.Count; i++)
                {
                    if (list[i] == _handler) // we found it
                    {
                        list.RemoveAt( i ); // remove it
                        return true; // and return success
                    }
                }
            }
            return false; // All lists searched, nothing found, return false
        }
    }

    /// <summary>
    /// Given an object presumably registered using CEventHandlerDiscovery,
    /// remove all handlers associated with that object.
    /// </summary>
    /// <param name="_object">
    /// The object whose methods need to be deregistered
    /// </param>
    /// <returns>TRUE if any handlers were removed, FALSE if not</returns>
    public bool DeregisterHandlerObject( object _object )
    {
        if (_object == null)
            throw new ArgumentNullException( "_object cannot be null" );

        var retval = false;

        lock (SyncObject)
        {
            foreach (var kv in m_handlersForTypes)
            {
                var list = kv.Value;
                for (var i = 0; i < list.Count; i++)
                {
                    if (list[i] is Handler_Reflection h && h.Object == _object)
                    {
                        list.RemoveAt( i ); // remove the handler
                        i--; // Move back on the indexer because it'll be quickly incremented in the loop
                        retval = true; // At least one thing was removed, so return true
                    }
                }
            }
        }

        return retval; // All lists searched, return the result
    }


    /// <summary>
    /// Add a handler for a given type to the handler database in a
    /// threadsafe manner
    /// </summary>
    /// <param name="_type">
    /// The Type of events to hande with this handler
    /// </param>
    /// <param name="_handler">
    /// The Handler to give events of type _type to
    /// </param>
    internal void AddHandlerToDatabase( Type _type, MessageHandler _handler )
    {
        lock (SyncObject)
        {
            if (!m_handlersForTypes.TryGetValue( _type, out var list ))
            {
                list = new List<MessageHandler>();
                m_handlersForTypes[_type] = list;
            }
            list.Add( _handler );
        }
    }

    /// <summary>
    /// Post a specific event.
    /// </summary>
    /// <param name="_event">The Event to dispatch</param>
    /// <param name="_dispatchMode">
    /// The thread method which to dispatch the events. If the handler(s)
    /// have explicitly set a dispatch thread, then that will override this
    /// parameter.
    /// </param>
    public void Post( object _event, EDispatchMode _dispatchMode = EDispatchMode.NotAssigned )
    {
        var dispatchMode = _dispatchMode != EDispatchMode.NotAssigned ? _dispatchMode : DefaultDispatchMode;

        // Deconstruct the event and queue up handlers for all levels of the
        // event's object heirarchy
        lock (SyncObject) // Do not allow other Post operations, nor any Registration operations, while this is done.
        {   // No event execution will occur within this nested loop.

            // Go through the whole Type hierarchy for the event.
            for (var typ = _event.GetType(); typ != null; typ = typ.BaseType)
            {
                QueueHandlersForType( typ, _event, dispatchMode );
            }
        }

        ExecuteInlineQueue();
    }

    /// <summary>
    /// Given an Event and a System.Type, figure out how all handlers for
    /// that Type need to be queued for execution.
    /// </summary>
    /// <param name="_type">The Type to look for</param>
    /// <param name="_event">The Event to handle</param>
    /// <param name="_dispatchMode">
    /// The method used for dispatching handlers
    /// </param>
    private void QueueHandlersForType( Type _type, object _event, EDispatchMode _dispatchMode )
    {
        // Check to see if there are any handlers for the Type at this level
        // of the hierarchy
        if (m_handlersForTypes.TryGetValue( _type, out var list ))
        {
            // There is (or at least was) a handler- go through all of the
            // handlers currently registered for the Type
            for (var i = 0; i < list.Count; i++)
            {
                var handler = list[i];

                // Support for Specific Value Filtering
                if (handler.SpecificValueFilter != null && !handler.SpecificValueFilter.Equals( _event ))
                    continue; // Supposed to be specific value and it is a mismatch

                // Figure out how this handler should be executed based on
                // the EDispatchThread for it
                var dt = handler.DispatchMode;
                if (dt == EDispatchMode.NotAssigned)
                    dt = _dispatchMode;

                var eei = GetEventExecutionInstance( _event, handler );

                switch (dt) // depending on the dispatch type
                {
                    case EDispatchMode.Inline: // Put on list for execution after going through entire m_handlersForTypes
                        lock (m_inlineQueue)
                            m_inlineQueue.Enqueue( eei );
                        break;

                    case EDispatchMode.Batched:
                        lock (m_batch)
                            m_batch.Enqueue( eei );
                        break;

                    case EDispatchMode.Threadpool:
                        QueueForThreadpool( eei );
                        break;

                    default:
                        throw new InvalidProgramException( "Not able to figure out what to do with event- At least set the DefaultDispatchThread property correctly." );
                }
            }
        }
    }

    /// <summary>
    /// Allow a derived class to return a different kind of
    /// EventExecutionInstance, so long as its base class is still
    /// <see cref="MessageHandlingInstance"/>
    /// </summary>
    /// <param name="_event">The Event that needs to be executed</param>
    /// <param name="_handler">The Handler for the event</param>
    /// <returns>
    /// A new object that can be used to encapsulate the need to execute an
    /// event on a handler
    /// </returns>
    protected virtual MessageHandlingInstance GetEventExecutionInstance( object _event, MessageHandler _handler ) => new MessageHandlingInstance( _handler, _event );




    /// <summary>
    /// Given an event and a handler, queue a new UserWorkItem on the
    /// ThreadPool for the app domain.
    /// </summary>
    /// <param name="_eei">
    /// The event execution instance for the event
    /// </param>
    private void QueueForThreadpool( MessageHandlingInstance _eei )
    {
        // Account for the un-executed event
        IncrementThreadpoolItems();

        // Queue the work item on the ThreadPool
        ThreadPool.QueueUserWorkItem( _obj =>
        {
            var eei = _obj as MessageHandlingInstance;
            eei?.HandleEvent();
            DecrementThreadpoolItems(); // The handler is done- remove the event from the count
        }, _eei );
    }


    /// <summary>
    /// Execute all handlers in the "inline" queue. This queue is an
    /// internal structure to this class, so this method should never be
    /// called from outside this class (more specifically, the Post method)
    /// </summary>
    private void ExecuteInlineQueue()
    {
        MessageHandlingInstance? eei;
        // Go through the list of inline items and execute them.
        do
        {
            eei = TrySafeDequeue( m_inlineQueue );
            eei?.HandleEvent();
        } while (eei != null);
    }

    /// <summary>
    /// Execute all handlers that are in the batch, so long as the timeout
    /// hasn't occurred.
    /// </summary>
    /// <remarks>
    /// The purpose of the timeout is to somewhat limit how long processing
    /// is dedicated to handling events. Envisioned is the scenario where a
    /// game loop wants to dedicate a certain number of milliseconds per
    /// frame to event handling.
    /// 
    /// All handlers are handled "atomically" with regard to the timeout. No
    /// handler will be interrupted mid-execution. The timeout determines
    /// solely if ANOTHER handler can be called within this method
    /// invocation.
    /// 
    /// The timeout should not give the impression that the application can
    /// have long-running event handlers with no repurcussions. For
    /// instance, if an event handler takes 5 seconds to execute, no
    /// possible value of _timeout will prevent that 5 seconds from being
    /// consumed by the handler.
    /// </remarks>
    /// <param name="_timeout">
    /// An optional timeout parameter, specified in milliseconds. If 0
    /// (zero) is specified, exactly one batch item will be executed. If -1
    /// is specified, all outstanding items in the Batch will be executed,
    /// even if more are added while this method is executing.
    /// </param>
    /// <returns>
    /// The number of event handlers that were executed by this method
    /// invocation
    /// </returns>
    public int ExecuteBatch( int _timeout = -1 )
    {
        var count = 0;

        var endTime = DateTime.MaxValue;
        if (_timeout > 0)
            endTime = DateTime.Now + TimeSpan.FromMilliseconds( _timeout );
        else if (_timeout == 0)
            endTime = DateTime.MinValue;

        do
        {
            var eei = TrySafeDequeue( m_batch );
            if (eei == null)
                break;

            eei.HandleEvent();
            count++;

        } while (DateTime.Now < endTime);

        if (count == 0) // nothing executed, so...
        {
            if (m_nothingExecutedInBatch == 0)
                MorpheusLog.Logger.Debug( $"Executed Batch - First Time with nothing in the queue" );

            m_nothingExecutedInBatch++;
        }
        else if (m_nothingExecutedInBatch > 0)
        {
            MorpheusLog.Logger.Debug( $"Executed Batch {m_nothingExecutedInBatch} times with nothing in the queue" );
            m_nothingExecutedInBatch = 0;
        }

        return count;
    }
    private int m_nothingExecutedInBatch = 0;


    /// <summary>
    /// Dequeue an item from the Batch queue, if one exists. The operation
    /// needs to be threadsafe.
    /// </summary>
    /// <returns>
    /// The next item in the batch, or NULL if the batch is empty
    /// </returns>
    private MessageHandlingInstance? TrySafeDequeue( Queue<MessageHandlingInstance> _queue )
    {
        MessageHandlingInstance? eei = null;
        lock (_queue)
        {
            if (_queue.Count > 0)
            {
                eei = _queue.Dequeue();
            }
        }
        return eei;
    }


    /// <summary>
    /// Get an enumeration of all handlers known by the dispatcher.
    /// </summary>
    /// <returns>
    /// A list (snapshot) of all handlers that the dispatcher had registered
    /// when this method was called.
    /// </returns>
    public List<MessageHandler> GetAllHandlers()
    {
        var handlers = new List<MessageHandler>();

        lock (SyncObject)
        {
            foreach (var kv in m_handlersForTypes)
            {
                foreach (var h in kv.Value)
                    handlers.Add( h );
            }
        }
        return handlers;
    }
}
