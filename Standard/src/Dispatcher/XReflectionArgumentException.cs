using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Morpheus
{
    /// <summary>
    /// This is the type of exception thrown by the <see cref="System.Reflection"/>-based Event Handler Discovery
    /// </summary>
    public class XReflectionArgumentException : ArgumentException
    {
        /// <summary>
        /// The <see cref="MethodInfo"/> object that was being analysed when the error was found
        /// </summary>
        public MethodInfo MethodInfo;

        /// <summary>
        /// Construct with a MethodInfo object and a formatted string
        /// </summary>
        /// <param name="_methodInfo">The MethodInfo object causing the exception</param>
        /// <param name="_formatString">Format string, suitable for <see cref="string.Format(string,object)"/></param>
        /// <param name="_params"></param>
        public XReflectionArgumentException( MethodInfo _methodInfo, string _formatString, params object[] _params )
            : base( string.Format( _formatString, _params ) )
        {
            MethodInfo = _methodInfo;
        }

        /// <summary>
        /// Make sure the MethodInfo.Name is in the output string
        /// </summary>
        public override string Message => "Method: [" + MethodInfo.Name + "] ... " + base.Message;
    }
}
