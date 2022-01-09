using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Morpheus.CommandLine
{
    public class Parser
    {
        public string CommandLine { get; set; } = Environment.CommandLine;
        public List<Param> ParamDefinitions { get; private set; } = new();

        protected object workingObject;

        public bool CaseSensitive { get; set; } = false;

        public string UsageTextHeader { get; set; } = Environment.CommandLine;



        public Parser() { }
        public Parser( IEnumerable<Param> paramDefinitions ) =>
            ParamDefinitions = paramDefinitions.Apply( p => p.Parser = this ).ToList();


        public void AddParam( Param p ) => ParamDefinitions.Add( p );


        public void ParamsFromType<T>() => ParamsFromType( typeof( T ) );
        public void ParamsFromType( Type type )
        {
            workingObject = Activator.CreateInstance( type );

            var flags = BindingFlags.Public | BindingFlags.Instance;
            var accessors = type.GetProperties( flags )
                                .Select( prop => new PropertyOrFieldProxy( prop ) )
                            .Union( type.GetFields( flags )
                                .Select( fld => new PropertyOrFieldProxy( fld ) ) );

            var caseSensAttr = type.GetSingleAttribute<CaseSensitive>();
            if (caseSensAttr != null)
                CaseSensitive = caseSensAttr.IsCaseSensitive;

            var usageAttr = type.GetSingleAttribute<Usage>();
            if (usageAttr != null)
                UsageTextHeader = usageAttr.UsageText;

            foreach (var member in accessors)
            {
                var mi = member.MemberInfo;
                var usage = mi.GetSingleAttribute<Usage>();
                if (usage == null) continue;

                Param p = new()
                {
                    Parser = this,
                    UsageText = usage.UsageText ?? "",
                    UsageParamName = usage.UsageParamName ?? "",
                    IsRequired = mi.HasAttribute<Required>(),
                    IsNegatable = mi.HasAttribute<Negatable>(),
                };

                p.Executor = match =>
                {
                    if (member.TheType().IsPrimitive)
                        member.Set( workingObject, match.Value );
                    
                };

                var paramNamesAttr = mi.GetSingleAttribute<ParamNames>();
                if (paramNamesAttr != null)
                    p.Names = paramNamesAttr.Names;
                else
                    p.Name = mi.Name;

                ParamDefinitions.Add( p );
            }
        }



        public IDictionary<string, List<Match>> Parse( string commandLine = null )
        {
            Dictionary<string, List<Match>> matches = new();

            RegexOptions options = RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace;
            if (!CaseSensitive) options |= RegexOptions.IgnoreCase;
            Regex paramRegex = new( @"(?<negated>no)?  (?<name>[^\s=:]+)  [\s =:]*  (?<value>.*)", options );


            var cmdstr = commandLine ?? CommandLine;
            cmdstr = cmdstr.Trim().Replace( " --", " /" ).Replace( " -", " /" );


            var opts = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
            IEnumerable<string> tokens = cmdstr.Split( " /", opts );
            tokens = tokens.Skip( 1 ) // ignore the first token, which should be the executable name
                            .Where( t => t?.Length > 0 ); // in case an empty one slips in

            foreach (var token in tokens) // could be a ToDictionary, but that gets unwieldy
            {
                matches[token] = ParamDefinitions
                    .Select( pdef => new Match( pdef, token ) )
                    .Where( match => match.IsMatch )
                    .ToList();
            }

            return matches;
        }

        public IEnumerable<string> Validate( string commandLine = null ) =>
            Parse( commandLine )
                .Where( kv => kv.Value.Count() > 1 )
                .Select( kv => $"Ambiguous Parameter Match: {kv.Key}\n{kv.Value.JoinAsString( "\n" )}" )
                .Union( Parse( commandLine ?? CommandLine )
                .Where( kv => kv.Value.Count() == 0 )
                .Select( kv => $"Unknown Parameter: {kv.Key}" ) );
        /*
        .Union( ParameterDefinitions01
                .Where( pdef => pdef.IsRequired && Parser( commandLine ?? CommandLine ).Union( )
        */

        public void Execute( PropertyOrFieldProxy member, string commandLine = null ) =>
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



    public class Parser<T> : Parser where T : class, new()
    {
        public T Params { get; private set; } = new T();
        public Parser() => ParamsFromType( typeof( T ) );
    }
}
