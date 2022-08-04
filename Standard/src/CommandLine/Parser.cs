using System.Reflection;

namespace Morpheus.CommandLine;



public class Parser
{
    #region Outputs

    // public static TextWriter Diag = StreamWriter.Null;
    public static TextWriter Diag = Console.Out;
    public static TextWriter Error = Console.Error;

    #endregion

    public bool CaseSensitive { get; set; } = false;
    public bool UsageTextOut { get; private set; }
    public string UsageTextHeader { get; set; } = Environment.CommandLine;
    public string EnvironmentVariablePrefix { get; set; } = null;
    public object WorkingObject { get; private set; }

    private List<Param> paramDefCollection { get; } = new List<Param>();
    public IEnumerable<Param> ParamDefinitions => paramDefCollection;




    public Parser( Type type )
    {
        Diag.WriteLine( $"Creating Parameters for Type '{type.Name}'" );
        WorkingObject = Activator.CreateInstance( type );

        CheckForAttribute<Usage>( type, attr => UsageTextHeader = attr.UsageText );
        CheckForAttribute<AutoUsagePrintout>( type, attr => AddAutoUsagePrintout() );
        CheckForAttribute<CaseSensitive>( type, attr => CaseSensitive = attr.IsCaseSensitive );
        CheckForAttribute<EnvironmentVariablePrefix>( type, attr => EnvironmentVariablePrefix = attr.Prefix );

        foreach (var member in type.GetMembers().Where( m => m is FieldInfo || m is PropertyInfo ))
        {
            Diag.Write( $"Member '{member.Name}' ({member.GetType().Name}) " );

            PropertyOrFieldProxy proxy = new( member );
            if (proxy.TheType.IsAssignableTo( typeof( Parser ) ))
            {
                Diag.WriteLine( $"PARSER object assigned to '{proxy.MemberInfo.Name}'" );
                proxy.Set( WorkingObject, this );
                continue;
            }

            if (!member.HasAttribute<Usage>())
            {
                Diag.WriteLine( "skipped- no Usage attribute" );
                continue;
            }

            Param param = new( this, proxy );
            paramDefCollection.Add( param );
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
        Param p = new()
        {
            Name = "help",
            UsageText = "Prints This Message",
            Executor = value =>
            {
                UsageTextOut = true;
                Console.WriteLine( this );
            },
            Parser = this,
        };

        paramDefCollection.Add( p );
    }




















    HashSet<Param> UniqueParamSet;
    string working;
    Param curParam;

    public object Parse( string[] argv )
    {
        Diag.WriteLine();
        Diag.WriteLine( argv.JoinAsString( "," ) );

        UniqueParamSet = new();
        working = "";
        curParam = null;

        for (int i = 0; i < argv.Length; i++)
        {
            var tok = argv[i].RemoveDuplicateWhitespace();
            Diag.Write( $"Token: '{tok}'  " );

            Param pp = ParamDefinitions.SingleOrDefault( p => p.PositionalParameterIndex == i );
            if (pp != null) // this token should be treated as positional
            {
                curParam = pp;
                working = tok;
                Execute( $"Positional: {pp.Name} " );
            }
            else if (IsParamName( ref tok ))
            {
                Execute( $"Named: {curParam?.Name}" );

                bool isNegated = GetCurParamFromToken( tok );
                bool isNegatable = curParam?.IsNegatable ?? false;

                if (isNegated)
                    working = "false";
                else if (isNegatable)
                    working = "true";
            }
            else
            {
                Diag.WriteLine( $"... appended to {working}" );
                working += " " + tok;
            }
        }

        Execute( $"Final Named: {curParam?.UsageParamName}" );

        VerifyRequiredParameters();

        return WorkingObject;
    }


    bool GetCurParamFromToken( string tok )
    {
        curParam = ParamDefinitions.SingleOrDefault( p => p.IsMatch( tok ) );
        if (curParam == null && tok.ToLower().StartsWith( "no" ))
        {
            curParam = ParamDefinitions.SingleOrDefault( p => p.IsMatch( tok[2..] ) );
            if (curParam != null)
            {
                if (curParam.IsNegatable)
                    return true;
                else
                    throw new InvalidOperationException( $"Found a non-negatable parameter '{curParam.Name}' that matches '{tok}'" );
            }
        }
        if (curParam == null)
            throw new InvalidOperationException( $"Param name '{tok}' not a valid parameter" );
        return false;
    }


    private void VerifyRequiredParameters()
    {
        foreach (var pdef in ParamDefinitions.Where( pd => pd.IsRequired ))
        {
            if (!UniqueParamSet.Contains( pdef ))
                throw new InvalidOperationException( $"Parameter {pdef.UsageLeftSide} is required but not present on the command line nor in the environment" );
        }
    }

    private void Execute( string message )
    {
        Diag.WriteLine( $"\n*** {curParam?.Name} >>{working}<<  " );
        if (curParam != null)
        {
            Diag.WriteLine( message );

            if (UniqueParamSet.Contains( curParam ))
                throw new InvalidOperationException( $"Cannot have a second command line token '{curParam.Name}', '{working}' resolve to a previous Param object" );
            UniqueParamSet.Add( curParam );

            curParam.Execute( working );
            curParam = null;
        }
        working = "";
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

}


public class Parser<T> : Parser where T : class, new()
{
    public Parser() : base( typeof( T ) ) { }
    public new T Parse( string[] argv ) => (T)base.Parse( argv );
}
