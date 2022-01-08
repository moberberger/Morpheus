﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Morpheus.CommandLine
{
    public class Parser
    {
        public string Delimiter { get; set; } = " -";
        public bool CaseSensitive { get; set; } = false;
        public string CommandLine { get; set; } = Environment.CommandLine;
        public List<Param> ParamDefinitions { get; private set; }




        public Parser() { }
        public Parser( IEnumerable<Param> paramDefinitions ) =>
            ParamDefinitions = paramDefinitions.Apply( p => p.Parser = this ).ToList();


        public void Param( string name,
                            Action<Match> executor,
                            string usage = "",
                            string subparamUsage = "",
                            string defaultValue = "",
                            bool isRequired = false,
                            bool isNegatable = false ) =>
            (ParamDefinitions ??= new List<Param>())
                .Add( new Param()
                {
                    Parser = this,
                    Name = name,
                    Executor = executor,
                    UsageText = usage,
                    SubparamUsage = subparamUsage,
                    DefaultValue = defaultValue,
                    IsNegatable = isNegatable,
                    IsRequired = isRequired
                } );


        public IDictionary<string, IEnumerable<Match>> Parse( string commandLine = null ) =>
            (commandLine ?? CommandLine)
                .Split( Delimiter )
                .Skip( 1 )
                .ToDictionary( token => token,
                                token => ParamDefinitions
                                            .Select( pdef => new Match( pdef, token ) )
                                            .Where( match => match.IsMatch ) );


        public IEnumerable<string> Validate( string commandLine = null ) =>
            Parse( commandLine ?? CommandLine )
                .Where( kv => kv.Value.Count() > 1 )
                .Select( kv => $"Ambiguous Parameter Match: {kv.Key}\n{kv.Value.JoinAsString( "\n" )}" )
                .Union( Parse( commandLine ?? CommandLine )
                .Where( kv => kv.Value.Count() == 0 )
                .Select( kv => $"Unknown Parameter: {kv.Key}" ) );
        /*
        .Union( ParameterDefinitions01
                .Where( pdef => pdef.IsRequired && Parser( commandLine ?? CommandLine ).Union( )
        */

        public void Execute( string commandLine = null ) =>
            Parse( commandLine ?? CommandLine )
                .ForEach( kv => kv.Value.Single().Execute() );

        public override string ToString() =>
            new TextGrid
            (
                "ProtoMake.exe USAGE",
                ParamDefinitions.Select( pdef => pdef.ToString().Split( "\t" ) )
            )
            .WithBorders( TextGrid.Single )
            .WithHorizontalAlign( GridAlignments.Left )
            .WithHeaderAlign( GridAlignments.Center )
            .WithColumnPadding( 1 )
            .ToString();
    }
}