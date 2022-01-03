using System.Text.RegularExpressions;

namespace Morpheus.CommandLine
{
    public class Match
    {
        public static Regex Regex { get; }
            = new Regex( @"^\s* (?'negated'no)? (?'param'[\w\d]+) (\s*=\s*|\s+) (?'value'[\w\d]+)? \s*$",
                RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace );
        public Parameter Parameter { get; init; }
        public string Token { get; init; }
        public bool IsNegated { get; init; }
        public string ParamFound { get; init; }
        public string DeducedValue { get; init; }
        public string Value => string.IsNullOrWhiteSpace( DeducedValue ) ? Parameter.DefaultValue : DeducedValue;
        public bool IsMatch => Parameter.Name.StartsWith( ParamFound, !Parameter.Parser.CaseSensitive, null );
        public static bool IsSeparator( char ch ) => char.IsWhiteSpace( ch ) || ch == '=';
        public void Execute() => Parameter.Executor( this );

        public Match( Parameter parameter, string token )
        {
            Parameter = parameter;
            Token = token.Trim();

            string working = Token;
            if (Parameter.IsNegatable && working.ToLower().StartsWith( "no" ))
            {
                working = working[2..];
                IsNegated = true;
            }

            int idx = Token.FirstIndexOf( ch => IsSeparator( ch ) );
            if (idx < 0)
            {
                ParamFound = working;
                DeducedValue = "";
            }
            else
            {
                ParamFound = working[..idx];
                working = working[(idx + 1)..];

                int idx2 = working.FirstIndexOf( ch => !IsSeparator( ch ) );
                DeducedValue = working[idx2..];
            }
        }
    }
}
