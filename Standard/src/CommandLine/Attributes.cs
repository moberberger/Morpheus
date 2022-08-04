namespace Morpheus.CommandLine;



[AttributeUsage( AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class )]
public class Usage : Attribute
{
    public string UsageText { get; set; }
    public string UsageParamName { get; set; }
    public Usage( string usageString, string usageParamName = "" )
    {
        UsageText = usageString.Trim();
        UsageParamName = usageParamName.Trim();
    }
}




[AttributeUsage( AttributeTargets.Property | AttributeTargets.Field )]
public class Required : Attribute { }






[AttributeUsage( AttributeTargets.Property | AttributeTargets.Field )]
public class ParamName : Attribute
{
    public string Name { get; set; }
    public ParamName( string paramName ) => Name = paramName;
}

[AttributeUsage( AttributeTargets.Property | AttributeTargets.Field )]
public class EnvironmentVariable : Attribute
{
    public string ExplicitName { get; set; }
    public EnvironmentVariable( string explicitName = null ) => ExplicitName = explicitName;
}

/// <summary>
/// A PositionalParameter indicates one of the parameters before the first named parameter
/// </summary>
[AttributeUsage( AttributeTargets.Property | AttributeTargets.Field )]
public class PositionalParameter : Attribute
{
    public int Index { get; set; }
    public PositionalParameter( int index ) => Index = index;
}





[AttributeUsage( AttributeTargets.Class )]
public class AutoUsagePrintout : Attribute { }


[AttributeUsage( AttributeTargets.Class )]
public class EnvironmentVariablePrefix : Attribute
{
    public string Prefix { get; set; }
    public EnvironmentVariablePrefix( string prefix ) => Prefix = prefix;
}


[AttributeUsage( AttributeTargets.Class )]
public class CaseSensitive : Attribute
{
    public bool IsCaseSensitive = false;
    public CaseSensitive( bool isCaseSensitive ) => IsCaseSensitive = isCaseSensitive;
}

