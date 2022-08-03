using System;
using System.Linq;
using System.Text;

using Morpheus;

namespace Morpheus.CommandLine
{
    public class Param
    {
        const int NO_POSITION = int.MaxValue;

        public Parser Parser { get; internal set; }

        public string[] Names { get; set; } = new string[1];
        public string Name { get => Names[0]; set => Names[0] = value; }

        public string UsageText { get; init; } = "TODO: Add Usage Text";
        public string UsageParamName { get; init; } = "";
        public string DefaultValue { get; init; } = "";
        public string EnvironmentVariableName { get; init; } = null;
        public bool IsRequired { get; init; }
        public bool IsNegatable { get; init; }
        public bool IsPositional => PositionalParameterIndex < NO_POSITION;
        public int PositionalParameterIndex { get; private set; } = NO_POSITION;
        public Func<Match, string> Executor { get; set; }

        public string ResolvedEnvironmentVariableName =>
            (EnvironmentVariableName?.Length > 0 ? EnvironmentVariableName :
             Parser.EnvironmentVariablePrefix?.Length > 0 ? Parser.EnvironmentVariablePrefix + Name :
             null)?.ToLower();

        public bool IsMatch( string nameInQuestion ) =>
            Names.Contains( name =>
                name.StartsWith( nameInQuestion, !Parser.CaseSensitive, null ) );

        public Param() { }
        public Param( PropertyOrFieldProxy proxy )
        {
            var mi = proxy.MemberInfo;
            var t = proxy.TheType;

            var usage = mi.GetSingleAttribute<Usage>() ??
                throw new ArgumentException( $"Member '{mi.Name}' doesn't have a 'Usage' attribute." );

            UsageText = usage.UsageText ?? "";
            UsageParamName = usage.UsageParamName ?? "";
            Executor = match => SetWithReflection( proxy, match );

            var unnamed = mi.GetSingleAttribute<PositionalParameter>();
            if (unnamed != null)
            {
                IsRequired = true;
                IsNegatable = false;
                EnvironmentVariableName = "";
                PositionalParameterIndex = unnamed.Index;
                Parser.Diag.Write( $"Position:{unnamed.Index} " );
            }
            else
            {
                IsRequired = mi.HasAttribute<Required>();
                IsNegatable = (t == typeof( bool ));
                EnvironmentVariableName = mi.GetSingleAttribute<EnvironmentVariable>()?.VariableName;
            }

            var paramNamesAttr = mi.GetSingleAttribute<ParamNames>();
            if (paramNamesAttr != null)
                Names = paramNamesAttr.Names;
            else
                Name = mi.Name;
        }

        private string SetWithReflection( PropertyOrFieldProxy proxy, Match match )
        {
            object val;

            if (proxy.TheType == typeof( bool ))
            {
                val = !match.IsNegated;
            }
            else
            {
                val = match.Value ?? "";
                if (proxy.TheType != typeof( string ))
                {
                    if (val.Equals( "" ))
                        val = Activator.CreateInstance( proxy.TheType );
                    else
                        val = Convert.ChangeType( val, proxy.TheType );
                }
            }

            proxy.Set( Parser.WorkingObject, val );
            return $"{proxy.MemberInfo.Name} = '{val}'";
        }


        public string UsageLeftSide =>
            new StringBuilder()
                .AppendIf( !IsRequired, "[" )
                .AppendIf( IsPositional, "<" )
                .AppendIf( !IsPositional, "-" )
                .AppendIf( IsNegatable, "[no]" )
                .Append( Name )
                .AppendIf( !string.IsNullOrWhiteSpace( UsageParamName ), $" <{UsageParamName}>" )
                .AppendIf( !IsRequired, "]" )
                .AppendIf( IsPositional, ">" )
                .ToString();


        public override string ToString() => $"{UsageLeftSide}\t{UsageText}";
    }
}
