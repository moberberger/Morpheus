using System;
using System.Xml;


namespace Morpheus.Standard.UnitTests.Serialization
{
    public class CStdExternalSurrogate : IExternalSurrogate
    {
        public bool Serialize( object _object, Type _useType, XmlElement _parentNode, CSerializer _framework )
        {
            var x = (CStdBaseObject) _object;

            CXmlHelper.AddAttribute( _parentNode, "NAME", x.Name );
            CXmlHelper.AddAttribute( _parentNode, "AGE", x.Age );

            CStdBaseObject.STATUS = ETestStatus.EXTERNAL_SERIALIZER;
            return true;
        }

        public bool Deserialize( CWorkingObject _object, XmlElement _node, CDeserializer _framework )
        {
            if (_object.WorkingObject == null)
                _object.Set( new CStdBaseObject() );

            var x = _object.WorkingObject as CStdBaseObject;

            x.Name = CXmlHelper.GetAttributeValue( _node, "NAME" );
            x.Age = int.Parse( CXmlHelper.GetAttributeValue( _node, "AGE" ) );

            CStdBaseObject.STATUS = ETestStatus.EXTERNAL_DESERIALIZER;
            return true;
        }
    }

    public class CIncompleteExternalSurrogate : IExternalSurrogate
    {
        public bool Serialize( object _object, Type _useType, XmlElement _parentNode, CSerializer _framework )
        {
            CStdBaseObject.STATUS = ETestStatus.EXTERNAL_SERIALIZER_INCOMPLETE;
            return false;
        }

        public bool Deserialize( CWorkingObject _object, XmlElement _node, CDeserializer _framework )
        {
            CStdBaseObject.STATUS = ETestStatus.EXTERNAL_DESERIALIZER_INCOMPLETE;
            return false;
        }
    }
}