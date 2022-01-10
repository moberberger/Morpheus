using System.Text.RegularExpressions;

namespace Morpheus.CommandLine
{
    public class Match
    {
        public Param Param { get; init; }
        public string Token { get; init; }
        public bool IsNegated { get; private set; }
        public string ParamInCmdline { get; private set; }
        public string DeducedValue { get; private set; }
        public string Value => string.IsNullOrWhiteSpace( DeducedValue ) ? Param.DefaultValue : DeducedValue;
        public bool IsMatch => Param.IsMatch( ParamInCmdline );
        public string Execute() => Param.Executor( this );

        public Match( Param parameter, string token )
        {
            Param = parameter;
            Token = token;

            RegexOptions rgxOpt = RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace;
            if (!Param.Parser.CaseSensitive)
                rgxOpt |= RegexOptions.IgnoreCase;

            var rgxStr = @"(?<name>[^\s=:]+)  [\s =:]*  (?<value>.*)";
            if (Param.IsNegatable)
                rgxStr = "(?<negated>no)?" + rgxStr;

            var paramRegex = new Regex( rgxStr, rgxOpt );
            var m = paramRegex.Match( Token );

            IsNegated = m.Groups["negated"]?.Value?.Length > 0;
            ParamInCmdline = m.Groups["name"].Value;
            DeducedValue = m.Groups["value"]?.Value ?? "";
        }

        public override string ToString() => $"'{Token}' :: {Param.Name}= '{Value}'";
    }
}
