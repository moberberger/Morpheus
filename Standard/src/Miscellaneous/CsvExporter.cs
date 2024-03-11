namespace Morpheus;

public static class CsvExporter
{
    public static string ToCsv<T>( this IEnumerable<T> values, int maxLen = -1 ) where T : class
    {
        var sb = new StringBuilder();
        var props = typeof( T ).GetProperties();
        var header = string.Join( ",", props.Select( p => p.Name ) );
        sb.AppendLine( header );

        foreach (var value in values)
        {
            var line = props
                .Select( p => p.GetValue( value ) ?? "" )
                .Select( v => EscapeForCsv( v, maxLen ) )
                .JoinAsString( "," );

            sb.AppendLine( line );
        }

        return sb.ToString();
    }

    public static string EscapeForCsv( object inputObject, int maxLen )
    {
        string input = inputObject.ToString()
            ?? throw new WhyIsThisNullException( "inputObject.ToString() returned null" );

        if (maxLen > 0 && input.Length > maxLen)
            input = input[..maxLen];

        if (input.Contains( "," ) || input.Contains( "\"" ) || input.Contains( "\n" ) || input.Contains( "\r" ))
        {
            input = input.Replace( "\"", "\"\"" );
            input = $"\"{input}\"";
        }

        return input;
    }
}
