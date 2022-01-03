using System;
using System.Text;

namespace Morpheus.CommandLine
{
    public class Parameter
    {
        public string Name { get; init; } = "Please Initialize Me";
        public string Usage { get; init; } = "TODO: Add Usage Text";
        public bool IsRequired { get; init; }
        public bool IsNegatable { get; init; }
        public string SubparamUsage { get; init; }
        public string DefaultValue { get; init; }
        public Action<Match> Executor { get; init; }
        public Parser Parser { get; internal set; }


        public string UsageLeftSide
            => new StringBuilder()
                .AppendIf( !IsRequired, "[" )
                .Append( Parser.Delimiter.Trim() )
                .AppendIf( IsNegatable, "[no]" )
                .Append( Name )
                .AppendIf( !string.IsNullOrWhiteSpace( SubparamUsage ), $" <{SubparamUsage}>" )
                .AppendIf( !IsRequired, "]" )
                .ToString();


        public override string ToString()
            => $"{UsageLeftSide}   {Usage}";
    }
}
