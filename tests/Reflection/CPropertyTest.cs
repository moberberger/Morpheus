using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace Morpheus.Standard.UnitTests
{
    [TestClass]
    public class CPropertyTest
    {
        public class CTestObject
        {
            public int X;
            public string m_string;
        }


        [TestMethod]
        public void BasicConstructionTest()
        {
            var prop = new CProperty<int>();

            Assert.AreEqual<int>( 0, prop, "Default Value" );

            prop = 5;
            Assert.AreEqual( 5, prop.Value, "Error Message" );

            Assert.AreEqual( "5", prop.ToString(), "ToString on property" );
        }


        [TestMethod]
        public void EnumPropertyTest()
        {
            var prop = new CProperty<EDispatchMode>();

            Assert.AreEqual( EDispatchMode.NotAssigned, prop.Value, "Default Value" );

            prop.Value = EDispatchMode.Inline;
            Assert.AreEqual<EDispatchMode>( EDispatchMode.Inline, prop, "After Assignment" );
        }

        [TestMethod]
        public void UserClassPropertyTest()
        {
            var prop = new CProperty<CTestObject>();

            var obj = new CTestObject() { X = 11, m_string = "hello" };
            prop.Value = obj;

            Assert.AreEqual( 11, prop.Value.X, "X should equal 11" );
            Assert.AreEqual( "hello", prop.Value.m_string, "String should be hello" );

            obj.X++;
            Assert.AreEqual( 12, prop.Value.X, "X should be 12" );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void NullObjectTest() => CPropertyBase.RegisterChangeHandler( null, ( _obj, _name, _prev, _new ) => { } );



        public class CTestObjectWithProperties
        {
            private readonly CProperty<int> m_age = new CProperty<int>();
            public int Age
            {
                get => m_age.Value;
                set => m_age.Value = value;
            }
            private readonly CProperty<string> m_name = new CProperty<string>();
            public string Name
            {
                get => m_name.Value;
                set => m_name.Value = value;
            }
            private readonly CProperty<EDispatchMode> m_mode = new CProperty<EDispatchMode>();
            public EDispatchMode Mode
            {
                get => m_mode.Value;
                set => m_mode.Value = value;
            }
        }

        [TestMethod]
        public void ObjectEventWiringTest()
        {
            var obj = new CTestObjectWithProperties();
            var count = 0;
            CPropertyBase.RegisterChangeHandler( obj, ( _obj, _name, _prev, _new ) =>
                {
                    Console.WriteLine( "{0} changed from {1} to {2}", _name, _prev, _new );

                    count++;
                } );

            obj.Age = 45;
            Assert.AreEqual( 1, count, "After Age" );

            obj.Name = "Michael";
            Assert.AreEqual( 2, count, "After Name" );

            obj.Age = 45; // no change, shouldn't trigger event
            Assert.AreEqual( 2, count, "After Age re-assigned with same value" );

            obj.Mode = EDispatchMode.Inline;
            Assert.AreEqual( 3, count, "After ENUM set" );

            obj.Name = null;
            Assert.AreEqual( 4, count, "After Name set to null" );

            obj.Name = null;
            Assert.AreEqual( 4, count, "After name RE-set to NULL (no change)" );
        }


        private int m_count = 0;

        [TestMethod]
        public void ObjectRemoveWiringTest()
        {
            var obj = new CTestObjectWithProperties();
            m_count = 0;

            Assert.AreEqual( 0, m_count, "Count starts at zero" );

            CPropertyBase.RegisterChangeHandler( obj, My_Handler );
            CPropertyBase.RegisterChangeHandler( obj, My_Handler ); // Add it a 2nd time for double increment
            obj.Age = 45;
            Assert.AreEqual( 2, m_count, "After setting age" );

            CPropertyBase.RemoveChangeHandler( obj, My_Handler ); // Remove one of them
            obj.Name = "Homer";
            Assert.AreEqual( 3, m_count, "After setting name" );

            CPropertyBase.RemoveChangeHandler( obj, My_Handler ); // Remove the last one
            obj.Mode = EDispatchMode.Threadpool;
            Assert.AreEqual( 3, m_count, "After setting Mode" );

            CPropertyBase.RemoveChangeHandler( obj, My_Handler ); // Duplicate remove- make sure its OK
            obj.Age = 11;
            Assert.AreEqual( 3, m_count, "After setting Age again" );
        }


        private void My_Handler( object _object, string _name, object _previous, object _new ) => m_count++;



        private static System.Xml.XmlDocument GetXml( CTestObjectWithProperties _object )
        {
            _object.Age = 45;
            _object.Name = "Michael";
            _object.Mode = EDispatchMode.Threadpool;

            var ser = new CSerializer();
            ser.Context.FixM_ = true;

            var doc = ser.Serialize( _object );
            Console.WriteLine( XmlExtensions.GetFormattedString( doc ) );
            return doc;
        }

        [TestMethod]
        public void SerializationTest()
        {
            var obj = new CTestObjectWithProperties();
            var doc = GetXml( obj );

            var root = doc.DocumentElement;
            Assert.AreEqual( 3, root.ChildNodes.Count, "Child Node Count" );

            var nName = root.SelectSingleNode( "Name" );
            Assert.AreEqual( obj.Name, nName.InnerText, "Name Wrong" );

            var nAge = root.SelectSingleNode( "Age" );
            Assert.AreEqual( obj.Age.ToString(), nAge.InnerText, "Age Wrong" );

            var nMode = root.SelectSingleNode( "Mode" );
            Assert.AreEqual( obj.Mode.ToString(), nMode.InnerText, "Mode Wrong" );
        }

        [TestMethod]
        public void DeSerializationTest()
        {
            var refObj = new CTestObjectWithProperties();
            var doc = GetXml( refObj );

            var deser = new CDeserializer();
            deser.Context.FixM_ = true;

            var obj = deser.Deserialize<CTestObjectWithProperties>( doc );
            Assert.IsNotNull( obj, "Nothing Deserialized" );

            Assert.AreEqual( refObj.Age, obj.Age, "Age Wrong" );
            Assert.AreEqual( refObj.Name, obj.Name, "Name Wrong" );
            Assert.AreEqual( refObj.Mode, obj.Mode, "Mode Wrong" );

            obj.Name = "Homer";
            Assert.AreNotEqual( refObj.Name, obj.Name, "Name should have changed in only one object" );
        }


    }
}
