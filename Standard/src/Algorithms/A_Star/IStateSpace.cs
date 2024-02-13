namespace Morpheus;


/// <summary>
/// Interface for A* to be able to deal with a "State Space". The implementation details are so not important to A* that
/// this interface exposes truly what needs to be done in order to properly path through a state space.
/// </summary>
public interface IStateSpace
{
    /// <summary>
    /// Remove all path information from the implementation of the state space, placing each IStateNode in a
    /// cleared state
    /// </summary>
    void ResetPath();

    /// <summary>
    /// Retrieve the "Start State" for the path.
    /// </summary>
    IStateNode GetStartState();

    /// <summary>
    /// Test node against goal state
    /// </summary>
    bool IsGoalState( IStateNode p_node );

    /// <summary>
    /// Given any particular state in the state space, return an enumeration of all successor states possible.
    /// 
    /// The nodes returned must have their Heuristic (Estimated Cost to Goal) value set by the state space.
    /// </summary>
    /// <param name="p_stateNode">The state for which successors will be enumerated and returned</param>
    /// <returns>An enumeration of the successor states for a given state node</returns>
    IEnumerable<IStateNode> Successors( IStateNode p_stateNode );

    /// <summary>
    /// An app-defined function that can calculate the actual cost of moving from one state to another
    /// </summary>
    Func<IStateNode, IStateNode, double> FnCalculateActualCost { get; set; }

    /// <summary>
    /// An app-defined function that can estimate the cost of getting to the goal from a node (the Heuristic)
    /// </summary>
    Func<IStateNode, double> FnCalculateEstimatedCost { get; set; }
}
