using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morpheus
{
    /// <summary>
    /// Implement the classic AStar algorithm without any preconcieved knowledge of the state
    /// space to explore
    /// </summary>
    public class AStar
    {
        /// <summary>
        /// This is the "open list" from which the algorithm pulls new nodes to consider
        /// </summary>
        private readonly PriorityQueueOptimized<IStateNode> m_openList = new PriorityQueueOptimized<IStateNode>();

        /// <summary>
        /// The AStar algorithm implementation
        /// </summary>
        /// <param name="p_stateSpace">The state space to explore</param>
        /// <returns>TRUE if a path was found, FALSE if not</returns>
        public IStateNode GeneratePath( IStateSpace p_stateSpace )
        {
            if (p_stateSpace == null)
                throw new InvalidOperationException( "Cannot generate a path with a NULL state space" );

            p_stateSpace.ResetPath();
            m_openList.Clear();

            var node = p_stateSpace.GetStartState();
            if (node == null)
                throw new InvalidOperationException( "The state space did not return a valid Start State" );

            node.ActualCostFromStart = 0;
            m_openList.Add( node );

            while (m_openList.Count > 0)
            {
                var state = m_openList.RemoveLowest();

                state.IsClosed = true;
                if (p_stateSpace.IsGoalState( state ))
                    return state;

                foreach (var sp in p_stateSpace.Successors( state ))
                {
                    if (!sp.IsClosed)
                        UpdateCell( state, sp, p_stateSpace );
                }
            }

            return null; // no path found
        }

        /// <summary>
        /// A state node has not yet been evaluated, so test it using the "current" path and
        /// update its values if its a shorter path
        /// </summary>
        /// <param name="p_current">The node being evaluated</param>
        /// <param name="p_successor">
        /// A successor to that node that needs to be checked for update
        /// </param>
        /// <param name="p_stateSpace">The state space governing the algorithm</param>
        protected virtual void UpdateCell( IStateNode p_current, IStateNode p_successor, IStateSpace p_stateSpace )
        {
            var deltaCost = p_stateSpace.FnCalculateActualCost( p_current, p_successor );

            // If its actually shorter to get to this node from the current node
            if (p_current.ActualCostFromStart + deltaCost < p_successor.ActualCostFromStart)
            {
                // Update the node's information to reflect its new position in the path
                p_successor.ActualCostFromStart = p_current.ActualCostFromStart + deltaCost;
                p_successor.Parent = p_current;

                m_openList.Update( p_successor ); // Will "Add" or "Update" depending on whether or not its in the queue
            }
        }
    }
}
