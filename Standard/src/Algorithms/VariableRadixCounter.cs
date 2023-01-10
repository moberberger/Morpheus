namespace Morpheus;

/// <summary>
/// Usage Model:
/// 
/// <code>
/// for( VariableRadixCounter c = new( 3,4,3,2 ); c.NotDone; c.Next() )
/// {
///     int x = c[2];
///     string s = c.Digits.JoinAsString( ", " );
///     Console.WriteLine( c );
/// }
/// </code>
/// </summary>
public class VariableRadixCounter
{
    private readonly int[] Radixes;
    private readonly int[] Counters;
    public bool Done { get; private set; }
    public bool NotDone => !Done;

    public int this[int index] => Counters[index];
    public int[] Digits => Counters;

    public VariableRadixCounter( params int[] radixes )
    {
        Radixes = radixes.ToArray();
        Counters = new int[Radixes.Length];
    }

    public void Next( int index = int.MaxValue )
    {
        index = Math.Min( index, Radixes.Length - 1 );
        Done = index < 0;
        if (NotDone)
        {
            Counters[index]++;
            if (Counters[index] >= Radixes[index])
            {
                Counters[index] = 0;
                Next( index - 1 );
            }
        }
    }

    public override string ToString() => "{ " + Counters.JoinAsString( " " ) + " }";
}
