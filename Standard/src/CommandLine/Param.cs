using System;
using System.Linq;
using System.Text;

using Morpheus;

namespace Morpheus.CommandLine
{
    public class Param
    {
        public Parser Parser { get; internal set; }

        public string[] Names { get; set; } = new string[1];
        public string Name { get => Names[0]; set => Names[0] = value; }

        public string UsageText { get; init; } = "TODO: Add Usage Text";
        public string UsageParamName { get; init; }
        public string DefaultValue { get; init; }
        public bool IsRequired { get; init; }
        public bool IsNegatable { get; init; }
        public Action<Match> Executor { get; set; }


        public string UsageLeftSide =>
            new StringBuilder()
                .AppendIf( !IsRequired, "[" )
                .Append( "-" )
                .AppendIf( IsNegatable, "[no]" )
                .Append( Name )
                .AppendIf( !string.IsNullOrWhiteSpace( UsageParamName ), $" <{UsageParamName}>" )
                .AppendIf( !IsRequired, "]" )
                .ToString();

        internal bool IsMatch( string paramFound )
        {
            foreach (var name in Names)
                if (name.StartsWith( paramFound, !Parser.CaseSensitive, null ))
                    return true;
            return false;
        }

        public override string ToString() => $"{UsageLeftSide}\t{UsageText}";


    }

}
