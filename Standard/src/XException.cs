using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morpheus
{
    /// <summary>
    /// A base override of Exception that works with Morpheus Code Snippets and distinguishes Morpheus exceptions from .NET exceptions.
    /// </summary>
    public class XException : Exception
    {
        /// <summary>
        /// Just contains a simple string as a message
        /// </summary>
        /// <param name="_message">The message. May be null.</param>
        public XException( string _message = null )
            : base( _message )
        {
        }

        /// <summary>
        /// Construct with a simple message and an "inner exception"
        /// </summary>
        /// <param name="_message">The simple messsage</param>
        /// <param name="_baseException">The inner exception</param>
        public XException( string _message, Exception _baseException )
            : base( _message, _baseException )
        {
        }

        /// <summary>
        /// A string.format style exception with no inner exception
        /// </summary>
        /// <param name="_format">The format for "string.Format"</param>
        /// <param name="_params">The parameters for "string.Format"</param>
        public XException( string _format, params object[] _params )
            : base( string.Format( _format, _params ) )
        {
        }

        /// <summary>
        /// A string.format style exception with an inner exception
        /// </summary>
        /// <param name="_innerException">The inner exception</param>
        /// <param name="_format">The format for "string.Format"</param>
        /// <param name="_params">The parameters for "string.Format"</param>
        public XException( Exception _innerException, string _format, params object[] _params )
            : base( string.Format( _format, _params ), _innerException )
        {
        }
    }

}
