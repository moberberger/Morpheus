using System;

namespace Morpheus
{
    /// <summary>
    /// Thrown when an element refers to a "reference element", but that reference element has not been deserialized yet.
    /// </summary>
    public class XUnknownReference : Exception
    {
        /// <summary>
        /// Construct the exception with a message
        /// </summary>
        /// <param name="_message">Presumably a message describing which reference was unknown</param>
        public XUnknownReference( string _message )
            : base( _message )
        {
        }
    }
}