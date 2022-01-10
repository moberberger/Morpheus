using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Morpheus;

namespace Morpheus.CommandLine
{
    public class Parsed
    {
        Parser parser;
        Dictionary<string, List<Match>> parsed = new();
        IEnumerable<Param> ParamDefinitions => parser.ParamDefinitions;

        public Parsed( Parser parser, IEnumerable<string> tokens )
        {
            this.parser = parser;

            foreach (var token in tokens) // could be a ToDictionary, but that gets unwieldy
            {
                parsed[token] = parser.ParamDefinitions
                    .Select( pdef => new Match( pdef, token ) )
                    .Where( match => match.IsMatch )
                    .ToList();
            }
        }

        public IEnumerable<string> Validate()
        {
            foreach (var problem in parsed.Where( kv => kv.Value.Count == 0 ))
                yield return $":UNK: {problem.Key} There were no matching parameter definitions";

            foreach (var problem in parsed.Where( kv => kv.Value.Count > 1 ))
                yield return $":DUP: {problem.Key} There were {problem.Value.Count} matching parameter definitions";

            var required = ParamDefinitions.Where( pdef => pdef.IsRequired ).ToHashSet();
            foreach (var problem in parsed.Where( kv => kv.Value.Count == 1 ))
                required.Remove( problem.Value.Single().Param );

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
