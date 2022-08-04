using System;

namespace Morpheus.CommandLine
{
    public class CommandLineException : Exception
    {
        public CommandLineException( string s ) : base( s ) { }
        public override string ToString()
        {
            return "\n\n" + Message + "\n";
        }
    }
}
