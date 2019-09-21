using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Linq;
using Morpheus;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Morpheus.Standard.UnitTests
{
    [TestClass]
    [TestCategory( "Reflection" )]
    public class MemberProxyTests
    {
        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void TestNullConstructorParameter_Field()
        {
            FieldInfo fi = null;
            var _ = new PropertyOrFieldProxy( fi );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void TestNullConstructorParameter_Property()
        {
            PropertyInfo pi = null;
            var _ = new PropertyOrFieldProxy( pi );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void TestNullConstructorParameter_Method()
        {
            MethodInfo mi = null;
            var _ = new MethodProxy( mi );
        }







        class Data
        {
            public int fieldPrimitive;
            public string fieldObject;
            public double propertyPrimitive { get; set; }
            public Data propertyObject { get; set; }

            public string GetPrimitiveValue( bool _doFieldNotProperty ) => _doFieldNotProperty ? fieldPrimitive.ToString() : propertyPrimitive.ToString();
            public string GetFieldObject() => fieldObject;
            public void InitFieldPrimitive() => fieldPrimitive = 69;
        }


        [TestMethod]
        public void TestFieldPrimitiveGet()
        {
            var d = new Data { fieldPrimitive = 7 };
            var fi = typeof( Data ).GetField( "fieldPrimitive" );
            var proxy = new PropertyOrFieldProxy( fi );
            var gotten = proxy.Get( d );

            Assert.AreEqual( d.fieldPrimitive, gotten );
        }

        [TestMethod]
        public void TestFieldPrimitiveSet()
        {
            int newVal = 69;

            var d = new Data { fieldPrimitive = 7 };
            var fi = typeof( Data ).GetField( "fieldPrimitive" );
            var proxy = new PropertyOrFieldProxy( fi );
            proxy.Set( d, newVal );

            Assert.AreEqual( newVal, d.fieldPrimitive );
        }


        [TestMethod]
        public void TestFieldObjectGet()
        {
            var d = new Data { fieldObject = "Hi" };
            var fi = typeof( Data ).GetField( "fieldObject" );
            var proxy = new PropertyOrFieldProxy( fi );
            var gotten = proxy.Get( d );

            Assert.AreEqual( d.fieldObject, gotten );
        }

        [TestMethod]
        public void TestFieldObjectSet()
        {
            string newVal = "Bye";

            var d = new Data { fieldObject = "Hi" };
            var fi = typeof( Data ).GetField( "fieldObject" );
            var proxy = new PropertyOrFieldProxy( fi );
            proxy.Set( d, newVal );

            Assert.AreEqual( newVal, d.fieldObject );
        }


        [TestMethod]
        public void TestCreateAccessors()
        {
            string newVal = "Bye";

            var d = new Data { fieldObject = "Hi" };
            var fi = typeof( Data ).GetField( "fieldObject" );
            (var getter, var setter) = PropertyOrFieldProxy.CreateAccessors( fi );

            var gotten = getter( d );
            Assert.AreEqual( "Hi", gotten );

            setter( d, newVal );
            Assert.AreEqual( newVal, d.fieldObject );
        }









        [TestMethod]
        public void TestPropertyPrimitiveGet()
        {
            var d = new Data { propertyPrimitive = Math.PI };
            var fi = typeof( Data ).GetProperty( "propertyPrimitive" );
            var proxy = new PropertyOrFieldProxy( fi );
            var gotten = proxy.Get( d );

            Assert.AreEqual( d.propertyPrimitive, gotten );
        }

        [TestMethod]
        public void TestPropertyPrimitiveSet()
        {
            double newVal = Math.E;

            var d = new Data { propertyPrimitive = Math.PI };
            var fi = typeof( Data ).GetProperty( "propertyPrimitive" );
            var proxy = new PropertyOrFieldProxy( fi );
            proxy.Set( d, newVal );

            Assert.AreEqual( newVal, d.propertyPrimitive );
        }


        [TestMethod]
        public void TestPropertyObjectGet()
        {
            Data firstVal = new Data();

            var d = new Data { propertyObject = firstVal };
            var fi = typeof( Data ).GetProperty( "propertyObject" );
            var proxy = new PropertyOrFieldProxy( fi );
            var gotten = proxy.Get( d );

            Assert.AreEqual( d.propertyObject, gotten );
        }

        [TestMethod]
        public void TestPropertyObjectSet()
        {
            Data firstVal = new Data();
            Data secondVal = new Data();

            var d = new Data { propertyObject = firstVal };
            var fi = typeof( Data ).GetProperty( "propertyObject" );
            var proxy = new PropertyOrFieldProxy( fi );
            proxy.Set( d, secondVal );

            Assert.AreEqual( secondVal, d.propertyObject );
        }



        [TestMethod]
        public void TestPropertyHelper()
        {
            var d = new Data { propertyPrimitive = Math.E };
            var proxy = new PropertyOrFieldProxy<Data>( _x => _x.propertyPrimitive );
            var gotten = proxy.Get( d );
            Assert.AreEqual( d.propertyPrimitive, gotten );
        }

        [TestMethod]
        public void TestFieldHelper()
        {
            var d = new Data { fieldPrimitive = 42 };
            var proxy = new PropertyOrFieldProxy<Data>( _x => _x.fieldPrimitive );
            var gotten = proxy.Get( d );
            Assert.AreEqual( d.fieldPrimitive, gotten );
        }




        [TestMethod]
        public void TestMethodProxy_BoolParam()
        {
            var d = new Data { fieldPrimitive = 42, propertyPrimitive = 3.14 };
            var mi = typeof( Data ).GetMethod( "GetPrimitiveValue" );
            var proxy = new MethodProxy( mi );

            var s = proxy.Invoke( d, new object[] { true } );
            Assert.AreEqual( "42", s, "Bad Conversion" );

            s = proxy.Invoke( d, new object[] { false } );
            Assert.AreEqual( "3.14", s, "Bad Conversion" );
        }

        [TestMethod]
        public void TestMethodProxy_NoParams()
        {
            var d = new Data { fieldObject = "Hi" };
            var mi = typeof( Data ).GetMethod( "GetFieldObject" );
            var proxy = new MethodProxy( mi );

            var s = proxy.Invoke( d, new object[0] );
            Assert.AreEqual( "Hi", s, "Bad Conversion" );
        }

        [TestMethod]
        public void TestMethodProxy_VoidFn()
        {
            var d = new Data();
            var mi = typeof( Data ).GetMethod( "InitFieldPrimitive" );
            var proxy = new MethodProxy( mi );

            Assert.AreEqual( 0, d.fieldPrimitive, "Should be default value" );
            var s = proxy.Invoke( d, new object[0] );
            Assert.IsNull( s, "retval should be null" );
            Assert.AreEqual( 69, d.fieldPrimitive, "Should have been set" );
        }
    }
}
