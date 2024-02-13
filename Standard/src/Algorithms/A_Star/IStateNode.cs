namespace Morpheus;


/// <summary>
/// This information is necessary for A* to process a given state within a state-space.
/// </summary>
/// <remarks>
/// It is recommended that the application try to use <see cref="StateSpace{TData}"/> rather than implement 
/// the corresponding interfaces. However, it is far from required- implementing <see cref="IStateNode"/> 
/// and <see cref="IStateSpace"/> directly in the application is absolutely fine and will result in no 
/// performance penalties.
/// </remarks>
public interface IStateNode : IOptimizedBinaryHeapNode, IComparable, IComparable<IStateNode>
{
    /// <summary>
    /// A value representing the "Total Estimated Cost" for any state in the state space.
    /// 
    /// A* relies on the IComparable implementation for this IStateNode to sort the nodes in the
    /// priority queue. It should be (ActualCostFromStart + EstimatedCostToGoal).
    /// </summary>
    double TotalEstimatedCost { get; }

    /// <summary>
    /// A value representing the "Actual Cost from Start" for any closed state
    /// </summary>
    double ActualCostFromStart { get; set; }

    /// <summary>
    /// A value representing the "Estimated Cost to Goal" (the Heuristic) for any state in the state space
    /// </summary>
    double EstimatedCostToGoal { get; }

    /// <summary>
    /// Allows A* to signal when a state has been evaluated.
    /// </summary>
    bool IsClosed { get; set; }

    /// <summary>
    /// Used by A* to create the actual "path" from the Goal state to the Start state
    /// </summary>
    IStateNode? Parent { get; set; }

    /// <summary>
    /// Direct the node to reset all internal state, as if freshly constructed
    /// </summary>
    void ResetState();
}
