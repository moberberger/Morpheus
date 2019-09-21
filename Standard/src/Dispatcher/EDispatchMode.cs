using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morpheus
{
    /// <summary>
    /// Tells the dispatcher how (on which thread and/or when) to execute handlers for events.
    /// </summary>
    public enum EDispatchMode
    {
        /// <summary>
        /// Used in combination with hints and/or defaults to specify handling when nothing explicit has been set
        /// </summary>
        NotAssigned = 0,

        /// <summary>
        /// Inlined dispatch occurs within the Post method of the dispatcher, in the same thread as the caller of Post
        /// </summary>
        Inline,

        /// <summary>
        /// Threadpool dispatch occurs on the threadpool. The UserWorkItem is immediately queued from within the Post method. 
        /// The handler itself is guaranteed not to execute on the same thread as the caller.
        /// </summary>
        Threadpool,

        /// <summary>
        /// Batched dispatch occurs on whichever thread calls the ExecuteBatch method of the dispatcher. The Post method 
        /// merely queues batched events up for execution in ExecuteBatch. The events are handled in a FIFO manner.
        /// </summary>
        Batched
    }
}
