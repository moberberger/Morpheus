using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Morpheus.CommandLine
{
    public class Parser : ICollection<Param>
    {
        public string CommandLine { get; set; } = Environment.CommandLine;

        public bool CaseSensitive { get; set; } = false;

        public string UsageTextHeader { get; set; } = Environment.CommandLine;

        public object WorkingObject { get; private set; }
        public T Params<T>( string cmdLine = null ) => (T)SetWorkingObject( cmdLine );

        public IEnumerable<Param> ParamDefinitions { get; private set; } = new List<Param>();

        public IEnumerable<string> ExecuteMessages { get; private set; }



        public Parser() { }
        public Parser( Type t ) => ParamsFromType( t );
        public Parser( IEnumerable<Param> paramDefinitions ) =>
            paramDefinitions.ForEach( param => Add( param ) );



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

            var autoUsageAttr = type.GetSingleAttribute<AutoUsagePrintout>();
            if (autoUsageAttr != null)
                AddAutoUsagePrintout();

            foreach (var member in accessors.Where( member => member.MemberInfo.HasAttribute<Usage>() ))
            {
                var p = new Param( type, member );
                Add( p );
            }

            return this;
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

            var cmdstr = CommandLine
                            .Trim()
                            .Replace( " --", " /" )
                            .Replace( " -", " /" );


            var opts = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
            var tokens = cmdstr
                .Split( " /", opts )             // split into tokens delimited by " /"
                .Skip( 1 )                       // ignore the first token, which should be the executable name
                .Where( t => t?.Length > 0 );   // in case an empty one slips in

            return new Parsed( this, tokens );
        }

        public object SetWorkingObject( string cmdLine = null )
        {
            var parsed = Parse( cmdLine );

            var errors = parsed.Validate();
            if (!errors.IsEmpty())
            {
                foreach (var error in errors)
                    Console.Error.WriteLine( error );
                return null;
            }

            ExecuteMessages = parsed.Execute().ToList();
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
        public T Params( string cmdLine = null ) => (T)SetWorkingObject( cmdLine );
    }
}
