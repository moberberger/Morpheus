using System;
using System.Reflection;
using System.Text;

namespace Morpheus
{
    /// <summary>
    /// This is a custom exception dealing with classes that use the <see cref="AImplicitDeserializer"/>
    /// and <see cref="AImplicitSerializer"/> attributes incorrectly.
    /// </summary>
    public class XInvalidImplicitSerializer : Exception
    {
        /// <summary>
        /// The <see cref="Type"/> of object that was being serialized
        /// </summary>
        public Type Type;

        /// <summary>
        /// <see cref="System.Reflection"/> information about the member being serialized
        /// </summary>
        public MemberInfo Member;

        /// <summary>
        /// Some message presumably describing the error
        /// </summary>
        public string Text;

        /// <summary>
        /// Constructor with information about the implicit serializer
        /// </summary>
        /// <param name="_type">The <see cref="Type"/> of object that was being serialized</param>
        /// <param name="_member"><see cref="System.Reflection"/> information about the member being serialized</param>
        /// <param name="_text">Some message presumably describing the error</param>
        public XInvalidImplicitSerializer( Type _type, MemberInfo _member, string _text )
        {
            Type = _type;
            Member = _member;
            Text = _text;
        }

        /// <summary>
        /// Some message presumably describing the error
        /// </summary>
        public override string Message => ToString();

        /// <summary>
        /// Some message presumably describing the error
        /// </summary>
        public override string ToString()
        {
            var s = new StringBuilder();

            if (Type != null)
            {
                s.Append( "For Type " );
                s.Append( Type.ToString() );
                s.Append( ", " );
            }

            if (Member != null)
            {
                s.Append( "Member '" );
                s.Append( Member.Name );
                s.Append( "' " );
            }

            s.Append( "has an implicit surrogate attribute that it shouldn't have, because " );
            s.AppendFormat( Text, Type, Member );

            return s.ToString();
        }
    }
}