namespace Morpheus.CommandLine;

#if false

public class Match
{
    public string val;
    public bool isNegated;

    public Match( string val, bool isNegated )
    {
        this.val = val;
        this.isNegated = isNegated;
    }
}

public class Match
{
    public Param Param { get; init; }
    public string Token { get; init; }
    public string ParamNameFromCmdline { get; private set; }
    public string ValueFromCmdline { get; private set; }
    public bool IsNegated { get; private set; }
    public bool IsMatch => !Param.IsPositional && Param.IsMatch( ParamNameFromCmdline );
    public string Execute() => Param.Executor( this );

    public Match( Param parameter, string token )
    {
        Param = parameter;
        Token = token;

        if (parameter.IsPositional)
        {
            IsNegated = false;
            ParamNameFromCmdline = parameter.Name;
            ValueFromCmdline = token;
        }
        else
        {
            ggg();
        }
    }

    private static Match ggg( Param param, string token )
    {
        RegexOptions regexOpt = RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace;
        if (true)
            regexOpt |= RegexOptions.IgnoreCase;

        var regexStr = @"(?<name>[^\s=:]+)  [\s =:]*  (?<value>.*)";
        if (param.IsNegatable)
            regexStr = "(?<negated>no)?" + regexStr;

        var paramRegex = new Regex( regexStr, regexOpt );
        var m = paramRegex.Match( token );
                
        bool isNegated = m.Groups["negated"]?.Value?.Length > 0;
        string name = m.Groups["name"].Value;
        string val = m.Groups["value"]?.Value ?? "";

        if (param.IsMatch(name))
        {
            var m = new Match( param );

        }
    }

    public override string ToString() => $"'{Token}' :: {Param.Name}= '{ValueFromCmdline}'";
}
#endif
