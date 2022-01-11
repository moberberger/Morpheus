using System;
using System.Collections.Generic;
using System.Linq;

namespace Morpheus.CommandLine
{
    public class Parsed
    {
        Parser parser;
        Dictionary<string, List<Match>> parsed = new();

        public Parsed( Parser parser, IEnumerable<string> tokens )
        {
            this.parser = parser;

            AddEnvironmentVariables();

            AddTokens( tokens );
        }

        private void AddEnvironmentVariables()
        {
            Parser.Diag.WriteLine();
            Parser.Diag.WriteLine( "Finding environment variables" );

            var fromEnv = Environment.GetEnvironmentVariables();
            var envVars = new Dictionary<string, string>();

            foreach (System.Collections.DictionaryEntry kv in fromEnv)
            {
                var key = kv.Key.ToString().ToLower();
                var val = fromEnv[kv.Key]?.ToString() ?? "";
                envVars[key] = val;
                Parser.Diag.WriteLine( $"[{key}] {val}" );
            }

            Parser.Diag.WriteLine();
            Parser.Diag.WriteLine( "Resolving appropriate variables" );

            foreach (var pdef in parser.ParamDefinitions)
            {
                var varName = pdef.ResolvedEnvironmentVariableName;
                if (varName != null && envVars.TryGetValue( varName, out string val ))
                {
                    Parser.Diag.WriteLine( $"{varName} == {val}" );
                    if (val?.Length > 0)
                        AddEnvironmentVariable( pdef, varName, val );
                }
            }
        }

        private Match AddEnvironmentVariable( Param pdef, string variable, string value )
        {
            var token = "%" + variable + "=" + value;
            var match = new Match( pdef, token );

            (parsed[token] = new List<Match>())
                .Add( match );

            return match;
        }

        private void AddTokens( IEnumerable<string> tokens )
        {
            Parser.Diag.WriteLine();
            Parser.Diag.WriteLine( "Adding Tokens" );

            foreach (var token in tokens) // could be a ToDictionary, but that gets unwieldy
            {
                var list = parser.ParamDefinitions
                    .Select( pdef => new Match( pdef, token ) )
                    .Where( match => match.IsMatch )
                .ToList();

                parsed[token] = list;

                var paramNames = list.Select( m => m.Param.Name ).JoinAsString( ", " );
                Parser.Diag.WriteLine( $"Token [{token}] added: [{parsed[token].Count}] {paramNames}" );
            }
        }


        public IEnumerable<string> Validate()
        {
            foreach (var problem in parsed.Where( kv => kv.Value.Count == 0 ))
                yield return $":UNK: {problem.Key} There were no matching parameter definitions";

            foreach (var problem in parsed.Where( kv => kv.Value.Count > 1 ))
                yield return $":DUP: {problem.Key} There were {problem.Value.Count} matching parameter definitions";

            var required = parser.ParamDefinitions.Where( pdef => pdef.IsRequired ).ToHashSet();
            foreach (var goodOne in parsed.Where( kv => kv.Value.Count == 1 ))
                required.Remove( goodOne.Value.Single().Param );

            foreach (var notFound in required)
                yield return $":PNF: {notFound.Name} This required parameter was not found";
        }

        public IEnumerable<string> Execute()
        {
            foreach (var kv in parsed.Where( kv => kv.Value.Count == 1 ))
                yield return kv.Value.Single().Execute();
        }
    }
}
