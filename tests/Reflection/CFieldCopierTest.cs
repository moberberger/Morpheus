using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Morpheus.Standard.UnitTests
{
    /// <summary>
    /// This is a test class for CFieldCopier and is intended
    /// to contain all CFieldCopier Unit Tests
    ///</summary>
    [TestClass()]
    public class CFieldCopierTest
    {
        private class CTestClass2 : CTestClass
        {
            public double a;
        }

        private class CTestClass
        {
            public int x, y;
            public string s;

            public CTestClass testClass;
        }

        private class CSimilarClass
        {
            public int x;
            public string s;
        }

        private class CDissimilarClass
        {
            public int x;
            public double s;
        }

        [TestMethod]
        public void TestCloneObject()
        {
            var expected = new CTestClass
            {
                x = 1,
                y = 2,
                s = "Homer",
                testClass = new CTestClass
                {
                    x = 11,
                    y = 22,
                    s = "Bart"
                }
            };

            var actual = CFieldCopier.Clone<CTestClass>( expected );
            Assert.AreEqual<int>( expected.x, actual.x, "X" );
            Assert.AreEqual<int>( expected.y, actual.y, "Y" );
            Assert.AreEqual<string>( expected.s, actual.s, "S" );
            Assert.AreEqual<int>( expected.testClass.x, actual.testClass.x, "sub.X" );
            Assert.AreEqual<int>( expected.testClass.y, actual.testClass.y, "sub.Y" );
            Assert.AreEqual<string>( expected.testClass.s, actual.testClass.s, "sub.S" );
        }

        [TestMethod]
        public void TestCloneObject2()
        {
            var expected = new CTestClass2
            {
                x = 1,
                y = 2,
                s = "Homer",
                a = 1.5,
                testClass = new CTestClass
                {
                    x = 11,
                    y = 22,
                    s = "Bart"
                }
            };

            var actual = (CTestClass2) CFieldCopier.Clone( expected );
            Assert.AreEqual<int>( expected.x, actual.x, "X" );
            Assert.AreEqual<int>( expected.y, actual.y, "Y" );
            Assert.AreEqual<double>( expected.a, actual.a, "A" );
            Assert.AreEqual<string>( expected.s, actual.s, "S" );
            Assert.AreEqual<int>( expected.testClass.x, actual.testClass.x, "sub.X" );
            Assert.AreEqual<int>( expected.testClass.y, actual.testClass.y, "sub.Y" );
            Assert.AreEqual<string>( expected.testClass.s, actual.testClass.s, "sub.S" );
        }

        [TestMethod]
        public void TestSimilarObjects()
        {
            var expected = new CTestClass2
            {
                x = 1,
                y = 2,
                s = "Homer",
                a = 1.5,
                testClass = new CTestClass
                {
                    x = 11,
                    y = 22,
                    s = "Bart"
                }
            };

            var actual = CFieldCopier.Clone<CSimilarClass>( expected );
            Assert.AreEqual<int>( expected.x, actual.x, "X" );
            Assert.AreEqual<string>( expected.s, actual.s, "S" );
        }

        [TestMethod]
        public void TestSimilarObjects2()
        {
            var expected = new CSimilarClass
            {
                x = 1,
                s = "Homer"
            };

            var actual = CFieldCopier.Clone<CTestClass2>( expected );
            Assert.AreEqual<int>( expected.x, actual.x, "X" );
            Assert.AreEqual<string>( expected.s, actual.s, "S" );
        }

        [TestMethod]
        public void TestDissimilarObjects()
        {
            var expected = new CDissimilarClass
            {
                s = 1.5,
                x = 1
            };

            var actual = CFieldCopier.Clone<CTestClass>( expected );
            Assert.AreEqual<int>( expected.x, actual.x, "X" );
            Assert.IsNull( actual.s, "S" );
        }
    }
}
