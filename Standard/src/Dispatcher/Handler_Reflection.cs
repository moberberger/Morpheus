using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Morpheus.EventDispatcher
{
    /// <summary>
    /// This class will handle an event that was registered using CEventHandlerDiscovery. It
    /// is not strongly typed to the event that is to be handled.
    /// 
    /// This class will allow the association of a handler that accepts a subclass of the
    /// specified event as its first (and possibly only) parameter.
    /// </summary>
    internal class Handler_Reflection : MessageHandler
    {
        private MethodInfo m_methodInfo;
        private AEventHandler m_methodAttribute;
        private int m_paramCount = 0;

        internal int m_eventIndex = -1;
        internal int m_dispatcherIndex = -1;
        internal int m_thisIndex = -1;


        /// <summary>
        /// The object that provides context to non-static methods
        /// </summary>
        public object Object { get; private set; }

        /// <summary>
        /// Create a Reflection-based handler given the minimum requirements. Use other
        /// methods to finish populating this object
        /// </summary>
        /// <param name="_dispatcher">
        /// The Dispatcher that this handler is associated with
        /// </param>
        /// <param name="_dispatchMode">The EDispatchMode for this handler</param>
        internal Handler_Reflection( Dispatcher _dispatcher, EDispatchMode _dispatchMode )
            : base( null, _dispatcher, _dispatchMode )
        {
        }


        /// <summary>
        /// Set up this object correctly for a given <see cref="MethodInfo"/>
        /// </summary>
        /// <param name="_methodInfo"></param>
        /// <param name="_attribute"></param>
        /// <param name="_object"></param>
        internal void ProcessMethodInfo( MethodInfo _methodInfo, AEventHandler _attribute, object _object )
        {
            m_methodInfo = _methodInfo;
            m_methodAttribute = _attribute;
            Object = _object;


            var _params = _methodInfo.GetParameters();
            if (_params.Length > 3) // The method takes too many parameters- No way to figure out what to send beyond 2
            {
                throw new XReflectionArgumentException( m_methodInfo, "Too Many Parameters ({0})", _params.Length );
            }
            m_paramCount = _params.Length;


            // base.EventType will contain an accurate event, or NULL if the Type needs to
            // be learned from the method signature
            SetEventType();

            for (var i = 0; i < m_paramCount; i++)
            {
                var pi = _params[i];

                if (pi.ParameterType == typeof( MessageHandler ))
                {
                    m_thisIndex = i;
                }
                else if (typeof( Dispatcher ).IsAssignableFrom( pi.ParameterType ))
                {
                    if (pi.ParameterType.IsAssignableFrom( Dispatcher.GetType() ))
                    {
                        m_dispatcherIndex = i;
                    }
                    else
                    {
                        throw new XReflectionArgumentException(
                            m_methodInfo,
                            "Parameter {0} is based on CEventDispatcher ({1}), but it cannot be assigned from THIS Event dispatcher ({2}).",
                            i,
                            pi.ParameterType.Name,
                            Dispatcher.GetType().Name );
                    }
                }
                else
                {
                    if (base.EventType == null) // hasn't been set yet, so set it according to this parameter's information
                    {
                        base.EventType = pi.ParameterType;
                    }
                    else // It has been set, make sure that this parameter type is assignable
                    {
                        // The EventType must be a subclass of the parameter (method takes a
                        // "Shape", but should only be triggered for "Squares")
                        if (!pi.ParameterType.IsAssignableFrom( base.EventType )) // The parameter can't be set from an event Type specified in the attribute
                        {
                            throw new XReflectionArgumentException(
                                m_methodInfo,
                                "The Event parameter {0} is of type ({1}), which cannot be assigned from the Type specified in the AEventHandler attribute ({2})",
                                i,
                                pi.ParameterType.Name,
                                _attribute.EventType.Name );
                        }
                    }
                    m_eventIndex = i;
                }
            }

            if (base.EventType == null)
            {
                throw new XReflectionArgumentException( m_methodInfo, "There was not enough information present to determine what Type of events to handle." );
            }

            var count = 0;
            if (m_eventIndex >= 0)
                count++;
            if (m_thisIndex >= 0)
                count++;
            if (m_dispatcherIndex >= 0)
                count++;

            if (count != m_paramCount)
                throw new XReflectionArgumentException( m_methodInfo, "There was a parameter count mismatch ({0} on method, {1} identified)", m_paramCount, count );
        }

        /// <summary>
        /// Setting the EventType can be non-trivial when both the AEventHandler.EventType
        /// and AEventHandler.Value are set. If Value is not set, then simply take EventType
        /// from the attribute. This may still be NULL if the method signature itself must
        /// be used to figure out the EventType
        /// </summary>
        private void SetEventType()
        {
            if (m_methodAttribute.Value != null) // Review EventType if this is a specific-value filter
            {
                base.EventType = m_methodAttribute.Value.GetType();
                base.SpecificValueFilter = m_methodAttribute.Value;

                if (m_methodAttribute.EventType != null) // Make sure the attribute's EventType matches the Value's Type
                {
                    if (m_methodAttribute.EventType != base.EventType)
                    {
                        throw new XReflectionArgumentException(
                            m_methodInfo,
                            "The Attribute has has both an EventType ({0}) and a Value (Type={1}) specified, and the Types of these don't agree",
                            m_methodAttribute.EventType.FullName,
                            m_methodAttribute.Value.GetType().FullName );
                    }
                }
            }
            else
            {
                base.EventType = m_methodAttribute.EventType; // Get the EventType from the attribute. If its NULL, we'll try to set it below
            }
        }




#pragma warning disable IDE0011 // Add braces
        /// <summary>
        /// Called when the actual handler needs to be called. Will build the parameter
        /// array to suit.
        /// </summary>
        /// <param name="_event"></param>
        public override void HandleEvent( object _event )
        {
            if (m_paramCount == 0)
            {
                MorpheusLog.Logger?.Debug( $"Discovered No Parameter Handler: {m_methodInfo.Name}( {_event.GetType().Name} )" );
                m_methodInfo.Invoke( Object, null );
            }
            else
            {
                var _params = new object[m_paramCount];

                for (var i = 0; i < m_paramCount; i++)
                {
                    if (m_eventIndex == i)
                        _params[i] = _event;
                    else if (m_thisIndex == i)
                        _params[i] = this;
                    else if (m_dispatcherIndex == i)
                        _params[i] = Dispatcher;
                    else
                        throw new InvalidOperationException(
                            string.Format( "While processing a ReflectionHandler ({0}), no value could be placed into Parameter #{1}",
                            m_methodInfo.Name, i ) );
                }

                MorpheusLog.Logger?.Debug( $"Discovered Event+Handler: {m_methodInfo.Name}( {_event.GetType().Name} )" );
                m_methodInfo.Invoke( Object, _params );
            }
        }
#pragma warning restore IDE0011 // Add braces
    }
}
