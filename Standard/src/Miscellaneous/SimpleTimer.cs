namespace Morpheus;

public class SimpleTimer : IDisposable
{
    public string? EndMessage { get; private set; }
    public readonly Func<string>? EndMessageFn;
    public readonly DateTime StartTime = DateTime.Now;
    public SimpleTimer( string? startMessage = null, string? endMessage = null )
    {
        EndMessage = endMessage;
        if (startMessage is not null)
            Console.WriteLine( startMessage );
    }

    public SimpleTimer( string? startMessage, Func<string> endMessageFn )
    {
        EndMessageFn = endMessageFn;
        if (startMessage is not null)
            Console.WriteLine( startMessage );
    }

    public void Done( string? msg = null )
    {
        if (msg is not null) EndMessage = msg;
        Dispose();
    }

    public void Dispose()
    {
        string s;
        if (EndMessageFn is not null)
            s = EndMessageFn();
        else
            s = EndMessage ?? "Duration";

        string msg = $"{s}: {DateTime.Now - StartTime}";
        Console.WriteLine( msg );
    }
}
