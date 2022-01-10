using System;

namespace Morpheus.CommandLine
{
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
    public class ParamNames : Attribute
    {
        public string[] Names { get; set; }
        public ParamNames( params string[] paramNames ) => Names = paramNames;
    }



    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field )]
    public class Negatable : Attribute { }


    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field )]
    public class Required : Attribute { }





    [AttributeUsage( AttributeTargets.Class )]
    public class CommandLineParams : Attribute { }


    [AttributeUsage( AttributeTargets.Class )]
    public class AutoUsagePrintout : Attribute { }


    [AttributeUsage( AttributeTargets.Class )]
    public class CaseSensitive : Attribute
    {
        public bool IsCaseSensitive = false;
        public CaseSensitive( bool isCaseSensitive ) => IsCaseSensitive = isCaseSensitive;
    }

}