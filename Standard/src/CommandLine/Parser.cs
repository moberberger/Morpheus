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

        public static TextWriter Diag = StreamWriter.Null;
        // public static TextWriter Diag = Console.Out;
        public static TextWriter Error = Console.Error;

        #endregion


        public bool CaseSensitive { get; set; } = false;

        public string UsageTextHeader { get; set; } = Environment.CommandLine;

        public string EnvironmentVariablePrefix { get; set; } = "";

        public object WorkingObject { get; private set; }

        public IEnumerable<Param> ParamDefinitions { get; private set; } = new List<Param>();

        public IEnumerable<string> ExecuteMessages { get; private set; }

        public bool UsageTextOut { get; private set; }


        public Parser() { }
        public Parser( Type t ) => ParamsFromType( t );
        public Parser( IEnumerable<Param> paramDefinitions ) =>
            paramDefinitions.ForEach( param => Add( param ) );



        public Parser ParamsFromType<T>() => ParamsFromType( typeof( T ) );
        public Parser ParamsFromType( Type type )
        {
            try
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

                    SetParserObjectIfPresent( member );

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
            catch (Exception ex)
            {
                Diag.WriteLine( ex );
                Console.Error.WriteLine( Diag.ToString() );
                throw new InvalidOperationException( "Exception thrown in parser." );
            }
        }

        private void SetParserObjectIfPresent( MemberInfo member )
        {
            var mfi = member as FieldInfo;
            var pfi = member as PropertyInfo;
            if (mfi != null)
            {
                if (mfi.FieldType.IsAssignableTo( typeof( Parser ) ))
                {
                    Diag.WriteLine( $"PARSER object assigned to field {member.Name}." );
                    mfi.SetValue( WorkingObject, this );
                }
            }
            else if (pfi != null)
            {
                if (pfi.PropertyType.IsAssignableTo( typeof( Parser ) ))
                {
                    Diag.WriteLine( $"PARSER object assigned to property {member.Name}." );
                    pfi.SetValue( WorkingObject, this );
                }
            }
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
                    UsageTextOut = true;
                    Console.WriteLine( this );
                    return "Usage Text";
                }
            } );
        }

        public Parsed Parse( string[] argv )
        {
            Diag.WriteLine();
            Diag.WriteLine( "Normalized Delims: " + argv.JoinAsString( "," ) );

            List<string> tokens = new();
            string working = "";

            for (int i = 0; i < argv.Length; i++)
            {
                var tok = argv[i].RemoveDuplicateWhitespace();
                Diag.Write( $"Token: '{tok}'  " );


                Param pp = ParamDefinitions.SingleOrDefault( p => p.PositionalParameterIndex == i );
                if (pp != null) // this token should be treated as positional
                {
                    Diag.WriteLine( $"Treated as Positional: {pp.Name}" );
                    pp.Executor( new Match( pp, tok ) );
                    continue;
                }

                if (IsParamName( ref tok ))
                {
                    Diag.WriteLine( "Param Name." );

                    if (!string.IsNullOrWhiteSpace( working ))
                    {
                        Diag.WriteLine( $"\t{working}     added to token list" );
                        tokens.Add( working );
                    }
                    working = tok;
                }
                else
                {
                    Diag.WriteLine( $"... appended to {working}" );

                    working += " " + tok;
                }
            }

            if (!string.IsNullOrWhiteSpace( working ))
            {
                Diag.WriteLine( $"\t{working} --- added to token list" );
                tokens.Add( working );
            }

            Diag.WriteLine( $"" );
            Diag.WriteLine( $"The following tokens will be processed:" );
            Diag.WriteLine( $"{tokens.JoinAsString( "\n" )}" );

            return new Parsed( this, tokens.ToArray() );
        }

        bool IsParamName( ref string token )
        {
            if (token.Length > 0)
            {
                if (token.StartsWith( "--" ))
                {
                    token = token[2..];
                    return true;
                }
                else if (token[0] == '-' || token[0] == '/')
                {
                    token = token[1..];
                    return true;
                }
            }

            return false;
        }

        public Parsed Parse( string commandLine )
        {
            Diag.WriteLine();
            Diag.WriteLine( "Parsing: " + commandLine );

            var cmdstr = commandLine
                            .Trim()
                            .Replace( " --", " /" )
                            .Replace( " -", " /" );

            Diag.WriteLine( "Normalized Delims: " + cmdstr );


            var opts = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
            var tokens = cmdstr
                .Split( " /", opts )             // split into tokens delimited by " /"
                .Skip( 1 )                       // ignore the first token, which should be the executable name
                .Where( t => t?.Length > 0 );   // in case an empty one slips in
            throw new NotImplementedException();
            return new Parsed( this, tokens.JoinAsString() );
        }

        public object CreateWorkingObject( string[] argv )
        {
            try
            {
                var parsed = Parse( argv );

                Diag.WriteLine();
                Diag.WriteLine( "Validating parsed information" );
                var errors = parsed.Validate();
                if (!errors.IsEmpty())
                {
                    Diag.WriteLine( "Errors found." );
                    foreach (var error in errors)
                        Error.WriteLine( error );
                    return WorkingObject;
                }

                Diag.WriteLine( "Executing the parsed command line" );
                ExecuteMessages = parsed.Execute().ToList();

                if (ExecuteMessages.Count() > 0)
                {
                    Diag.WriteLine(
                        new TextGrid( ExecuteMessages.Select( line => line.Split( '=' ) ) )
                        .WithBorders( TextGrid.NullBorder )
                        .WithColumnPadding( 1 )
                        .WithHorizontalAlign( GridAlignments.Left )
                    );
                }
                else
                {
                    Diag.WriteLine( $"No Executed Parameters" );
                }

                Diag.WriteLine();

                return WorkingObject;
            }
            catch (Exception ex)
            {
                Diag.WriteLine( ex );
                Console.WriteLine( Diag.ToString() );
                throw new InvalidOperationException( "Exception thrown in parser." );
            }
        }



        public override string ToString() =>
            new TextGrid
            (
                UsageTextHeader,
                ParamDefinitions
                    .Where( pdef => !pdef.IsPositional )
                    .Select( pdef => pdef.ToString().Split( "\t" ) )
            )
            .WithBorders( TextGrid.Single )
            .WithHorizontalAlign( GridAlignments.Left )
            .WithHeaderAlign( GridAlignments.Center )
            .WithColumnPadding( 1 )
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
        public new T Parse( string[] argv ) => (T)CreateWorkingObject( argv );
    }
}
