namespace Morpheus;


public class ConsoleRedirect : IDisposable
{
    StringBuilder Output;
    TextWriter Saved;

    public ConsoleRedirect( StringBuilder? output = null )
    {
        Output = output ?? new();
        Saved = Console.Out;
        Console.SetOut( new StringWriter( Output ) );
    }

    public void Dispose() => Console.SetOut( Saved );
    public override string ToString() => Output.ToString();
}
