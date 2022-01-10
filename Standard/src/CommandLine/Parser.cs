using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Parsed = System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<Morpheus.CommandLine.Match>>;


namespace Morpheus.CommandLine
{
    public class Parser
    {
        public string CommandLine { get; set; } = Environment.CommandLine;

        public List<Param> ParamDefinitions { get; private set; } = new();

        public bool CaseSensitive { get; set; } = false;

        public string UsageTextHeader { get; set; } = Environment.CommandLine;


        public object WorkingObject { get; private set; }
        public T Params<T>() => (T)WorkingObject;



        public Parser() { }
        public Parser( Type t ) => ParamsFromType( t );
        public Parser( IEnumerable<Param> paramDefinitions ) =>
            ParamDefinitions = paramDefinitions.Apply( p => p.Parser = this ).ToList();
        public void Add( Param p ) => ParamDefinitions.Add( p );



        public Parser ParamsFromType<T>() => ParamsFromType( typeof( T ) );
        public Parser ParamsFromType( Type type )
        {
            WorkingObject = Activator.CreateInstance( type );

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
                var p = Param.FromType( this, type, member );
                if (p != null)
                    ParamDefinitions.Add( p );
            }

            return this;
        }



        public Parsed Parse( string commandLine = null )
        {
            Parsed matches = new();

            RegexOptions options = RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace;
            if (!CaseSensitive) options |= RegexOptions.IgnoreCase;
            Regex paramRegex = new( @"(?<negated>no)?  (?<name>[^\s=:]+)  [\s =:]*  (?<value>.*)", options );

            if (commandLine != null)
                CommandLine = commandLine;

            var cmdstr = CommandLine
                            .Trim()
                            .Replace( " --", " /" )
                            .Replace( " -", " /" );


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



        public IEnumerable<string> Validate( string cmd ) => Validate( Parse( cmd ) );
        public IEnumerable<string> Validate( Parsed parsed )
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


        public IEnumerable<string> Execute( string cmd ) => Execute( Parse( cmd ) );
        public IEnumerable<string> Execute( Parsed parsed )
        {
            foreach (var kv in parsed.Where( kv => kv.Value.Count == 1 ))
                yield return kv.Value.Single().Execute();
        }


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
            .WithHeader( UsageTextHeader )
            .ToString();
    }


    public class Parser<T> : Parser where T : class, new()
    {
        public List<string> Messages { get; } = new List<string>();

        public Parser() => ParamsFromType( typeof( T ) );
        public T Params( string cmdLine = null )
        {
            Messages.AddRange(
                Execute( cmdLine )
            );
            return (T)WorkingObject;
        }
    }
}
