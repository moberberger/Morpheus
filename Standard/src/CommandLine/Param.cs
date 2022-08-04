using System.Reflection;

namespace Morpheus.CommandLine;

public class Param
{
    TextWriter Diag => Parser.Diag;

    public Parser Parser { get; internal set; }
    public PropertyOrFieldProxy Proxy { get; init; }
    public Action<string> Executor { private get; set; }

    public string Name { get; init; }
    public string UsageText { get; init; }
    public string UsageParamName { get; init; }
    public bool IsRequired { get; init; }
    public bool IsNegatable { get; init; }
    public bool IsBool => Proxy?.TheType == typeof( bool );


    public int PositionalParameterIndex { get; init; } = NO_POSITION;
    public string EnvironmentVariableName { get; private set; }
    public string EnvironmentVariableValue { get; private set; }



    const int NO_POSITION = int.MaxValue;
    public bool IsPositional => PositionalParameterIndex != NO_POSITION;



    public bool IsMatch( string nameInQuestion ) =>
            Name.StartsWith( nameInQuestion, !Parser.CaseSensitive, null );


    public Param() { }
    public Param( Parser parser, PropertyOrFieldProxy proxy )
    {
        Parser = parser;
        Proxy = proxy;

        var mi = Proxy.MemberInfo;
        var usage = mi.GetSingleAttribute<Usage>() ??
            throw new ArgumentException( $"Member '{mi.Name}' doesn't have a 'Usage' attribute." );

        Name = mi.GetSingleAttribute<ParamName>()?.Name ?? mi.Name;
        UsageText = usage.UsageText ?? "";
        UsageParamName = usage.UsageParamName ?? "";
        IsRequired = mi.HasAttribute<Required>();
        IsNegatable = (Proxy.TheType == typeof( bool ));
        EnvironmentVariableName = EnvironmentVariableValue = null; // handle below if needed
        Diag.Write( $"{Name}: Req:{IsRequired} Negatable:{IsNegatable} " );

        var positional = mi.GetSingleAttribute<PositionalParameter>();
        if (positional != null)
        {
            PositionalParameterIndex = positional.Index;
            Diag.WriteLine( $"Positional:{positional.Index} " );
        }
        else
        {
            PositionalParameterIndex = NO_POSITION;
            SetEnvironmentData( mi );
            Diag.WriteLine( $" Named" );
        }

        Executor = SetWithProxy;
    }

    void SetEnvironmentData( MemberInfo member )
    {
        var envAttr = member.GetSingleAttribute<EnvironmentVariable>();
        if (envAttr != null)
        {
            EnvironmentVariableName = envAttr.ExplicitName;
            Diag.Write( "Explicit " );
        }
        else if (Parser.EnvironmentVariablePrefix != null)
        {
            EnvironmentVariableName = Parser.EnvironmentVariablePrefix + member.Name;
            Diag.Write( "Implicit " );
        }

        if (EnvironmentVariableName != null)
        {
            EnvironmentVariableValue = Environment.GetEnvironmentVariable( EnvironmentVariableName );
            Diag.Write( $"'{EnvironmentVariableName}' = '{EnvironmentVariableValue ?? "null"}'" );
        }
        else
        {
            Diag.Write( "No Environment Variable" );
        }
    }


    public void SetWithProxy( string val )
    {
        object obj = Convert.ChangeType( val, Proxy.TheType );
        Proxy.Set( Parser.WorkingObject, obj );
        Diag.WriteLine( $"{Proxy.MemberInfo.Name} = '{obj}'" );
    }


    public string UsageLeftSide
    {
        get
        {
            var s = new StringBuilder();
            if (!IsRequired) s.Append( '[' );
            if (IsPositional) s.Append( '<' );
            else s.Append( '-' );
            if (IsNegatable) s.Append( "[no]" );

            s.Append( Name );

            if (!IsRequired) s.Append( ']' );
            if (IsPositional) s.Append( '>' );

            return s.ToString();
        }
    }

    internal void Execute( string tok )
    {
        Diag.WriteLine( $"Executing {Name} = '{tok}'" );
        Executor( tok );
    }

    public override string ToString() => $"{UsageLeftSide}\t{UsageText}";
}
