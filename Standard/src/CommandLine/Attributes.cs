using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morpheus.CommandLine
{
    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field )]
    public class Usage : Attribute
    {
        public string UsageString { get; set; }
        public string UsageParamName { get; set; }
        public Usage( string usageString, string usageParamName = "" )
        {
            UsageString = usageString.Trim();
            UsageParamName = usageParamName.Trim();
        }
    }


    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field )]
    public class ParamNames : Attribute
    {
        public string[] Names { get; set; }
        public ParamNames( string paramName ) => Names = new string[] { paramName };
        public ParamNames( params string[] paramNames ) => Names = paramNames;
    }


    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field )]
    public class Negatable : Attribute { }


    [AttributeUsage( AttributeTargets.Class )]
    public class CommandLineParams : Attribute { }


    [AttributeUsage( AttributeTargets.Class )]
    public class CaseSensitive : Attribute
    {
        public bool IsCaseSensitive = false;
        public CaseSensitive( bool isCaseSensitive ) => IsCaseSensitive = isCaseSensitive;
    }


    [AttributeUsage( AttributeTargets.Class )]
    public class Delimiters : Attribute
    {
        public string[] Delims { get; set; }
        public Delimiters( string delim ) => Delims = new string[] { delim };
        public Delimiters( params string[] delims ) => Delims = delims;
    }
}
