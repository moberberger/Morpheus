using System.Collections;

namespace Morpheus;

/// <summary>
/// Usage Model:
/// 
/// <code>
/// for( VariableRadixCounter c = new( 3,4,3,2 ); c.NotDone; c++ )
/// {
///     int x = c[2];
///     string s = c.Digits.JoinAsString( ", " );
///     Console.WriteLine( c );
/// }
/// </code>
/// </summary>
public class VariableRadixCounter : IEnumerable<int[]>
{
    private readonly int[] Radixes;
    private readonly int[] Counters;
    public bool Done { get; private set; }
    public bool NotDone => !Done;

    public int this[int index] => Counters[index];
    public int[] Digits => Counters;

    public VariableRadixCounter( IEnumerable<int> radixes )
    {
        Radixes = radixes.ToArray();
        Counters = new int[Radixes.Length];
    }

    public VariableRadixCounter Next()
    {
        if (NotDone)
            NextInternal( Radixes.Length - 1 );

        return this;
    }

    private void NextInternal( int index )
    {
        Done = index < 0;
        if (NotDone)
        {
            Counters[index]++;
            if (Counters[index] >= Radixes[index])
            {
                Counters[index] = 0;
                NextInternal( index - 1 );
            }
        }
    }

    public static VariableRadixCounter operator ++( VariableRadixCounter c ) => c.Next();
    public override string ToString() => "{ " + Counters.JoinAsString( " " ) + " }";

    public IEnumerator<int[]> GetEnumerator()
    {
        while (NotDone)
        {
            Next();
            yield return (int[])Digits.Clone();
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static IEnumerable<T[]> Enumerate<T>( IEnumerable<IEnumerable<T>> values )
    {
        var vals = values.Select( v => v.ToList() ).ToList();
        var results = new T[vals.Count()];
        foreach (var indices in new VariableRadixCounter( vals.Select( s => s.Count ) ))
        {
            for (int i = 0; i < results.Length; i++)
                results[i] = vals[i][indices[i]];
            yield return results;
        }
    }

}
