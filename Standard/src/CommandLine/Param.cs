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
        public Func<Match, string> Executor { get; set; }


        public bool IsMatch( string paramFound )
            => Names.Contains( name => name.StartsWith( paramFound, !Parser.CaseSensitive, null ) );


        public static Param FromType( Type type, PropertyOrFieldProxy member )
        {
            var mi = member.MemberInfo;
            var t = member.TheType;

            var usage = mi.GetSingleAttribute<Usage>();
            if (usage == null) return null;

            Param p = new()
            {
                UsageText = usage.UsageText ?? "",
                UsageParamName = usage.UsageParamName ?? "",
                IsRequired = mi.HasAttribute<Required>(),
                IsNegatable = mi.HasAttribute<Negatable>(),
            };
            p.Executor = match => p.SetWithReflection( member, match );

            if (p.IsNegatable && t != typeof( bool ))
                throw new CommandLineException( $"{mi.Name} is declared as Negatable but the underlying type isn't bool: {t}" );

            var paramNamesAttr = mi.GetSingleAttribute<ParamNames>();
            if (paramNamesAttr != null)
                p.Names = paramNamesAttr.Names;
            else
                p.Name = mi.Name;

            return p;
        }

        private string SetWithReflection( PropertyOrFieldProxy member, Match match )
        {
            object val;

            if (member.TheType == typeof( bool ))
            {
                val = !match.IsNegated;
            }
            else
            {
                val = match.Value ?? "";
                if (val.Equals( "" ))
                    val = Activator.CreateInstance( member.TheType );
                else
                    val = Convert.ChangeType( val, member.TheType );
            }

            member.Set( Parser.WorkingObject, val );
            return $"{member.MemberInfo.Name} = '{val}'";
        }


        public string UsageLeftSide =>
            new StringBuilder()
                .AppendIf( !IsRequired, "[" )
                .Append( "-" )
                .AppendIf( IsNegatable, "[no]" )
                .Append( Name )
                .AppendIf( !string.IsNullOrWhiteSpace( UsageParamName ), $" <{UsageParamName}>" )
                .AppendIf( !IsRequired, "]" )
                .ToString();


        public override string ToString() => $"{UsageLeftSide}\t{UsageText}";
    }
}
