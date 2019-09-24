using Morpheus.Serialization;
using System;
using System.Xml;

namespace Morpheus.Standard.UnitTests.Serialization
{
    public class CStdImplicitSurrogate : CStdBaseObject
    {
        public CStdImplicitSurrogate()
        {
            STATUS = ETestStatus.NONE;

            Name = "Default";
            Age = 666;
        }

        [AImplicitSerializer]
        public bool MySerialize( CSerializer _serializer, XmlNode _node )
        {
            XmlExtensions.AddAttribute( _node, "NAME", Name );
            XmlExtensions.AddAttribute( _node, "AGE", Age );

            STATUS = ETestStatus.IMPLICIT_SERIALIZER;
            return true;
        }

        [AImplicitDeserializer]
        private static bool CreateFromXml( XmlElement _node, CWorkingObject _object, CDeserializer _framework )
        {
            var x = _object.GetExistingOrCreateNew<CStdImplicitSurrogate>();

            x.Name = XmlExtensions.GetAttributeValue( _node, "NAME" );
            x.Age = int.Parse( XmlExtensions.GetAttributeValue( _node, "AGE" ) );

            STATUS = ETestStatus.IMPLICIT_DESERIALIZER;
            return true;
        }

        private static bool IncorrectDeserializer1( CDeserializer _framework, XmlNode _node ) => true;

        private static bool IncorrectDeserializer2( CDeserializer _framework, ref object _object ) => false;

        public void IncorrectSerializer( CFramework _framework )
        {
        }
    }


    public class CIncompleteImplicitSurrogate : CStdBaseObject
    {
        public CIncompleteImplicitSurrogate()
        {
            STATUS = ETestStatus.NONE;

            Name = "Default";
            Age = 666;
        }

        [AImplicitSerializer]
        public bool MySerialize( CSerializer _framework, XmlNode _node )
        {
            XmlExtensions.AddAttribute( _node, "Incomplete", "Yes" );

            STATUS = ETestStatus.IMPLICIT_SERIALIZER_INCOMPLETE;
            return false;
        }

        [AImplicitDeserializer]
        private static bool CreateFromXml( CWorkingObject _object, XmlElement _node, CDeserializer _framework )
        {
            if (_object.WorkingObject == null)
                _object.Set( new CIncompleteImplicitSurrogate() );

            if ("Yes" != XmlExtensions.GetAttributeValue( _node, "Incomplete" ))
            {
                throw new InvalidOperationException(
                    "Expected an attribute named 'Incomplete' and that attrribute to have the value 'Yes'." );
            }

            STATUS = ETestStatus.IMPLICIT_DESERIALIZER_INCOMPLETE;
            return false;
        }
    }


    public class CVoidImplicitSurrogate : CStdBaseObject
    {
        public CVoidImplicitSurrogate()
        {
            STATUS = ETestStatus.NONE;

            Name = "Default";
            Age = 666;
        }

        [AImplicitSerializer]
        public void MySerialize( CSerializer _framework, XmlNode _node )
        {
            XmlExtensions.AddAttribute( _node, "NAME", Name );
            XmlExtensions.AddAttribute( _node, "AGE", Age );

            STATUS = ETestStatus.IMPLICIT_SERIALIZER_VOID;
        }

        [AImplicitDeserializer]
        private static void CreateFromXml( XmlElement _node, CWorkingObject _object, CDeserializer _framework )
        {
            var x = new CVoidImplicitSurrogate();
            _object.Set( x );

            x.Name = XmlExtensions.GetAttributeValue( _node, "NAME" );
            x.Age = int.Parse( XmlExtensions.GetAttributeValue( _node, "AGE" ) );

            STATUS = ETestStatus.IMPLICIT_DESERIALIZER_VOID;
        }
    }
}