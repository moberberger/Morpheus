using System;
using System.Xml;

namespace Morpheus.Standard.UnitTests.Serialization
{
    /// <summary>
    /// This class contains many members that will cause errors if treated as an implicit serializer
    /// </summary>
    public abstract class CImplicitSerializerErrors
    {
        public int ImNotAMethod = 0;

        public static void StaticMethod( XmlNode _node )
        {
        }

        public void NonStaticMethod( XmlNode _node )
        {
        }

        protected abstract void AbstractMethod( XmlNode _node );

        static CImplicitSerializerErrors()
        {
        }

        public static CImplicitSerializerErrors_SuperClass ReturnASuperclass( XmlNode _node ) => null;

        private int SerializerReturningBadValueType( XmlNode _node ) => 1;

        private static bool DeserializerWithoutXml( CWorkingObject _object ) => true;
    }

    public class CImplicitSerializerErrors_SuperClass : CImplicitSerializerErrors
    {
        protected override void AbstractMethod( XmlNode _node ) => throw new Exception( "The method or operation is not implemented." );
    }
}