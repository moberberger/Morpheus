namespace Morpheus;
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

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
