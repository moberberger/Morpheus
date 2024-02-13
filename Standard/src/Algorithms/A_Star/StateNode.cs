namespace Morpheus;


/// <summary>
/// The StateNode part of the StateSpace class
/// </summary>
/// <typeparam name="TData">The user-type used for the State Space</typeparam>
public abstract partial class StateSpace<TData>
    where TData : class
{
    /// <summary>
    /// This is a good general implementation of <see cref="IStateNode"/> which will handle
    /// most every kind of application data. It is used (and mandated) by the
    /// <see cref="StateSpace{TData}"/> implementation of <see cref="IStateSpace"/> .
    /// </summary>
    public class StateNode : IStateNode
    {
        // Constructed with...

        /// <summary>
        /// The state space that this node belongs to
        /// </summary>
        public IStateSpace StateSpace { get; private set; }

        /// <summary>
        /// The application data that this node represents
        /// </summary>
        public TData Data { get; private set; }


        // Maintained / Manipulated by AStar

        /// <summary>
        /// Set by A-Star as it builds the path towards the goal
        /// </summary>
        public double ActualCostFromStart { get; set; } = double.NaN;

        /// <summary>
        /// Set by A-Star when it has removed the node from the Open list
        /// </summary>
        public bool IsClosed { get; set; } = false;

        /// <summary>
        /// Set by A-Star when it adds or updates a node on the current path
        /// </summary>
        public IStateNode? Parent { get; set; } = null;

        /// <summary>
        /// Used by the priority queue for optimized access
        /// </summary>
        public int HeapIndex { get; set; } = -1;


        /// <summary>
        /// Calculate the estimated cost to the goal (the Heuristic) if it isn't already
        /// stored.
        /// </summary>
        public double EstimatedCostToGoal
        {
            get
            {
                if (m_estimatedCostToGoal == double.NaN)
                {
                    if (StateSpace.FnCalculateEstimatedCost == null)
                        throw new MissingMethodException( "Application must assign a delegate to calculate the estimated cost between states" );

                    m_estimatedCostToGoal = StateSpace.FnCalculateEstimatedCost( this );
                }

                return m_estimatedCostToGoal;
            }
        }
        private double m_estimatedCostToGoal = double.NaN;


        /// <summary>
        /// This straight-forward implementation assumes Actual + Estimated = Total
        /// </summary>
        public double TotalEstimatedCost => ActualCostFromStart + EstimatedCostToGoal;

        /// <summary>
        /// If the HeapIndex is -1, then the node is not in the heap, and therefore not open
        /// </summary>
        public bool IsOpen => HeapIndex >= 0;


        // Implementation of other functionality

        /// <summary>
        /// Reset the node's values that aren't part of the constructor
        /// </summary>
        public void ResetState()
        {
            m_estimatedCostToGoal = double.NaN;
            ActualCostFromStart = double.NaN;
            IsClosed = false;
            Parent = null;
            HeapIndex = -1;
        }



        /// <summary>
        /// Must construct with a state space and a piece of application data
        /// </summary>
        /// <param name="_stateSpace">The state space that this node belongs to</param>
        /// <param name="_data">The application data represented by this node</param>
        public StateNode( IStateSpace _stateSpace, TData _data )
        {
            StateSpace = _stateSpace ?? throw new ArgumentNullException( "Cannot create a StateNode with a NULL state space" );
            Data = _data ?? throw new ArgumentNullException( "Cannot create a StateNode with NULL data" );

            ResetState();
        }

        // IComparable implementation


        /// <summary>
        /// General IComparable refers to specific implementation
        /// </summary>
        /// <param name="_other">Some other object</param>
        /// <returns>See IComparable</returns>
        public int CompareTo( object? _other )
        {
            var other = _other as IStateNode
                ?? throw new InvalidCastException( $"Cannot convert '{_other?.GetType()}' to IStateNode" );

            return CompareTo( other );
        }

        /// <summary>
        /// Decide if one state node belongs before or after another in the Priority Queue
        /// </summary>
        /// <param name="_other">The other node</param>
        /// <returns></returns>
        public int CompareTo( IStateNode? _other )
        {
            if (_other is null)
                throw new NotSupportedException( "Cannot compare an IStateNode to NULL" );

            var _ths = this.TotalEstimatedCost;
            var _oth = _other.TotalEstimatedCost;

            if (_ths < _oth) return -1;
            if (_ths > _oth) return 1;

            return ActualCostFromStart.CompareTo( _other.ActualCostFromStart );
        }
    }
}
