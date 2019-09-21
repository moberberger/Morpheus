using System;

namespace Morpheus
{
    /// <summary>
    /// This is a custom exception dealing with serialization.
    /// </summary>
    public class XSerializationError : Exception
    {
        /// <summary>
        /// Construct the exception with a message
        /// </summary>
        /// <param name="_message">Presumably a message describing the serialization error</param>
        public XSerializationError( string _message )
            : base( _message )
        {
        }
    }
}