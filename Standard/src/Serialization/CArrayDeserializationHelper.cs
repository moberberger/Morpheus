#nullable disable

namespace Morpheus;


/// <summary>
/// This class is support for deserializing arrays. This is a complex process when one wants to be "flexible" about handling array
/// deserialization.
/// </summary>
/// <remarks>
/// This class is responsible for keeping track of the array and the elements of the array. It is not responsible for any analysis
/// of the XML itself.
/// 
/// After construction, this class allows the deserializer a mechanism to add information to the array without needing to keep track
/// of information such as "current index", while at the same time allowing the application the ability to "adjust" the current index
/// if needed.
/// </remarks>
internal class CArrayDeserializationHelper
{
    private readonly int m_rank;
    private readonly int[] m_lowerBounds;
    private readonly int[] m_lengths;
    private int[] m_currentIndicies;

    private bool m_isFull = false;


    /// <summary>
    /// Access to the underlying array resulting from the object's operation
    /// </summary>
    internal Array Array { get; }

    /// <summary>
    /// The expected Type of each element in the array
    /// </summary>
    internal Type ElementType { get; }

    /// <summary>
    /// Construct using an "existing object", or a "working object" presumably taken from a surrogate
    /// </summary>
    /// <param name="_array"></param>
    internal CArrayDeserializationHelper( Array _array )
    {
        Array = _array ?? throw new ArgumentNullException(
                "Must construct the array helper with a non-null array object. Chances are the working object was not an array." );
        m_rank = Array.Rank;
        var arrayType = Array.GetType();
        if (!arrayType.IsArray)
        {
            throw new XDeserializationError(
                "A 'working object' was used to deserialize an array, but that working object was not an array itself." );
        }
        ElementType = arrayType.GetElementType();

        m_lowerBounds = new int[m_rank];
        m_lengths = new int[m_rank];
        for (var i = 0; i < m_rank; i++)
        {
            m_lowerBounds[i] = Array.GetLowerBound( i );
            m_lengths[i] = Array.GetLength( i );
        }

        ResetIndicies();
    }

    /// <summary>
    /// Construct an array from information presumably found in the XML
    /// </summary>
    /// <param name="_elementType"></param>
    /// <param name="_lengths"></param>
    /// <param name="_lowerBounds"></param>
    internal CArrayDeserializationHelper( Type _elementType, int[] _lengths, int[] _lowerBounds )
    {
        m_rank = _lengths.Length;
        ElementType = _elementType;
        m_lengths = _lengths;
        m_lowerBounds = _lowerBounds ?? (new int[m_rank]);

        if (m_lengths.Length != m_lowerBounds.Length)
        {
            throw new ArgumentException( "Lengths array has " + m_lengths.Length +
                                         " elements, but LowerBounds array has " + m_lowerBounds.Length );
        }

        Array = Array.CreateInstance( ElementType, m_lengths, m_lowerBounds );

        ResetIndicies();
    }


    /// <summary>
    /// Set the current indicies to equal the lower-bound for each dimension
    /// </summary>
    internal void ResetIndicies()
    {
        m_isFull = false;
        m_currentIndicies = new int[m_rank];
        for (var i = 0; i < m_rank; i++)
        {
            m_currentIndicies[i] = m_lowerBounds[i];
        }
    }

    /// <summary>
    /// Used to set the "Current indicies" by the application, most likely when the XML has an explicit 'Index' attribute
    /// </summary>
    /// <param name="_newIndicies">The indicies to set the currentIndex array to.</param>
    internal void SetIndicies( int[] _newIndicies )
    {
        if (_newIndicies.Length != m_rank)
        {
            throw new XDeserializationError( "The array deserializer was told to set indicies for " +
                                             _newIndicies.Length + " dimensions, but the array has " + m_rank +
                                             " dimensions." );
        }

        for (var i = 0; i < m_rank; i++)
        {
            if (_newIndicies[i] > GetHighestIndex( i ))
            {
                throw new ArgumentOutOfRangeException( "New Index " + i +
                                                       " is too large, given the current array data" );
            }
            if (_newIndicies[i] < m_lowerBounds[i])
            {
                throw new ArgumentOutOfRangeException( "New Index " + i +
                                                       " is too small, given the current array data" );
            }

            m_currentIndicies[i] = _newIndicies[i];
        }
        m_isFull = false;
    }

    /// <summary>
    /// Return the highest allowable index for the given dimension. Valid Indicies are between m_lowerBound and GetHighestIndex, INCLUSIVE
    /// </summary>
    /// <param name="_dimension">The dimension of the index to check for</param>
    /// <returns>The highest valid index for that dimension, INCLUSIVE</returns>
    internal int GetHighestIndex( int _dimension ) => m_lowerBounds[_dimension] + m_lengths[_dimension] - 1;

    /// <summary>
    /// This routine will increment the indicies by one. The return value denotes whether or not this was successful.
    /// </summary>
    /// <returns>TRUE if the indicies were incremented, FALSE if they weren't due to the array being "full" (all indicies at their max)</returns>
    internal bool IncrementIndicies()
    {
        if (m_isFull)
        {
            throw new InvalidOperationException(
                "It is not allowed to increment the indicies once the array has been deemed 'full'" );
        }

        for (var dim = m_rank - 1; dim >= 0; dim--)
        {
            if (m_currentIndicies[dim] < GetHighestIndex( dim ))
            {
                m_currentIndicies[dim]++;
                return true;
            }
            // Overflow- Reset for this dimension and move on to next.
            m_currentIndicies[dim] = m_lowerBounds[dim];
        }
        // If the loop falls through, then all of the dimensions "overflowed", which means we're at the end and the increment actually failed.
        return false;
    }

    /// <summary>
    /// Add an object to the array. Increment the indicies after the item is added.
    /// </summary>
    /// <param name="_objectToAdd">The object being added to the array</param>
    /// <returns>TRUE if the array has more space in it, FALSE if it doesn't.</returns>
    internal bool Add( object _objectToAdd )
    {
        if (m_isFull)
        {
            throw new InvalidOperationException(
                "It is not allowed to add to the array once the array has been deemed 'full'" );
        }

        Array.SetValue( _objectToAdd, m_currentIndicies );
        return IncrementIndicies();
    }
}