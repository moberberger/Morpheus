#if false 

using System;
using System.Reflection;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// The members of the test classes are indeed never used or set in a manner that would be
/// detected by the compiler, so ignore these warnings for this file only. The warnings are
/// still good and proper for most everything else (other than a reflection-based unit test)
/// </summary>
#pragma warning disable 0169, 0649

//using CFieldTransfer = Morpheus.Core.CFieldTransfer;

namespace Morpheus.Standard.UnitTests.Reflection
{
    [TestClass]
    [TestCategory( "Reflection" )]
    public class ReflectionTests
    {

        private class Attr : Attribute
        {
            public string Name { get; set; }
            public int Number { get; set; }

            public Attr() { Name = "Default"; Number = 0; }
        }

        [Attr( Name = "Homer" )]
        private class A
        {
            private readonly int age;
        };

        [Attr]
        private class B : A
        {
            [Attr( Name = "homer", Number = 1 )]
            private readonly string name;
        }

        private class BB : B
        {
            private readonly DateTime bday;
            public float height;
        }

        private class Z : A
        {
            public string zoo;

        }

        [Attr( Name = "Whatever" )]
        private class C
        {
            public int constructorType = -1;
            public int constructorValue = 0;

            public C() { constructorType = 0; }
            public C( int x ) { constructorType = 1; constructorValue = x; }
            public C( A x ) { constructorType = 2; }
        }

        [TestMethod]
        [TestCategory( "Reflection" )]
        public void GetAllFieldsTest()
        {
            var typ = typeof( B );
            var reflectedFields = typ.GetFields( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance );

            Assert.AreEqual( 1, reflectedFields.Length, "Reflection only returns the NAME field" );
            Assert.AreEqual( "name", reflectedFields[0].Name, "Only field should be NAME" );

            var betterFields = typ.GetAllFields().ToArray();

            Assert.AreEqual( 2, betterFields.Length, "GetAllFields looks at base classes" );
            Assert.IsTrue( betterFields.Contains( _fi => _fi.Name == "name" ) );
            Assert.IsTrue( betterFields.Contains( _fi => _fi.Name == "age" ) );
        }

        [TestMethod]
        [TestCategory( "Reflection" )]
        public void GetInheritanceChainTest()
        {
            var typ = typeof( BB );
            var others = typ.GetTypesInInheritanceChain( false, false ).ToArray();
            Assert.AreEqual( 2, others.Length, "Should be two 'other' types, not including BB and -object-" );
            Assert.AreEqual( typeof( B ), others[0], "B should be first in enumeration" );
            Assert.AreEqual( typeof( A ), others[1], "A should be second in enumeration" );

            others = typ.GetTypesInInheritanceChain( true, true ).ToArray();
            Assert.AreEqual( 4, others.Length, "Now it should contain BB, B, A, and -object-" );
            Assert.AreEqual( typeof( BB ), others[0], "BB should be first in enumeration" );
            Assert.AreEqual( typeof( B ), others[1], "B should be second in enumeration" );
            Assert.AreEqual( typeof( A ), others[2], "A should be third in enumeration" );
            Assert.AreEqual( typeof( object ), others[3], "-object- should be fourth in enumeration" );
        }


        [TestMethod]
        [TestCategory( "Reflection" )]
        public void FT_GetCommonTypeTest()
        {
            var mAge = typeof( A ).GetField( "age", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly );
            var mName = typeof( B ).GetField( "name", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly );
            var mBday = typeof( BB ).GetField( "bday", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly );
            var mZoo = typeof( Z ).GetField( "zoo", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly );
            var mHeight = typeof( BB ).GetField( "height", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly );

            Assert.IsNotNull( mAge );
            Assert.IsNotNull( mName );
            Assert.IsNotNull( mBday );
            Assert.IsNotNull( mZoo );

            var t = CFieldTransfer.GetCommonType( new FieldInfo[] { mAge } );
            Assert.AreEqual( typeof( A ), t, "Age alone" );

            t = CFieldTransfer.GetCommonType( new FieldInfo[] { mBday } );
            Assert.AreEqual( typeof( BB ), t, "Bday alone" );

            t = CFieldTransfer.GetCommonType( new FieldInfo[] { mAge, mBday, mName } );
            Assert.AreEqual( typeof( BB ), t, "BB Chain" );

            t = CFieldTransfer.GetCommonType( new FieldInfo[] { mZoo, mAge } );
            Assert.AreEqual( typeof( Z ), t, "Z Chain" );

            t = CFieldTransfer.GetCommonType( new FieldInfo[] { mHeight, mName, mBday, mAge } );
            Assert.AreEqual( typeof( BB ), t, "BB All" );

            try
            {
                t = CFieldTransfer.GetCommonType( new FieldInfo[] { mAge, mZoo, mName, mBday } );
            }
            catch (InvalidOperationException)
            {
                return;
            }
            throw new InvalidOperationException( "Expected an InvalidOperationException to be thrown from GetCommonType for a double inheritance chain." );
        }


        public class CPerson
        {
            public string Name;
            public int Age;

            public string NameAge
            {
                get => Name + Age;
                set => Name = value;
            }
        }

        public class CMother : CPerson
        {
            public CPerson Child;
        }

        public class CFather : CPerson
        {
            public CPerson Child;
        }


        [TestMethod]
        [TestCategory( "Reflection" )]
        public void FT_FullType()
        {
            var typ = typeof( CMother );
            var ft = CFieldTransfer.CreateFromType( typ );

            var kid = new CPerson() { Name = "Kid", Age = 8 };
            var m1 = new CMother() { Name = "Mom", Age = 35, Child = kid };

            var m2 = new CMother();
            Assert.IsNull( m2.Child, "Child" );
            Assert.AreEqual( 0, m2.Age, "Age" );
            Assert.IsNull( m2.Name, "Name" );

            ft.TransferFields( m2, m1 ); // Actually sending in more derived types, but this is OK

            Assert.AreEqual( "Mom", m2.Name, "Name" );
            Assert.AreEqual( 35, m2.Age, "Age" );
            Assert.AreEqual( kid, m2.Child, "Kid" );
        }

        [TestMethod]
        [TestCategory( "Reflection" )]
        public void FT_FullTypeGeneric()
        {
            var ft = CFieldTransfer.CreateFromType<CPerson>();

            var kid = new CPerson() { Name = "Kid", Age = 8 };

            var m1 = new CMother() { Name = "Mom", Age = 35, Child = kid };

            var f1 = new CFather();
            Assert.IsNull( f1.Child, "Child" );
            Assert.AreEqual( 0, f1.Age, "Age" );
            Assert.IsNull( f1.Name, "Name" );

            ft.TransferFields( f1, m1 );

            Assert.AreEqual( "Mom", f1.Name, "Name" );
            Assert.AreEqual( 35, f1.Age, "Age" );
            Assert.IsNull( f1.Child, "Child should be null" );
        }

        [TestMethod]
        [TestCategory( "Reflection" )]
        public void FT_FieldListGood()
        {
            var mName = typeof( CPerson ).GetField( "Name" );
            var mAge = typeof( CPerson ).GetField( "Age" );
            var mChild = typeof( CMother ).GetField( "Child" );

            var ft = CFieldTransfer.CreateFromFields( new FieldInfo[] { mAge, mChild, mName } );

            var kid = new CPerson() { Name = "Kid", Age = 8 };
            var m1 = new CMother() { Name = "Mom", Age = 35, Child = kid };

            var m2 = new CMother();
            Assert.IsNull( m2.Child, "Child" );
            Assert.AreEqual( 0, m2.Age, "Age" );
            Assert.IsNull( m2.Name, "Name" );

            ft.TransferFields( m2, m1 ); // Actually sending in more derived types, but this is OK

            Assert.AreEqual( "Mom", m2.Name, "Name" );
            Assert.AreEqual( 35, m2.Age, "Age" );
            Assert.AreEqual( kid, m2.Child, "Kid" );
        }

        [TestMethod]
        [TestCategory( "Reflection" )]
        public void FT_FieldListExpression()
        {
            var ft = CFieldTransfer.CreateFromExpressions<CMother>(
                m => m.Name,
                m => m.Child );

            var kid = new CPerson() { Name = "Kid", Age = 8 };

            var m1 = new CMother() { Name = "Mom", Age = 35, Child = kid };

            var m2 = new CMother();
            Assert.IsNull( m2.Child, "Child" );
            Assert.AreEqual( 0, m2.Age, "Age" );
            Assert.IsNull( m2.Name, "Name" );

            ft.TransferFields( m2, m1 );

            Assert.AreEqual( "Mom", m2.Name, "Name" );
            Assert.AreEqual( 0, m2.Age, "Age should not have copied" );
            Assert.AreEqual( kid, m2.Child, "Child" );
        }

        [TestMethod]
        [TestCategory( "Reflection" )]
        public void FT_FieldListExpression2()
        {
            var ft = CFieldTransfer.CreateFromExpressions<CMother>(
                m => m.Age,
                m => m.Child );

            var kid = new CPerson() { Name = "Kid", Age = 8 };

            var m1 = new CMother() { Name = "Mom", Age = 35, Child = kid };

            var m2 = new CMother();
            Assert.IsNull( m2.Child, "Child" );
            Assert.AreEqual( 0, m2.Age, "Age" );
            Assert.IsNull( m2.Name, "Name" );

            ft.TransferFields( m2, m1 );

            Assert.IsNull( m2.Name, "Name" );
            Assert.AreEqual( 35, m2.Age, "Age" );
            Assert.AreEqual( kid, m2.Child, "Child" );
        }

        [TestMethod]
        [TestCategory( "Reflection" )]
        [ExpectedException( typeof( ArgumentException ) )]
        public void FT_FieldListExpressionBad()
        {
            var ft = CFieldTransfer.CreateFromExpressions<CMother>(
                m => m.Name,
                m => m.Child,
                m => m.NameAge );
        }

        [TestMethod]
        [TestCategory( "Reflection" )]
        [ExpectedException( typeof( InvalidOperationException ) )]
        public void FT_FieldListBad()
        {
            var mName = typeof( CPerson ).GetField( "Name" );
            var mAge = typeof( CPerson ).GetField( "Age" );
            var mChild = typeof( CMother ).GetField( "Child" );
            var fChild = typeof( CFather ).GetField( "Child" );

            var ft = CFieldTransfer.CreateFromFields( new FieldInfo[] { mChild, fChild } );
        }

        [TestMethod]
        [TestCategory( "Reflection" )]
        public void GetSingleAttributeTest()
        {
            var member = typeof( B ).GetField( "name", BindingFlags.NonPublic | BindingFlags.Instance );
            Assert.IsNotNull( member );

            var attr = member.GetSingleAttribute<Attr>();
            Assert.IsNotNull( attr );
            Assert.AreEqual( "homer", attr.Name, "attr.name" );
            Assert.AreEqual( 1, attr.Number, "attr.number" );

            var attr2 = member.GetSingleAttribute<FlagsAttribute>();
            Assert.IsNull( attr2, "Should not have Flags" );
        }


        [TestMethod]
        [TestCategory( "Reflection" )]
        public void GetTypesWithAttributeTest()
        {
            var types = Lib.GetTypesWithAttribute<Attr>();
            Assert.AreEqual( 3, types.Count(), "Expected 2 types with that attribute" );
        }

        [TestMethod]
        [TestCategory( "Reflection" )]
        public void GetTypesWithAttributeFilteredTest()
        {
            var types = Lib.GetTypesWithAttribute<Attr>( a => a.Name == "Homer" );
            Assert.AreEqual( 1, types.Count(), "Expected 1 type with that attribute" );
            var t = types.First();
            Assert.AreEqual( "A", t.Name, "Expected type 'A' to have the attribute" );
        }


        [TestMethod]
        [TestCategory( "Reflection" )]
        public void TestHasAttribute()
        {
            var t = GetType();
            var mi = t.GetMember( "TestHasAttribute" );

            var actual = Lib.HasAttribute( mi[0], typeof( TestMethodAttribute ) );
            Assert.AreEqual<bool>( true, actual, "This method DOES have the attribute in question (Type as method parameter)" );

            actual = Lib.HasAttribute<TestMethodAttribute>( mi[0] );
            Assert.AreEqual<bool>( true, actual, "This method DOES have the attribute in question (Type as generic parameter)" );
        }


        [TestMethod]
        [TestCategory( "Reflection" )]
        public void CreateWithConstructorTest()
        {
            object o = Lib.CreateWithConstructor( typeof( C ), new A() );
            Assert.IsInstanceOfType( o, typeof( C ), "Expected a type of C when using TYPE" );
            Assert.AreEqual( 2, (o as C).constructorType, "Should have used constructor for B" );

            o = Lib.CreateWithConstructor( typeof( C ) );
            Assert.IsInstanceOfType( o, typeof( C ), "Expected a type of C for default constructor when using TYPE" );
            Assert.AreEqual( 0, (o as C).constructorType, "Should have used default constructor" );
        }

        [TestMethod]
        [TestCategory( "Reflection" )]
        public void ArrayFillTest()
        {
            var arr = new C[10].Fill( () => new C( 69 ) );
            Assert.IsNotNull( arr, "created array" );
            Assert.AreEqual( 10, arr.Length, "Length" );
            for (var i = 0; i < arr.Length; i++)
            {
                Assert.AreEqual( 69, arr[i].constructorValue, "Constructor Value at index: " + i );
            }
        }

        [TestMethod]
        [TestCategory( "Reflection" )]
        [ExpectedException( typeof( ArgumentException ) )]
        public void CantImplementNonInterfaceTest() => typeof( string ).ImplementsInterface( typeof( string ) );



        public class CImplementsIEnumerable : System.Collections.IEnumerable
        {
            public System.Collections.IEnumerator GetEnumerator() => throw new NotImplementedException();
        }

        [TestMethod]
        [TestCategory( "Reflection" )]
        public void NonGenericImplementsInterfaceTest()
        {
            Assert.IsTrue( typeof( CImplementsIEnumerable ).ImplementsInterface( typeof( System.Collections.IEnumerable ) ) );
            Assert.IsFalse( typeof( CImplementsIEnumerable ).ImplementsInterface( typeof( System.Collections.Generic.IEnumerable<> ) ) );
            Assert.IsFalse( typeof( CImplementsIEnumerable ).ImplementsInterface( typeof( System.Collections.Generic.IEnumerable<string> ) ) );
            Assert.IsFalse( typeof( CImplementsIEnumerable ).ImplementsInterface( typeof( System.ICloneable ) ) );
        }



        public class CImplementsIEnumerableT<T> : System.Collections.Generic.IEnumerable<T>
        {
            public System.Collections.Generic.IEnumerator<T> GetEnumerator() => throw new NotImplementedException();

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => throw new NotImplementedException();
        }


        [TestMethod]
        [TestCategory( "Reflection" )]
        public void ImplementsGenericInterfaceTest()
        {
            Assert.IsFalse( typeof( CImplementsIEnumerableT<> ).ImplementsInterface( typeof( System.Collections.Generic.IEnumerable<int> ) ) );
            Assert.IsTrue( typeof( CImplementsIEnumerableT<> ).ImplementsInterface( typeof( System.Collections.Generic.IEnumerable<> ) ) );
            Assert.IsTrue( typeof( CImplementsIEnumerableT<> ).ImplementsInterface( typeof( System.Collections.IEnumerable ) ) );
            Assert.IsFalse( typeof( CImplementsIEnumerableT<> ).ImplementsInterface( typeof( System.ICloneable ) ) );

        }


        public class CImplementsSpecificIEnumerable : ICloneable, System.Collections.Generic.IEnumerable<int>
        {
            public System.Collections.Generic.IEnumerator<int> GetEnumerator() => throw new NotImplementedException();

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => throw new NotImplementedException();

            public object Clone() => throw new NotImplementedException();
        }

        [TestMethod]
        [TestCategory( "Reflection" )]
        public void ImplementsSpecificVersionOfGenericTest()
        {
            Assert.IsTrue( typeof( CImplementsSpecificIEnumerable ).ImplementsInterface( typeof( System.Collections.Generic.IEnumerable<int> ) ) );
            Assert.IsTrue( typeof( CImplementsSpecificIEnumerable ).ImplementsInterface( typeof( System.Collections.Generic.IEnumerable<> ) ) );
            Assert.IsTrue( typeof( CImplementsSpecificIEnumerable ).ImplementsInterface( typeof( System.Collections.IEnumerable ) ) );
            Assert.IsTrue( typeof( CImplementsSpecificIEnumerable ).ImplementsInterface( typeof( ICloneable ) ) );
        }


        public class MemberProxyTestClass
        {
            public string Name;
            public double Height;
            public string City { get; set; }
            public int Zip { get; set; }

            public MemberProxyTestClass()
            {
                Name = "Walter";
                Height = 6.05;
                City = "Sparks";
                Zip = 89431;
            }
        }


        [TestMethod]
        [TestCategory( "Reflection" )]
        public void GS_GeneralTest()
        {
            var t = typeof( MemberProxyTestClass );

            var fiName = t.GetField( "Name" );
            var fiHeight = t.GetField( "Height" );
            var piCity = t.GetProperty( "City" );
            var piZip = t.GetProperty( "Zip" );

            var gsName = new PropertyOrFieldProxy( fiName );
            var gsHeight = new PropertyOrFieldProxy( fiHeight );
            var gsCity = new PropertyOrFieldProxy( piCity );
            var gsZip = new PropertyOrFieldProxy( piZip );

            var x1 = new MemberProxyTestClass
            {
                Name = "Daeven",
                Height = 4.2,
                City = "Spring Hill",
                Zip = 37174
            };


            var x2 = new MemberProxyTestClass();
            Assert.AreEqual( x2.Name, "Walter", "NAme" );
            Assert.AreEqual( x2.City, "Sparks", "City" );
            Assert.AreEqual( x2.Height, 6.05, "height" );
            Assert.AreEqual( x2.Zip, 89431, "Zip" );

            gsName.Set( x2, gsName.Get( x1 ) );
            gsHeight.Set( x2, gsHeight.Get( x1 ) );
            gsCity.Set( x2, gsCity.Get( x1 ) );
            gsZip.Set( x2, gsZip.Get( x1 ) );

            Assert.AreEqual( x2.Name, "Daeven", "NAme" );
            Assert.AreEqual( x2.City, "Spring Hill", "City" );
            Assert.AreEqual( x2.Height, 4.2, "height" );
            Assert.AreEqual( x2.Zip, 37174, "Zip" );
        }

        [TestMethod]
        [TestCategory( "Reflection" )]
        public void GS_GenericTest()
        {
            // var a = new Attr();

            // var stringGS = new MemberProxy
            // <Attr> ( _a => _a.Name ); stringGS.Set( a, "Homer" ); Assert.AreEqual( "Homer",
            // a.Name, "General Name Set" );

            // a.Name = "Bart"; Assert.AreEqual( "Bart", stringGS.Get( a ), "General Name Get"
            // );


            // var intGS = new MemberProxy
            // <Attr> ( _a => _a.Number ); intGS.Set( a, 5 ); Assert.AreEqual( 5, a.Number,
            // "Set" );

            // a.Number = 69; Assert.AreEqual( 69, intGS.Get( a ), "Int Set" );
        }

        [TestMethod]
        [TestCategory( "Reflection" )]
        public void TestBetterGetType()
        {
            var tString = "System.String";

            var expected = tString.GetType();
            var actual = Lib.BetterGetType( tString );

            Assert.AreEqual<IntPtr>( expected.TypeHandle.Value, actual.TypeHandle.Value, "Test of basic Type getting is wrong" );

            expected = typeof( ReflectionTests );
            tString = expected.FullName;
            actual = Lib.BetterGetType( tString );
            Assert.IsNotNull( actual, "Actual from Reflection is NULL" );

            Assert.AreEqual<IntPtr>( expected.TypeHandle.Value, actual.TypeHandle.Value, "Test of advanced Type getting is wrong" );

            tString = "UnitTestMorpheus.CReflectionTestNothing";
            actual = Lib.BetterGetType( tString );

            Assert.IsNull( actual, "NULL Expected from fictious type name" );

            // Re-test here to make sure we check the "Cached" version
            expected = typeof( ReflectionTests );
            tString = expected.FullName;
            actual = Lib.BetterGetType( tString, true );
            Assert.IsNotNull( actual, "Actual from Reflection is NULL" );
            Assert.AreEqual<IntPtr>( expected.TypeHandle.Value, actual.TypeHandle.Value, "Test of advanced Type getting is wrong using Cache" );

            actual = Lib.BetterGetType( tString, true );
            Assert.IsNotNull( actual, "Actual from Reflection is NULL" );
            Assert.AreEqual<IntPtr>( expected.TypeHandle.Value, actual.TypeHandle.Value, "Test of advanced Type getting is wrong using Cache" );

            actual = Lib.BetterGetType( tString, false );
            Assert.IsNotNull( actual, "Actual from Reflection is NULL" );
            Assert.AreEqual<IntPtr>( expected.TypeHandle.Value, actual.TypeHandle.Value, "Test of advanced Type getting is wrong using Cache" );
        }

    }
}

#endif