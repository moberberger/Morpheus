#nullable disable

namespace Morpheus;


/// <summary>
/// This class is designed to take as input a string containing many lines of data and a
/// Regular Expression containing named groups/captures. The Regex is used to create records
/// out of the data, where each field in each record is named. The names found in the REGEX
/// captures are looked for as members on the Type specified when parsing. For each record,
/// a new object of the specified Type is created, and its members are populated from the
/// named captures found in the Regex.
/// </summary>
public class ParseStringToObjects
{
    private readonly Regex m_regex;

    /// <summary>
    /// Called every time the utility could not parse a value into a field/property.
    /// </summary>
    public event Action<Exception, string, string> OnParseException;

    /// <summary>
    /// Construct with a pre-formed REGEX
    /// </summary>
    /// <param name="_regex"></param>
    public ParseStringToObjects( Regex _regex )
    {
        m_regex = _regex;
    }

    /// <summary>
    /// Construct with the parameters that would be used to create a new REGEX object
    /// </summary>
    /// <param name="_pattern"></param>
    /// <param name="_options"></param>
    public ParseStringToObjects( string _pattern, RegexOptions _options = RegexOptions.None )
    {
        m_regex = new Regex( _pattern, _options );
    }

    /// <summary>
    /// Given a string, and using the REGEX specified at construction, break the string down
    /// into records (Matches) and then populate objects of the specified Type using member
    /// names found it the named captures.
    /// </summary>
    /// <typeparam name="T">The Type of the Returned records</typeparam>
    /// <param name="_stringData">The string containing the records of data</param>
    /// <returns>
    /// An enumeration of type T containing the successfully parsed records from the string
    /// </returns>
    public IEnumerable<T> ParseToObjects<T>( string _stringData )
        where T : class, new()
    {
        var matches = m_regex.Matches( _stringData );

        foreach (Match match in matches)
        {
            var obj = new T();

            for (var i = 1; i < match.Groups.Count; i++)
            {
                var group = match.Groups[i];
                var name = m_regex.GroupNameFromNumber( i );
                if (group == null || string.IsNullOrWhiteSpace( name )) continue;

                var setter = GetValueSetter<T>( name, out var memberType );
                if (setter == null) continue;

                try
                {
                    var val = Convert.ChangeType( group.Value, memberType );
                    setter( obj, val );
                }
                catch (Exception ex)
                {
                    if (OnParseException != null) // only to determine if we throw or notify
                        OnParseException?.Invoke( ex, name, group.Value ); // safe call
                    else
                        throw;
                }
            }

            yield return obj;
        }
    }

    /// <summary>
    /// Internal- Can be used to cache "setter" functions.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_name"></param>
    /// <param name="_memberType"></param>
    /// <returns></returns>
    private static Action<object, object> GetValueSetter<T>( string _name, out Type _memberType )
        where T : class, new()
    {
        // var setter = CGetterSetter.GetCached( typeof( T ), _name ); _memberType =
        // setter?.MemberType; return (setter == null) ? default( Action
        // <object, object> ) : setter.Set;
        throw new NotImplementedException();
    }


    /// <summary>
    /// Parse a string into an enumeration of strongly-typed records
    /// </summary>
    /// <typeparam name="T">The Type of the Returned records</typeparam>
    /// <param name="_stringData">The string containing the records of data</param>
    /// <param name="_regex">The Regex used to parse / validate the records</param>
    /// <returns>
    /// An enumeration of type T containing the successfully parsed records from the string
    /// </returns>
    public static IEnumerable<T> Parse<T>( Regex _regex, string _stringData )
        where T : class, new() => new ParseStringToObjects( _regex ).ParseToObjects<T>( _stringData );

    /// <summary>
    /// Parse a string into an enumeration of strongly-typed records
    /// </summary>
    /// <typeparam name="T">The Type of the Returned records</typeparam>
    /// <param name="_stringData">The string containing the records of data</param>
    /// <param name="_regex">The Regex used to parse / validate the records</param>
    /// <param name="_regexOptions">The options used to create the Regex object</param>
    /// <returns>
    /// An enumeration of type T containing the successfully parsed records from the string
    /// </returns>
    public static IEnumerable<T> Parse<T>( string _regex, RegexOptions _regexOptions, string _stringData )
        where T : class, new() => new ParseStringToObjects( _regex, _regexOptions ).ParseToObjects<T>( _stringData );
}
