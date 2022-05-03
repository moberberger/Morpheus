using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;



namespace Morpheus.Standard.UnitTests
{
    /// <summary>
    /// This class tests the CXmlHelper class from MorpheusUtil
    /// </summary>
    [TestClass]
    [TestCategory("Dependency Injection")]
    public class DI_Tests
    {
        [TestMethod]
        public void BasicOperationTest()
        {
            var di = DI.Default.New();
            di.For<Random>().UseNewInstance<CryptoRandomNumbers>();

            var rng = di.Get<Random>();
            Assert.AreEqual( typeof( CryptoRandomNumbers ), rng.GetType() );

            var rng2 = di.Get<Random>();
            Assert.AreNotEqual( rng, rng2 );
        }

        [TestMethod]
        public void SingletonTest()
        {
            var di = DI.Default.New();
            di.For<Random>().UseNewInstance<MersenneTwister>().AsSingleton();

            var rng = di.Get<Random>();
            Assert.AreEqual( rng.GetType(), typeof( MersenneTwister ) );

            var rng2 = di.Get<Random>();
            Assert.AreEqual( rng, rng2 );
        }


        [TestMethod]
        public void SpecificSingletonTest()
        {
            var di = DI.Default.New();

            var basis = new MersenneTwister();
            di.For<Random>().UseNewInstance<MersenneTwister>().AsSingleton( basis );

            var rng = di.Get<Random>();
            Assert.AreEqual( rng, basis );

            var rng2 = di.Get<Random>();
            Assert.AreEqual( rng, rng2 );
        }


        [TestMethod]
        public void ChangingSingletonTest()
        {
            var di = DI.Default.New();

            var basis = new MersenneTwister();
            di.For<Random>().UseNewInstance<MersenneTwister>().AsSingleton( basis );

            var rng = di.Get<Random>();
            Assert.AreEqual( rng, basis );

            var basis2 = new CryptoRandomNumbers();
            di.For<Random>().Use( basis2 );

            var rng2 = di.Get<Random>();
            Assert.AreEqual( rng2, basis2 );
            Assert.AreNotEqual( rng2, rng );
        }


        // TODO: Not Implemented Exception
        [TestMethod]
        public void DIContainerTests()
        {
            var di = DI.Default.New();

            di.For<Random>().UseNewInstance<CryptoRandomNumbers>();
            var rng1 = di.Get<Random>();
            Assert.AreEqual( typeof( CryptoRandomNumbers ), rng1.GetType() );

            var basis = new List<int>();
            di.For<IEnumerable<int>>().Use( basis );
            var list1 = di.Get<IEnumerable<int>>();
            Assert.AreEqual( typeof( List<int> ), list1.GetType() );
            Assert.AreEqual( basis, list1 );

            var di2 = di.New();
            var rng2 = di2.GetInstance<Random>();
            Assert.AreEqual( typeof( CryptoRandomNumbers ), rng2.GetType() );

            di2.For<Random>().UseNewInstance<MersenneTwister>();
            var rng3 = di2.GetInstance<Random>();
            Assert.AreEqual( typeof( MersenneTwister ), rng3.GetType() );

            var rng4 = di.Get<Random>();
            Assert.AreEqual( typeof( CryptoRandomNumbers ), rng4.GetType() );
            Assert.AreNotEqual( rng1, rng4 );

            var rng5 = di2.GetInstance<IEnumerable<int>>();
            Assert.AreEqual( basis, rng5 );
        }



        [TestMethod]
        public void DIContainerSingletonTest()
        {
            var di = DI.Default.New();
            di.For<Random>()
                    .UseNewInstance<MersenneTwister>()
                    .Use( new CryptoRandomNumbers() );

            var rng1 = di.Get<Random>();
            Assert.AreEqual( typeof( CryptoRandomNumbers ), rng1.GetType() );

            di.For<Random>().AsSingleton();
            var rng2 = di.Get<Random>();
            Assert.AreEqual( typeof( MersenneTwister ), rng2.GetType() );

            di.For<Random>().UseNewInstance<Random>();
            var rng3 = di.Get<Random>();
            Assert.AreEqual( typeof( Random ), rng3.GetType() );
        }


        // TODO: Broken
        //[TestMethod]
        //public void DefaultOverrideTest()
        //{
        //    DI.Default.For<Random>().Use( Rng.Default );

        //    var di = DI.NewDefault();
        //    var rng1 = di.Get<Random>();
        //    Assert.AreEqual( Rng.Default, rng1 );

        //    di.For<Random>().Use<MersenneTwister>();
        //    var rng2 = di.Get<Random>();
        //    Assert.AreEqual( typeof( MersenneTwister ), rng2.GetType() );
        //    Assert.AreNotEqual( Rng.Default, rng2 );

        //    var rng3 = DI.Default.Get<Random>();
        //    Assert.AreEqual( Rng.Default, rng3 );

        //    di.For<Random>().Clear();
        //    var rng4 = di.Get<Random>();
        //    Assert.AreEqual( Rng.Default, rng4 );
        //}
    }
}
