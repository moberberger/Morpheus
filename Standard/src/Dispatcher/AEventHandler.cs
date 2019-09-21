using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morpheus
{
    /// <summary>
    /// By placing this attribute on a class, that class can be automatically searched for
    /// event handlers. By placing this on a method, that method can be found using
    /// reflection and registered as an event handler for a specified event type.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Event )]
    public class AEventHandler : Attribute
    {
        /// <summary>
        /// The Type of the event to be handled by the event handler. Must "agree" with the
        /// first parameter of the method, if there is one. If present, this Type is what is
        /// registered for with the Dispatcher, not the Type of the first method parameter.
        /// </summary>
        public Type EventType = null;

        /// <summary>
        /// When set to a non-null value, only events that are equal to this value will be
        /// dispatched to the handler. This is a <see cref="object.Equals(object,object)"/>
        /// comparison, not a reference equality test.
        /// </summary>
        public object Value = null;

        /// <summary>
        /// How the event should be dispatched (inline, batch, threadpool). If set to
        /// <see cref="EDispatchMode.NotAssigned"/> , other defaults are used.
        /// </summary>
        public EDispatchMode DispatchMode = EDispatchMode.NotAssigned;

        /// <summary>
        /// A description of the handler. Maybe used for diagnostics
        /// </summary>
        public string Description = null;
    }
}
