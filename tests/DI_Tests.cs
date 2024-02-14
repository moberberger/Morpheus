using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Morpheus.Standard.UnitTests;


[TestClass]
[TestCategory( "Dependency Injection" )]
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
    public void DefaultResolversTest()
    {
        var list1 = DI.Default.Get<List<int>>();
        Assert.AreEqual( typeof( List<int> ), list1.GetType() );

        var di = DI.Default.New();
        var list2 = di.Get<List<int>>();
        Assert.AreEqual( typeof( List<int> ), list2.GetType() );

        di.For<LCPRNG_MMIX>().UseNewInstance();
        var rng = di.Get<LCPRNG_MMIX>();
        Assert.AreEqual( typeof( LCPRNG_MMIX ), rng.GetType() );
    }

    [TestMethod]
    public void SingletonTest()
    {
        var di = DI.Default.New();
        di.For<Random>().UseSingleton( new MersenneTwister() );

        var rng = di.Get<Random>();
        Assert.AreEqual( rng.GetType(), typeof( MersenneTwister ) );

        var rng2 = di.Get<Random>();
        Assert.AreEqual( rng, rng2 );

        // Passing null is only possible because the unit test is an a #nullable
        // disabled project
        Assert.ThrowsException<NullReferenceException>( () =>
            di.For<Rng>().UseSingleton( null )
        );
    }


    [TestMethod]
    public void SpecificSingletonTest()
    {
        var di = DI.Default.New();

        var basis = new MersenneTwister();
        di.For<Random>().UseSingleton( basis );

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
        di.For<Random>().UseSingleton( basis );

        var rng = di.Get<Random>();
        Assert.AreEqual( rng, basis );

        var basis2 = new CryptoRandomNumbers();
        di.For<Random>().UseSingleton( basis2 );

        var rng2 = di.Get<Random>();
        Assert.AreEqual( rng2, basis2 );
        Assert.AreNotEqual( rng2, rng );

        var di2 = di.New();
        var rng3 = di2.Get<Random>();
        Assert.AreEqual( rng3, basis2 );
    }


    [TestMethod]
    public void DIContainerTests()
    {
        var di = DI.Default.New();

        di.For<Random>().UseNewInstance<CryptoRandomNumbers>();
        var rng1 = di.Get<Random>();
        Assert.AreEqual( typeof( CryptoRandomNumbers ), rng1.GetType() );

        var basis = new List<int>();
        di.For<IEnumerable<int>>().UseSingleton( basis );

        var list1 = di.Get<IEnumerable<int>>();
        Assert.AreEqual( typeof( List<int> ), list1.GetType() );
        Assert.AreEqual( basis, list1 );

        var di2 = di.New();
        var rng2 = di2.Get<Random>();
        Assert.AreEqual( typeof( CryptoRandomNumbers ), rng2.GetType() );

        di2.For<Random>().UseNewInstance<MersenneTwister>();
        var rng3 = di2.Get<Random>();
        Assert.AreEqual( typeof( MersenneTwister ), rng3.GetType() );

        var rng4 = di.Get<Random>();
        Assert.AreEqual( typeof( CryptoRandomNumbers ), rng4.GetType() );
        Assert.AreNotEqual( rng1, rng4 );

        var list2 = di2.Get<IEnumerable<int>>();
        Assert.AreEqual( basis, list2 );
    }

    class OverrideTestClass() { }
    [TestMethod]
    public void OverrideToParentTest()
    {
        var di = DI.Default.New();

        var di2 = di.New();
        di2.For<OverrideTestClass>();

        var obj = di2.Get<OverrideTestClass>();
        Assert.AreEqual( typeof( OverrideTestClass ), obj.GetType() );
    }

    [TestMethod]
    public void ParameterizedConstructorTest()
    {
        var di = DI.Default.New();
        var basisRng = new LCPRNG_MMIX( 42 );
        var basis = basisRng.Next();

        di.For<Rng>().UseNewInstance<LCPRNG_MMIX>();
        var rng1 = di.Get<Rng>();
        Assert.AreNotEqual( basis, rng1.Next() );

        var rng2 = di.Get<Rng>( 42ul );
        Assert.AreEqual( basis, rng2.Next() );
    }

    [TestMethod]
    public void ParameterizedConstructorTest2()
    {
        var expected = new LCPRNG_MMIX( 42 ).Next();

        var basis = new LCPRNG_MMIX( 42 );
        var di = DI.Default.New();
        di.For<Rng>().UseNewInstance<SynchronizedRng>();

        Assert.ThrowsException<MissingMethodException>( () => di.Get<Rng>() );

        var rng = di.Get<Rng>( basis );
        var actual = rng.Next();
        Assert.AreEqual( expected, actual );
    }

    [TestMethod]
    public void IncompatibleTypeException()
    {
        var di = DI.Default.New();
        Assert.ThrowsException<InvalidCastException>( () => di.For<Random>().UseNewInstance<DI_Tests>() );
    }

    [TestMethod]
    public void SimpleFactoryTest()
    {
        var di = DI.Default.New();
        di.For<Random>().UseFactory( () => new MersenneTwister() );

        var rng = di.Get<Random>();
        Assert.AreEqual( typeof( MersenneTwister ), rng.GetType() );
    }

    [TestMethod]
    public void ParameterizedFactoryTest1()
    {
        var di = DI.Default.New();
        string str = "";
        di.For<Random>().UseFactory( ( ulong seed ) =>
            {
                str = string.Format( $"Schnoodle Seed = {seed}" );
                Console.WriteLine( str );
                return new LCPRNG_MMIX( seed );
            } );

        var rng = di.Get<Random>( 42ul );
        Assert.AreEqual( typeof( LCPRNG_MMIX ), rng.GetType() );
        Assert.AreEqual( "Schnoodle Seed = 42", str );
    }

    [TestMethod]
    public void ParameterizedFactoryTest2()
    {
        var di = DI.Default.New();
        string str = "";
        di.For<Random>().UseFactory( ( string msg, ulong seed ) =>
            {
                str = string.Format( msg, seed );
                Console.WriteLine( str );
                return new LCPRNG_MMIX( seed );
            } );

        var rng = di.Get<Random>( "Snizzle Seed = {0}", 42ul );
        Assert.AreEqual( typeof( LCPRNG_MMIX ), rng.GetType() );
        Assert.AreEqual( "Snizzle Seed = 42", str );
    }

    [TestMethod]
    public void ParameterizedFactoryTest3()
    {
        var di = DI.Default.New();
        string str = "";
        di.For<Random>().UseFactory( ( string msg, ulong s, int count ) =>
            {
                str = msg + " ";
                for (int i = 0; i < count; i++)
                    str += s.ToString() + " ";
                Console.WriteLine( str );
                return new LCPRNG_MMIX( s );
            } );

        var rng = di.Get<Random>( "Snizzle Seed", 42ul, 4 );
        Assert.AreEqual( typeof( LCPRNG_MMIX ), rng.GetType() );
        Assert.AreEqual( "Snizzle Seed 42 42 42 42 ", str );
    }

    class SimpleResolver : DependencyInjection.IResolver
    {
        public object Get( object[] @params ) =>
            @params.Length == 0 ? "what?" : @params[0];
    }

    [TestMethod]
    public void IResolverFactoryTest()
    {
        var di = DI.Default.New();
        di.For<string>().UseFactory( new SimpleResolver() );
        var str = di.Get<string>();
        Assert.AreEqual( "what?", str );

        Assert.ThrowsException<InvalidCastException>(
            () => di.Get<string>( 1 )
        );
    }

    [TestMethod]
    public void ParentContainsTest()
    {
        var di = DI.Default.New();
        di.For<string>().UseSingleton( "homer" );

        var di2 = di.New();
        Assert.IsTrue( di2.KnowsAbout<string>() );
        Assert.IsFalse( di2.KnowsAbout<List<Exception>>() );
    }
}
