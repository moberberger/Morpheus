namespace Morpheus;

public class SimpleTimer : IDisposable
{
    public readonly string? EndMessage;
    public readonly DateTime StartTime = DateTime.Now;
    public SimpleTimer( string? startMessage = null, string? endMessage = null )
    {
        EndMessage = endMessage;
        if (startMessage is not null)
            Console.WriteLine( startMessage );
    }

    public void Dispose()
    {
        var s = EndMessage ?? "Duration";
        string msg = $"{s}: {DateTime.Now - StartTime}";
        Console.WriteLine( msg );
    }
}
