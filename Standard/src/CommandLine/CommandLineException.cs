using System;

namespace Morpheus.CommandLine
{
    public class CommandLineException : Exception
    {
        public CommandLineException( string s ) : base( s ) { }
    }
}
