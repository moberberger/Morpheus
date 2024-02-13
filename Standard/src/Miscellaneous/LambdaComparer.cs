#nullable disable

namespace Morpheus;


/// <summary>
/// This is a "plugin" that allows the application a more expressive method of specifying a
/// comparator for two objects.
/// </summary>
/// <typeparam name="T">The type of the data that is to be compared</typeparam>
public class LambdaComparer<T> : IComparer<T>
{
    /// <summary>
    /// A delegate used by this class as an IComparer
    /// </summary>
    /// <param name="_left">The left side of the inequality being tested</param>
    /// <param name="_right">The right side of the inequality being tested</param>
    /// <returns>
    /// NEGATIVE if the left side is LESS THAN the right, POSITIVE if its GREATER THAN, and
    /// zero if they are EQUAL
    /// </returns>
    public delegate int DLambdaComparer( T _left, T _right );

    private readonly DLambdaComparer m_comparer;

    /// <summary>
    /// Construct an IComparer object for use in datasets requiring ordering of elements.
    /// </summary>
    /// <param name="_comparer">
    /// A lambda returning a Negative number if the left is less than the right, 0 if
    /// they're equal, Positive if the left is greater than the right
    /// </param>
    public LambdaComparer( DLambdaComparer _comparer ) => m_comparer = _comparer;

    /// <summary>
    /// Implementation of IComparer using the comparer lambda specified in the constructor
    /// </summary>
    /// <param name="_left">The "left-hand" parameter for the relationship operator</param>
    /// <param name="_right">The "right-hand" parameter for the relationship operator</param>
    /// <returns>
    /// Negative number if the left is less than the right, 0 if they're equal, Positive if
    /// the left is greater than the right
    /// </returns>
    public int Compare( T _left, T _right ) => m_comparer( _left, _right );

    /// <summary>
    /// Create a CLambdaComparer based on the system "Func(T,T,int)" delegate
    /// </summary>
    /// <param name="_comparer">The lambda to create an IComparer for</param>
    /// <returns>A new IComparer using the specified "Func(T,T,int) delegate</returns>
    public static IComparer<T> FromFunc( Func<T, T, int> _comparer )
    {
        if (_comparer == null)
            throw new ArgumentNullException( "Must specify a non-null lambda function" );

        return new LambdaComparer<T>( new DLambdaComparer( _comparer ) );
    }
}
