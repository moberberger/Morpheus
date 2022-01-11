using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Morpheus.CommandLine
{
    public class Parser : ICollection<Param>
    {
        #region Outputs

        public static TextWriter Diag = Console.Out;
        public static TextWriter Log = Console.Out;
        public static TextWriter Error = Console.Error;
        public static TextWriter Recipe = StreamWriter.Null;

        #endregion


        public string CommandLine { get; set; } = Environment.CommandLine;

        public bool CaseSensitive { get; set; } = false;

        public string UsageTextHeader { get; set; } = Environment.CommandLine;

        public string EnvironmentVariablePrefix { get; set; } = "";

        public object WorkingObject { get; private set; }
        public T Params<T>( string cmdLine = null ) => (T)CreateWorkingObject( cmdLine );

        public IEnumerable<Param> ParamDefinitions { get; private set; } = new List<Param>();

        public IEnumerable<string> ExecuteMessages { get; private set; }



        public Parser() { }
        public Parser( Type t ) => ParamsFromType( t );
        public Parser( IEnumerable<Param> paramDefinitions ) =>
            paramDefinitions.ForEach( param => Add( param ) );



        public Parser ParamsFromType<T>() => ParamsFromType( typeof( T ) );
        public Parser ParamsFromType( Type type )
        {
            Diag.WriteLine( $"Creating Parameters for Type '{type.Name}'" );
            WorkingObject = Activator.CreateInstance( type );

            CheckForAttribute<Usage>( type, attr => UsageTextHeader = attr.UsageText );
            CheckForAttribute<AutoUsagePrintout>( type, attr => AddAutoUsagePrintout() );
            CheckForAttribute<CaseSensitive>( type, attr => CaseSensitive = attr.IsCaseSensitive );
            CheckForAttribute<EnvironmentVariablePrefix>( type, attr => EnvironmentVariablePrefix = attr.Prefix );

            foreach (var member in type.GetMembers())
            {
                Diag.Write( $"Member '{member.Name}' ({member.GetType().Name}) " );

                if (!(member is FieldInfo || member is PropertyInfo))
                {
                    Diag.WriteLine( "skipped- not appropriate" );
                    continue;
                }

                if (!member.HasAttribute<Usage>())
                {
                    Diag.WriteLine( "skipped- no Usage attribute" );
                    continue;
                }

                Diag.Write( "proxy " );
                PropertyOrFieldProxy proxy = new( member );

                Diag.Write( "param " );
                Param param = new( proxy );

                Diag.WriteLine( param.UsageLeftSide );
                Add( param );
            }

            return this;
        }

        private void CheckForAttribute<Tattr>( Type type, Action<Tattr> action )
            where Tattr : Attribute
        {
            Diag.Write( $"Checking for {typeof( Tattr ).Name} attribute: " );
            var attr = type.GetSingleAttribute<Tattr>();
            if (attr != null)
            {
                Diag.WriteLine( "  FOUND  " );
                action( attr );
            }
            else
            {
                Diag.WriteLine( "not found" );
            }
        }

        public void AddAutoUsagePrintout()
        {
            Add( new()
            {
                Names = new[] { "help", "?" },
                UsageText = "Prints This Message",
                Executor = match =>
                {
                    Console.WriteLine( this );
                    return "Usage Text";
                }
            } );
        }

        public Parsed Parse( string commandLine = null )
        {
            if (commandLine != null)
                CommandLine = commandLine;

            Diag.WriteLine( "Parsing: " + CommandLine );

            var cmdstr = CommandLine
                            .Trim()
                            .Replace( " --", " /" )
                            .Replace( " -", " /" );

            Diag.WriteLine( "Normalized Delims: " + cmdstr );


            var opts = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
            var tokens = cmdstr
                .Split( " /", opts )             // split into tokens delimited by " /"
                .Skip( 1 )                       // ignore the first token, which should be the executable name
                .Where( t => t?.Length > 0 );   // in case an empty one slips in

            return new Parsed( this, tokens );
        }

        public object CreateWorkingObject( string cmdLine = null )
        {
            var parsed = Parse( cmdLine );

            Diag.WriteLine();
            Diag.WriteLine( "Validating parsed information" );
            var errors = parsed.Validate();
            if (!errors.IsEmpty())
            {
                Diag.WriteLine( "Errors found." );
                foreach (var error in errors)
                    Error.WriteLine( error );
                return null;
            }

            Diag.WriteLine( "Executing the parsed command line" );
            ExecuteMessages = parsed.Execute().ToList();

            Diag.WriteLine( ExecuteMessages.JoinAsString( "\n" ) );
            Diag.WriteLine();

            return WorkingObject;
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



        #region ICollection<Param>

        public int Count => ParamDefinitions.Count();

        public bool IsReadOnly => false;

        public void Clear() => (ParamDefinitions as ICollection<Param>).Clear();

        public void Add( Param p )
        {
            p.Parser = this;
            (ParamDefinitions as IList<Param>).Add( p );
            Diag.WriteLine( $"Parameter {p.Name} added to parser" );
        }

        public void AddRange( IEnumerable<Param> parameters )
        {
            foreach (var param in parameters)
                Add( param );
        }

        public bool Remove( Param item ) => (ParamDefinitions as ICollection<Param>).Remove( item );

        public bool Contains( Param item ) => ParamDefinitions.Contains( item );

        public void CopyTo( Param[] array, int arrayIndex )
        {
            int destIdx = arrayIndex;
            for (int i = 0; i < Count && destIdx < array.Length; i++)
                array[destIdx++] = ParamDefinitions.ElementAt( i );
        }

        public IEnumerator<Param> GetEnumerator() => ParamDefinitions.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => ParamDefinitions.GetEnumerator();

        #endregion

    }


    public class Parser<T> : Parser where T : class, new()
    {
        public Parser() => ParamsFromType( typeof( T ) );
        public T Params( string cmdLine = null ) => Params<T>();

        public static implicit operator T( Parser<T> parser ) => parser.Params();
    }
}
