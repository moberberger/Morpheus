using System;

namespace Morpheus
{
    /// <summary>
    /// This is a custom exception dealing with Deserialization.
    /// </summary>
    public class XDeserializationError : Exception
    {
        /// <summary>
        /// Construct the exception with a message
        /// </summary>
        /// <param name="_message">Presumably a message describing the deserialization error</param>
        public XDeserializationError( string _message )
            : base( _message )
        {
        }
    }
}