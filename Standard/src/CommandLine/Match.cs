using System.Text.RegularExpressions;

namespace Morpheus.CommandLine
{
    public class Match
    {
        public static Regex Regex { get; }
            = new Regex( @"^\s* (?'negated'no)? (?'param'[\w\d\?]+) (\s*=?\s*) (?'value'[\w\d]+)? \s*$",
                            RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace );

        public Param Parameter { get; init; }
        public string Token { get; init; }
        public bool IsNegated { get; private set; }
        public string ParamFound { get; private set; }
        public string DeducedValue { get; private set; }
        public string Value => string.IsNullOrWhiteSpace( DeducedValue ) ? Parameter.DefaultValue : DeducedValue;
        public bool IsMatch => Parameter.Name.StartsWith( ParamFound, !Parameter.Parser.CaseSensitive, null );
        public void Execute() => Parameter.Executor( this );

        public Match( Param parameter, string token ) =>
            (Parameter = parameter, Token = token.Trim())
            .NowUse( Regex.Match( Token ) )
            .With( m => IsNegated = m.Groups["negated"].Value.ToLower() == "no" )
            .With( m => ParamFound = m.Groups["param"].Value )
            .With( m => DeducedValue = m.Groups["value"].Value );
    }
}
