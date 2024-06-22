namespace Morpheus;


/// <summary>
/// A simple wrapper class around some number of other textwriters that will make sure all output that goes to one writer will
/// go to all writers
/// </summary>
/// <remarks>
/// 
/// var outFile = File.CreateText( @"\t.txt" );
/// 
/// Console.SetOut( new CTextWriterSplitter( Console.Out, outFile ) );
///
/// </remarks>
public class TextWriterSplitter : TextWriter
{
    /// <summary>
    /// The <see cref="TextWriter"/> list that receives all output
    /// </summary>
    private readonly IEnumerable<TextWriter> m_writers;


    /// <summary>
    /// A new splitter class to send output to whatever TextWriters are specified in the constructor
    /// </summary>
    /// <param name="_writers">Any number of TextWriters, all of which will receive output</param>
    public TextWriterSplitter( params TextWriter[] _writers )
    {
        m_writers = _writers?.ToList() ?? throw new ArgumentNullException( string.Format( "Must specify non-null list of TextWriters" ) );
    }

    /// <summary>
    /// Use the first textwriter's encoding unless there are no text writers; then use default but it doesn't really matter
    /// </summary>
    public override Encoding Encoding => m_writers?.FirstOrDefault()?.Encoding ?? Encoding.Default;

    /// <summary>
    /// The only method that must be implemented- all other methods in the base implementation use this at the end
    /// </summary>
    /// <param name="_char">A character to write</param>
    public override void Write( char value )
    {
        foreach (var writer in m_writers)
            writer.Write( value );
    }

    public override void Write( string? value )
    {
        foreach (var writer in m_writers)
            writer.Write( value );
    }
}