namespace Morpheus;


/// <summary>
/// This is a base class for a State Space. The main function it provides is a caching
/// ability for AStar node states. It relies on an application implementation to describe
/// many of the things required by <see cref="IStateSpace"/> . This class mandates the use
/// of <see cref="StateNode"/> as the implementation of IStateNode.
/// </summary>
public abstract partial class StateSpace<TData> : IStateSpace
{
    /// <summary>
    /// This is the cache of all states for any node that has been requested by the AStar
    /// algorithm
    /// </summary>
    protected Dictionary<TData, StateNode> m_nodeLookup = new Dictionary<TData, StateNode>();

    /// <summary>
    /// A function that will calculate the "actual cost" of moving from one state to the
    /// next. Used by the AStar algorithm to determine the cost between a node and one of
    /// its successors. Must be assigned by the application.
    /// </summary>
    public virtual Func<IStateNode, IStateNode, double> FnCalculateActualCost { get; set; } = null!;

    /// <summary>
    /// A function that will provide a heuristic cost between a node and the Goal state.
    /// </summary>
    public virtual Func<IStateNode, double> FnCalculateEstimatedCost { get; set; } = null!;

    /// <summary>
    /// Retain state objects in the cache, but clear their data to default values.
    /// </summary>
    public void ResetPath()
    {
        foreach (var node in m_nodeLookup.Values)
            node.ResetState();
    }

    /// <summary>
    /// The implementation of Successors for <see cref="IStateSpace"/> . This relies on the
    /// abstract function of this class to generate successor data from the application.
    /// </summary>
    /// <param name="_stateNode">The state node to generate successors for</param>
    /// <returns>An enumeration of successors for the node specified</returns>
    public IEnumerable<IStateNode> Successors( IStateNode _stateNode )
    {
        var node = _stateNode as StateNode ?? throw new ArgumentNullException();

        foreach (var data in Successors( node.Data ))
        {
            IStateNode nextNode = GetStateNodeFromData( data );
            yield return nextNode;
        }
    }

    /// <summary>
    /// A helper function to return a State object for an application-defined Data object.
    /// This will either get the state from the cache (if it exists) or add a new state
    /// object to the cache (if its not already there).
    /// </summary>
    /// <param name="p_data">The application data to get a state node for</param>
    /// <returns>A IStateNode implementation for the application data specified</returns>
    public virtual StateNode GetStateNodeFromData( TData p_data )
        => m_nodeLookup.GetOrAdd( p_data, _d => new StateNode( this, _d ) );




    /// <summary>
    /// Implemented by the application to return all app-data which succeeds the specified
    /// data. The application needs to have no knowledge of AStar state to work with this
    /// function.
    /// </summary>
    /// <param name="p_data">The data element to generate successors for</param>
    /// <returns>An enumeration of successors for the data specified</returns>
    public abstract IEnumerable<TData> Successors( TData p_data );

    /// <summary>
    /// Application must return a StateNode for the starting state. May use
    /// <see cref="GetStateNodeFromData"/> to turn data into an IStateNode
    /// </summary>
    /// <returns>A state node representing the starting state of the path</returns>
    public abstract IStateNode GetStartState();

    /// <summary>
    /// Application defined mechanism for determining if a state is the "Goal State".
    /// </summary>
    /// <param name="p_node">The state to check against the "Goal" state</param>
    /// <returns>TRUE if the Goal is equivalent to this node, FALSE if not</returns>
    public abstract bool IsGoalState( IStateNode p_node );
}
