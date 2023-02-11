using System.Collections;

namespace Morpheus;


/// <summary>
/// The ForEach extension methods all assure the "Execute" or "Run" usage model.
/// Enumerations are consumed, not produced.
/// </summary>
public static class ForEachExtensions
{
    /// <summary>
    /// This is an Executor- it "executes" the elements of an enumeration by actually
    /// enumerating them. In this overload, nothing is done with the elements returned from
    /// the enumeration.
    /// 
    /// This is an important step in the way LINQ handles enumerations- if they are never
    /// enumerated, then they are never executed.
    /// </summary>
    /// <remarks>Rather than "ForEach", both "Run" and "Execute" were considered.</remarks>
    /// <param name="stuff">The enumeration to enumerate</param>
    public static void ForEach( this IEnumerable stuff )
    {
        foreach (var _ in stuff) { }
    }


    /// <summary>
    /// This is an Executor- it "executes" the elements of an enumeration by actually
    /// enumerating them. In this overload, each element of the enumeration is passed to an
    /// <see cref="Action{T1}"/> for processing.
    /// 
    /// This is an important step in the way LINQ handles enumerations- if they are never
    /// enumerated, then they are never executed.
    /// </summary>
    /// <remarks>Rather than "ForEach", both "Run" and "Execute" were considered.</remarks>
    /// <param name="stuff">The enumeration to enumerate</param>
    public static void ForEach( this IEnumerable stuff, Action<object> action )
    {
        foreach (var item in stuff)
            action( item );
    }

    /// <summary>
    /// This is an Executor- it "executes" the elements of an enumeration by actually
    /// enumerating them. In this overload, each element of the enumeration is passed to an
    /// <see cref="Action{T1}"/> for processing.
    /// 
    /// This is an important step in the way LINQ handles enumerations- if they are never
    /// enumerated, then they are never executed.
    /// </summary>
    /// <remarks>Rather than "ForEach", both "Run" and "Execute" were considered.</remarks>
    /// <param name="stuff">The enumeration to enumerate</param>
    public static void ForEach<T>( this IEnumerable<T> stuff, Action<T> action )
    {
        foreach (var item in stuff)
            action( item );
    }



    /// <summary>
    /// This is an Executor- it "executes" the elements of an enumeration by actually
    /// enumerating them. In this overload, each element of the enumeration is passed to an
    /// <see cref="Action{T1,T2}"/> for processing. The second element of the Action's
    /// parameters indicates the index of the element in the sequence. This mimics the
    /// behavior of
    /// <see cref="Enumerable.Select{TSrc, TRes}(IEnumerable{TSource}, Func{TSrc, int, TRes})"/>
    /// 
    /// This is an important step in the way LINQ handles enumerations- if they are never
    /// enumerated, then they are never executed.
    /// </summary>
    /// <remarks>Rather than "ForEach", both "Run" and "Execute" were considered.</remarks>
    /// <param name="stuff">The enumeration to enumerate</param>
    public static void ForEach( this IEnumerable stuff, Action<object, int> actionWithIndex )
    {
        var index = 0;
        foreach (var item in stuff)
            actionWithIndex( item, index++ );
    }

    public static void ForEach<T>( this IEnumerable<T> stuff, Action<T, int> actionWithIndex )
    {
        var index = 0;
        foreach (var item in stuff)
            actionWithIndex( item, index++ );
    }




    /// <summary>
    /// Equivalent to "for" loops, as opposed to "foreach" loops.
    /// </summary>
    /// <param name="max"></param>
    /// <returns></returns>
    public static void ForEach( this int max, Action<int> action )
    {
        for (int i = 0; i < max; i++)
            action( i );
    }

    public static void ForEach( this (int, int) range, Action<int> action )
    {
        for (int i = range.Item1; i < range.Item2; i++)
            action( i );
    }

    public static void ForEach( this (int, int, int) range, Action<int> action )
    {
        for (int i = range.Item1; i < range.Item2; i += range.Item3)
            action( i );
    }



    /// <summary>
    /// Equivalent to a nested "for" loop model where a Cartesian product of both integer
    /// ranges is needed.
    /// </summary>
    /// <remarks>
    /// <code>
    /// (5,7).ForEach( (x,y) => grid[x][y].member = value;
    /// </code>
    /// </remarks>
    /// <param name="dimensions"></param>
    /// <param name="action"></param>
    public static void ForEach( this (int, int) dimensions, Action<int, int> action )
    {
        for (int x = 0; x < dimensions.Item1; x++)
            for (int y = 0; y < dimensions.Item2; y++)
                action( x, y );
    }

    public static void ForEach( this (int, int, int) dimensions, Action<int, int, int> action )
    {
        for (int x = 0; x < dimensions.Item1; x++)
            for (int y = 0; y < dimensions.Item2; y++)
                for (int z = 0; z < dimensions.Item3; z++)
                    action( x, y, z );
    }




    /// <summary>
    /// Equivalent to a nested "for" loop model where a Cartesian product of both integer
    /// ranges is needed.
    /// </summary>
    /// <remarks>
    /// <code>
    /// (5,7).ForEach( (x,y) => grid[x][y].member = value;
    /// </code>
    /// </remarks>
    /// <param name="dimensions"></param>
    /// <param name="action"></param>
    public static IEnumerable<(int, int)> Range( this (int, int) dimensions )
    {
        for (int x = 0; x < dimensions.Item1; x++)
            for (int y = 0; y < dimensions.Item2; y++)
                yield return (x, y);
    }

    public static IEnumerable<(int, int, int)> Range( this (int, int, int) dimensions )
    {
        for (int x = 0; x < dimensions.Item1; x++)
            for (int y = 0; y < dimensions.Item2; y++)
                for (int z = 0; z < dimensions.Item3; z++)
                    yield return (x, y, z);
    }




    /// <summary>
    /// Apply an action to each element of an enumeration
    /// </summary>
    /// <param name="action"></param>
    /// <returns>
    /// The same enumeration after <see cref="action"/> has been applied to each element
    /// </returns>
    // public static IEnumerable Apply( this IEnumerable stuff, Action
    // <object> action ) { foreach (var item in stuff) { action( item ); yield return item;
    // } }

    public static IEnumerable<T> Apply<T>( this IEnumerable<T> stuff, Action<T> action )
    {
        foreach (var item in stuff)
        {
            action( item );
            yield return item;
        }
    }

    /*
    // public static T Apply
    // <T> ( this IEnumerable stuff, Action <object> action, T retval ) { foreach (var item
    // in stuff) action( item ); return retval; }

    // public static T Apply
    // <T, U> ( this IEnumerable <U> stuff, Action <U> action, T retval ) { foreach (var
    // item in stuff) action( item ); return retval; }
    */


    /// <summary>
    /// Apply an action to each element of an enumeration
    /// </summary>
    /// <param name="actionWithIndex"></param>
    /// <returns>
    /// The same enumeration after <see cref="action"/> has been applied to each element
    /// </returns>
    public static IEnumerable Apply( this IEnumerable stuff, Action<object, int> actionWithIndex )
    {
        int index = 0;
        foreach (var item in stuff)
        {
            actionWithIndex( item, index++ );
            yield return item;
        }
    }

    public static IEnumerable<T> Apply<T>( this IEnumerable<T> stuff, Action<T, int> actionWithIndex )
    {
        int index = 0;
        foreach (var item in stuff)
        {
            actionWithIndex( item, index++ );
            yield return item;
        }
    }
}
