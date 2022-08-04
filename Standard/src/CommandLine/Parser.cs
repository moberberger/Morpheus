using System.Reflection;

namespace Morpheus.CommandLine;



public class Parser
{
    #region Outputs

    // public static TextWriter Diag = StreamWriter.Null;
    public static TextWriter Diag = Console.Out;
    public static TextWriter Error = Console.Error;

    #endregion

    public Type ParsedType { get; init; }
    public bool CaseSensitive { get; set; } = false;
    public bool UsageTextOut { get; private set; }
    public string UsageTextHeader { get; set; } = Environment.CommandLine;
    public string EnvironmentVariablePrefix { get; set; } = null;
    public object WorkingObject { get; private set; }

    private List<Param> paramDefCollection { get; } = new List<Param>();
    public IEnumerable<Param> ParamDefinitions => paramDefCollection;
    private PropertyOrFieldProxy parserProxy { get; init; }




    public Parser( Type type )
    {
        ParsedType = type;

        Diag.WriteLine( $"Creating Parameters for Type '{ParsedType.Name}'" );

        CheckForAttribute<Usage>( ParsedType, attr => UsageTextHeader = attr.UsageText );
        CheckForAttribute<AutoUsagePrintout>( ParsedType, attr => AddAutoUsagePrintout() );
        CheckForAttribute<CaseSensitive>( ParsedType, attr => CaseSensitive = attr.IsCaseSensitive );
        CheckForAttribute<EnvironmentVariablePrefix>( ParsedType, attr => EnvironmentVariablePrefix = attr.Prefix );

        foreach (var member in ParsedType.GetMembers().Where( m => m is FieldInfo || m is PropertyInfo ))
        {
            Diag.Write( $"Member '{member.Name}' ({member.GetType().Name}) " );

            PropertyOrFieldProxy proxy = new( member );
            if (proxy.TheType.IsAssignableTo( typeof( Parser ) ))
            {
                Diag.WriteLine( $"PARSER member found: '{proxy.MemberInfo.Name}'" );
                parserProxy = proxy;
            }
            else if (!member.HasAttribute<Usage>())
            {
                Diag.WriteLine( "skipped- no Usage attribute" );
            }
            else
            {
                Param param = new( this, proxy );
                paramDefCollection.Add( param );
            }
        }

        VerifyPositionalParameterIndices();
    }

    private void VerifyPositionalParameterIndices()
    {
        HashSet<int> positions = new();
        int firstOptional = int.MaxValue;
        int lastRequired = int.MinValue;
        foreach (var p in paramDefCollection.Where( p => p.IsPositional ))
        {
            if (positions.Contains( p.PositionalParameterIndex ))
                throw new InvalidOperationException( "Multiple positional parameters found for index: " + p.PositionalParameterIndex );
            positions.Add( p.PositionalParameterIndex );

            if (p.IsRequired)
                lastRequired = Math.Max( lastRequired, p.PositionalParameterIndex );
            else
                firstOptional = Math.Min( firstOptional, p.PositionalParameterIndex );
        }

        if (firstOptional < lastRequired)
            throw new CommandLineException( $"The last required positional param is index {lastRequired}, which is after the first optional positional parameter at index {firstOptional}." );

        int idx = 0;
        while (positions.Count > 0)
        {
            if (!positions.Contains( idx ))
                throw new InvalidOperationException( $"Index position {idx} does not have a corresponding Positional Parameter." );
            positions.Remove( idx++ );
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
            Parser = this,
            Executor = value =>
            {
                UsageTextOut = true;
                Console.WriteLine( this );
            },
        };

        paramDefCollection.Add( p );
    }




















    HashSet<Param> uniqueParamSet;
    string workingValueToken;
    Param workingParameter;

    public object Parse( string[] argv )
    {
        Diag.WriteLine();
        Diag.WriteLine( argv.JoinAsString( "," ) );

        uniqueParamSet = new();
        workingValueToken = "";
        workingParameter = null;

        WorkingObject = Activator.CreateInstance( ParsedType );
        parserProxy?.Set( WorkingObject, this );
        foreach (var pdef in ParamDefinitions.Where( p => p.EnvironmentVariableValue != null ))
            pdef.Execute( pdef.EnvironmentVariableValue );

        for (int i = 0; i < argv.Length; i++)
        {
            var tok = argv[i].RemoveDuplicateWhitespace();
            Diag.Write( $"-- '{tok}'  " );

            Param pp = ParamDefinitions.SingleOrDefault( p => p.PositionalParameterIndex == i );

            if (IsParamName( ref tok ))
            {
                if (pp?.IsRequired ?? false)
                    throw new CommandLineException( $"Parameter '{tok}' found, but required positional parameter {pp.Name} should be in its place." );

                Execute( $"Named: {workingParameter?.Name}" );

                bool isNegated = GetCurParamFromToken( tok );
                bool isBool = workingParameter?.IsBool ?? false;
                if (isBool)
                {
                    workingValueToken = isNegated ? "false" : "true";
                    Execute( $"Boolean: {workingValueToken}" );
                }
            }
            else if (pp != null) // this token should be treated as positional
            {
                workingParameter = pp;
                workingValueToken = tok;
                Execute( $"Positional: {pp.Name}[{i}] " );
            }
            else
            {
                if (workingParameter == null)
                    throw new CommandLineException( $"Found token '{tok}' on the command line when no positional parameter is expected at index {i}." );

                Diag.WriteLine( $"... appended to {workingValueToken}" );
                workingValueToken += " " + tok;
            }
        }

        Execute( $"Final Named: {workingParameter?.UsageParamName}" );

        VerifyRequiredParameters();

        return WorkingObject;
    }


    bool GetCurParamFromToken( string tok )
    {
        workingParameter = ParamDefinitions.SingleOrDefault( p => p.IsMatch( tok ) );
        if (workingParameter == null && tok.ToLower().StartsWith( "no" ))
        {
            workingParameter = ParamDefinitions.SingleOrDefault( p => p.IsMatch( tok[2..] ) );
            if (workingParameter != null)
            {
                if (workingParameter.IsNegatable)
                    return true;
                else
                    throw new InvalidOperationException( $"Found a non-negatable parameter '{workingParameter.Name}' that matches '{tok}'" );
            }
        }
        if (workingParameter == null)
            throw new InvalidOperationException( $"Param name '{tok}' not a valid parameter" );
        return false;
    }


    private void VerifyRequiredParameters()
    {
        foreach (var pdef in ParamDefinitions.Where( pd => pd.IsRequired ))
        {
            if (!uniqueParamSet.Contains( pdef ))
                throw new InvalidOperationException( $"Parameter {pdef.UsageLeftSide} is required but not present on the command line nor in the environment" );
        }
    }

    private void Execute( string message )
    {
        Diag.Write( "\t... " );
        if (workingParameter != null)
        {
            Diag.WriteLine( message );

            if (uniqueParamSet.Contains( workingParameter ))
                throw new InvalidOperationException( $"Cannot have a second command line token '{workingParameter.Name}', '{workingValueToken}' resolve to a previous Param object" );
            uniqueParamSet.Add( workingParameter );

            workingParameter.Execute( workingValueToken );
            workingParameter = null;
        }
        else
        {
            Diag.WriteLine( "Nothing Executed- " + message );
        }

        workingValueToken = "";
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
