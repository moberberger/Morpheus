using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Morpheus.Standard.UnitTests
{
    [TestClass()]
    public class CObjectCopierTest
    {
        public class CBase1
        {
            public string Name;
            public int Age { get; set; }

            private double m_weight = 0;
            public double Weight
            {
                get => m_weight;
                private set => m_weight = value;
            }

            public void SetWeight( double _value ) => m_weight = _value;
        }

        public class CBase2
        {
            public int Age;
            public string Address;
        }

        public class CExtend2 : CBase2
        {
            public double Weight;
            private string m_name;
            public string Name => m_name;
            public void SetName( string _value ) => m_name = _value;
        }

        public class CBase3
        {
            public int Age;
            public string Name;
            private double m_weight;
            public double Weight
            {
                private get => m_weight;
                set => m_weight = value;
            }
            public double GetWeight() => m_weight;
        }

        [TestMethod]
        public void Base1SourceTest()
        {
            var o1 = new CBase1
            {
                Name = "One",
                Age = 11
            };
            o1.SetWeight( 111 );

            var o2 = new CExtend2();
            o2.SetName( "Two" );
            o2.Age = 22;
            o2.Weight = 222;

            CObjectCopier.CopyValues( o2, o1 );

            Assert.AreEqual( "Two", o2.Name, "Name" ); // Cannot set Name through property
            Assert.AreEqual( 11, o2.Age, "Age" );
            Assert.AreEqual( 111.0, o2.Weight, "Weight" );
        }

        [TestMethod]
        public void Base2SourceTest()
        {
            var o1 = new CBase1
            {
                Name = "One",
                Age = 11
            };
            o1.SetWeight( 111 );

            var o2 = new CExtend2();
            o2.SetName( "Two" );
            o2.Age = 22;
            o2.Weight = 222;

            CObjectCopier.CopyValues( o1, o2 );

            Assert.AreEqual( "Two", o1.Name, "Name" ); // Cannot set Name through property
            Assert.AreEqual( 22, o1.Age, "Age" );
            Assert.AreEqual( 111.0, o1.Weight, "Weight" );
        }

        [TestMethod]
        public void PrivateGetterTest()
        {
            var o1 = new CBase1
            {
                Name = "One",
                Age = 11
            };
            o1.SetWeight( 111 );

            var o3 = new CBase3
            {
                Name = "Three",
                Age = 33,
                Weight = 333
            };

            CObjectCopier.CopyValues( o1, o3 );

            Assert.AreEqual( "Three", o1.Name, "Name" ); // Cannot set Name through property
            Assert.AreEqual( 33, o1.Age, "Age" );
            Assert.AreEqual( 111.0, o1.Weight, "Weight" );
        }



    }
}
