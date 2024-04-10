using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Morpheus.Standard.UnitTests.DataStructs;

[TestClass]
[TestCategory( "Data Structures" )]
public class CircularListTest
{
    [TestMethod]
    public void ConstructionFromListTest()
    {
        var list = new List<int>() { 69, 420 };
        var cl = new CircularList<int>( list );
        Assert.AreEqual( list.Count, cl.Count );
        Assert.AreEqual( ((ICollection<int>)list).IsReadOnly, cl.IsReadOnly );
        Assert.AreEqual( list[0], cl[0] );
        Assert.AreEqual( list[1], cl[1] );
    }

    [TestMethod]
    public void ConstructionFromIEnumerableTest()
    {
        var list = new List<int>() { 69, 420 };
        var enm = list.Select( x => x * 2 );
        var cl = new CircularList<int>( enm );
        Assert.AreEqual( list.Count, cl.Count );
        Assert.AreEqual( list[0] * 2, cl[0] );
        Assert.AreEqual( list[1] * 2, cl[1] );

        cl[0] = 42;
        Assert.AreNotEqual( list[0], cl[0] );
        Assert.AreNotEqual( list[0] * 2, cl[0] );
    }

    [TestMethod]
    public void SafeIndexTest()
    {
        var list = new List<int>() { 69, 420 };
        var cl = new CircularList<int>( list );
        Assert.AreEqual( list[0], cl[0] );
        Assert.AreEqual( list[1], cl[1] );
        cl[0] = cl[1];
        Assert.AreEqual( list[1], cl[0] );
    }

    [TestMethod]
    public void UnsafeIndexTest()
    {
        var list = new List<int>() { 69, 420 };
        var cl = new CircularList<int>( list );
        Assert.AreEqual( list[0], cl[2] );
        Assert.AreEqual( list[1], cl[-1] );
        cl[4] = cl[-19];
        Assert.AreEqual( list[1], cl[0] );
    }

    [TestMethod]
    public void IndexOfTest()
    {
        var list = new List<int>() { 69, 420 };
        var cl = new CircularList<int>( list );
        Assert.AreEqual( 0, cl.IndexOf( 69 ) );
        Assert.AreEqual( 1, cl.IndexOf( 420 ) );
        Assert.AreEqual( -1, cl.IndexOf( 666 ) );
    }

    [TestMethod]
    public void ContainsTest()
    {
        var list = new List<int>() { 69, 420 };
        var cl = new CircularList<int>( list );
        Assert.IsTrue( cl.Contains( 69 ) );
        Assert.IsTrue( cl.Contains( 420 ) );
        Assert.IsFalse( cl.Contains( 666 ) );
    }

    [TestMethod]
    public void CopyToTest()
    {
        var list = new List<int>() { 69, 420 };
        var cl = new CircularList<int>( list );
        var array = new int[2];
        cl.CopyTo( array, 0 );
        Assert.AreEqual( list[0], array[0] );
        Assert.AreEqual( list[1], array[1] );
    }

    [TestMethod]
    public void GetEnumeratorTest()
    {
        var list = new List<int>() { 69, 420 };
        var cl = new CircularList<int>( list );
        var enm = cl.GetEnumerator();
        enm.MoveNext();
        Assert.AreEqual( list[0], enm.Current );
        enm.MoveNext();
        Assert.AreEqual( list[1], enm.Current );
    }
}