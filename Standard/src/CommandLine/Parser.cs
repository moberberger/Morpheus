using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morpheus.CommandLine
{
    public class Parser
    {
        public string Delimiter { get; set; } = " -";
        public bool CaseSensitive { get; set; } = false;
        public string CommandLine { get; set; } = Environment.CommandLine;
        public List<Parameter> ParameterDefinitions { get; private set; }


        public Parser() { }
        public Parser( IEnumerable<Parameter> parameterDefinitions )
            => ParameterDefinitions = parameterDefinitions
                    .Apply( p => p.Parser = this )
                    .ToList();


        public void Param( string name,
                            Action<Match> executor,
                            string usage = "",
                            string subparamUsage = "",
                            string defaultValue = "",
                            bool isRequired = false,
                            bool isNegatable = false )
            => (ParameterDefinitions ??= new List<Parameter>())
                .Add( new Parameter()
                {
                    Parser = this,
                    Name = name,
                    Executor = executor,
                    Usage = usage,
                    SubparamUsage = subparamUsage,
                    DefaultValue = defaultValue,
                    IsNegatable = isNegatable,
                    IsRequired = isRequired
                } );


        public IDictionary<string, IEnumerable<Match>> Parse( string commandLine = null )
            => (commandLine ?? CommandLine)
                .Split( Delimiter )
                .Skip( 1 )
                .ToDictionary( token => token,
                               token => ParameterDefinitions
                                           .Select( pdef => new Match( pdef, token ) )
                                           .Where( match => match.IsMatch ) );


        public IEnumerable<string> Validate( string commandLine = null )
            => Parse( commandLine ?? CommandLine )
                .Where( kv => kv.Value.Count() > 1 )
                .Select( kv => $"Ambiguous Parameter Match: {kv.Key}\n{kv.Value.JoinAsString( "\n" )}" )
                .Union( Parse( commandLine ?? CommandLine )
                .Where( kv => kv.Value.Count() == 0 )
                .Select( kv => $"Unknown Parameter: {kv.Key}" ) );
        /*
        .Union( ParameterDefinitions
                .Where( pdef => pdef.IsRequired && Parser( commandLine ?? CommandLine ).Union( )
        */


        public override string ToString()
        => new TextGrid
            (
                "ProtoMake.exe USAGE",
                ParameterDefinitions.Select
                (
                    pdef => pdef.ToString().Split( "\t" )
                )
            )
            .WithBorders( TextGrid.Single )
            .WithHorizontalAlign( TextGrid.Alignments.Left )
            .WithColumnPadding( 1 )
            .ToString();

        public void Execute( string commandLine = null )
        {
            Parse( commandLine ?? CommandLine )
                .Apply( kv => kv.Value.Single().Execute() )
                .ForEach();
        }
    }
}
