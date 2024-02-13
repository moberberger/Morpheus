using System.Reflection;
using Morpheus.EventDispatcher;

namespace Morpheus;


/// <summary>
/// Helper class that uses Reflection to find methods suitable for Event Dispatch using the
/// <see cref="Dispatcher"/> .
/// </summary>
/// <remarks>
/// An event handler that deals with Types can be any method that takes zero, one or two
/// parameters, as follows:
/// 
/// 0 params: The AEventHandler MUST specifiy the EventType for the handler. Nothing will be
/// sent to the handler when the event is handled.
/// 
/// 1 param: The handler will receive one parameter, which is the event itself. No other
/// context will be sent to the handler.
/// 
/// 2 params: The first param is the event. The second param is either a CEventHandler
/// context or a CEventDispatcher (or subclass) context. In this case, the discovery will
/// figure out what to send the handler, including figuring out if a subclass of the
/// CEventDispatcher is appropriate. This allows the application to inherit
/// CEventDispatcher, place application-specific information in the subclass, then have that
/// context-full dispatcher sent to static functions (or instance functions, but arguably
/// less useful)
/// 
/// An event handler that deals with a specific Value can only receive one parameter. This
/// is either a <see cref="MessageHandler"/> or something that can be cast to a
/// <see cref="Dispatcher"/> . The event's value will not be sent as a parameter
/// because that method can discover that value via the CEventHandler object, if it really
/// needs it.
/// </remarks>
public class MessageHandlerDiscovery
{
    /// <summary>
    /// The dispatcher uesd with this discovery object
    /// </summary>
    private readonly Dispatcher m_dispatcher;

    /// <summary>
    /// Must be constructed with a Dispatcher to use as target for handler registration
    /// </summary>
    /// <param name="_dispatcher">
    /// The dispatcher that will be used to register (and de-register) handlers
    /// </param>
    public MessageHandlerDiscovery( Dispatcher _dispatcher )
    {
        m_dispatcher = _dispatcher ?? throw new ArgumentNullException( "Must specifiy an Event Dispatcher" );
    }

    /// <summary>
    /// Set this before calling RegisterHandlers to have all handlers registered using this
    /// dispatch mode IFF the <see cref="AEventHandler"/> attribute doesn't contain a
    /// different EDispatchMode
    /// </summary>
    public EDispatchMode DispatchMode = EDispatchMode.NotAssigned;

    /// <summary>
    /// Register all event handlers for the calling assembly by looking for the
    /// AEventHandler attribute on a class and also on a method. This only works with static
    /// methods.
    /// </summary>
    /// <remarks>
    /// This method first looks for a class that has the <see cref="AEventHandler"/>
    /// attribute. This signals that there may be methods within this class that also
    /// contain event handlers. The class (Type) is then searched for methods that have the
    /// <see cref="AEventHandler"/> attribute.
    /// </remarks>
    /// <returns>A list of Event Handlers that were found</returns>
    public List<MessageHandler> RegisterHandlers() => RegisterHandlers( Assembly.GetCallingAssembly() );

    /// <summary>
    /// Register all event handlers in the specified assembly by looking for the
    /// AEventHandler attribute on a class and also on a method. This only works with static
    /// methods.
    /// </summary>
    /// <remarks>
    /// This method first looks for a class that has the <see cref="AEventHandler"/>
    /// attribute. This signals that there may be methods within this class that also
    /// contain event handlers. The class (Type) is then searched for methods that have the
    /// <see cref="AEventHandler"/> attribute.
    /// </remarks>
    /// <param name="_assembly">
    /// The assembly to search for the AEventHandler attribute
    /// </param>
    /// <returns>A list of Event Handlers that were found</returns>
    public List<MessageHandler> RegisterHandlers( Assembly _assembly )
    {
        var retval = new List<MessageHandler>();

        var types = _assembly.GetExportedTypes();
        for (var i = 0; i < types.Length; i++)
        {
            var t = types[i];
            var attrs = t.GetCustomAttributes( typeof( AEventHandler ), false );
            if (attrs.Length == 1)
            {
                var typeHandlers = RegisterHandlers( t );
                retval.AddRange( typeHandlers );
            }
        }

        return retval;
    }

    /// <summary>
    /// Register all event handlers in the specified Type by looking for the AEventHandler
    /// attribute on all static methods.
    /// </summary>
    /// <remarks>
    /// This method first looks for a method that has the <see cref="AEventHandler"/>
    /// attribute. When it finds one, it registers that static method as a handler for
    /// events matching the Type of the first parameter.
    /// </remarks>
    /// <param name="_type">The Type to search for the AEventHandler attribute</param>
    /// <returns>A list of Event Handlers that were found</returns>
    public List<MessageHandler> RegisterHandlers( Type _type ) => RegisterHandlers( _type, null );

    /// <summary>
    /// Register all event handlers in the specified object by looking for the AEventHandler
    /// attribute on all instance methods.
    /// </summary>
    /// <remarks>
    /// This method first looks for an instance method that has the
    /// <see cref="AEventHandler"/> attribute. When it finds one, it registers that method
    /// as a handler for events matching the Type of the first parameter.
    /// </remarks>
    /// <param name="_instanceHandlers">
    /// An object that should have instance methods with the AEventHandler attribute
    /// </param>
    /// <returns>A list of Event Handlers that were found</returns>
    public List<MessageHandler> RegisterInstanceHandlers( object _instanceHandlers ) => RegisterHandlers( _instanceHandlers.GetType(), _instanceHandlers );

    /// <summary>
    /// Register all event handlers in the specified Type by looking for the AEventHandler
    /// attribute on all methods. Static methods will be searched when _object is null.
    /// </summary>
    /// <remarks>
    /// This method first looks for a method that has the <see cref="AEventHandler"/>
    /// attribute. When it finds one, it registers that static method as a handler for
    /// events matching the Type of the first parameter.
    /// </remarks>
    /// <param name="_type">The Type to search for the AEventHandler attribute</param>
    /// <param name="_object">
    /// The object to reference when invoking the MethodInfo object
    /// </param>
    /// <returns>A list of Event Handlers that were found</returns>
    private List<MessageHandler> RegisterHandlers( Type _type, object? _object )
    {
        var retval = new List<MessageHandler>();

        var flags = BindingFlags.Public | BindingFlags.FlattenHierarchy;
        flags |= (_object == null) ? BindingFlags.Static : BindingFlags.Instance;

        var methods = _type.GetMethods( flags );
        foreach (var mi in methods)
        {
            var attrs = mi.GetCustomAttributes( typeof( AEventHandler ), false );
            if (attrs.Length == 1) // the method has the attribute
            {
                var handler = RegisterHandler( mi, (AEventHandler)attrs[0], _object );
                retval.Add( handler );
            }
        }

        return retval;
    }

    /// <summary>
    /// Given a method and an AEventHandler attribute, figure out how to create a handler
    /// out of the method.
    /// </summary>
    /// <param name="_methodInfo">
    /// The <see cref="MethodInfo"/> information for the method
    /// </param>
    /// <param name="_attribute">
    /// The AEventHandler attribute that may contain information about how to handle events
    /// </param>
    /// <param name="_object">
    /// The object (context) to use with the MethodInfo invocation, or NULL if its a static
    /// method
    /// </param>
    /// <returns>
    /// The CEventHandler object for the new handler. This is registered with the Dispatcher
    /// before its returned.
    /// </returns>
    private MessageHandler RegisterHandler( MethodInfo _methodInfo, AEventHandler _attribute, object? _object )
    {
        var dMode = _attribute.DispatchMode;
        if (dMode == EDispatchMode.NotAssigned)
            dMode = DispatchMode;

        var handler = new Handler_Reflection( m_dispatcher, dMode, _methodInfo, _attribute, _object )
        {
            Description = _attribute.Description
        };

        if (handler.EventType is null)
            throw new InvalidOperationException( "The AEventHandler attribute must specify the EventType" );

        m_dispatcher.AddHandlerToDatabase( handler.EventType, handler );
        return handler;
    }
}
