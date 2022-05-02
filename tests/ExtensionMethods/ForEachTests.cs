using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

using System.Linq;

namespace Morpheus.Standard.UnitTests.IEnumerable
{
    [TestClass]
    [TestCategory( "IEnumerable" )]
    public class ForEachTests
    {
        [TestMethod]
        public void ForEachTest()
        {
            var col = new int[] { 1, 3, 5, 7 };

            int sum1 = 0;
            col.ForEach( _number => sum1 += _number );

            int sum2 = col.Sum();
            Assert.AreEqual( sum1, sum2 );
        }

        [TestMethod]
        public void ForEachWithIndexTest()
        {
            var col = new int[] { 1, 2, 3, 4, 5, 6 };
            
            col.ForEach(
                ( _item, _index ) => Assert.AreEqual( _index + 1, _item, $"At Index {_index}" )
            );
        }

        [TestMethod]
        public void ForEachObjectTest()
        {
            object[] stuff = new object[] { "hello", 42, DateTime.Now };

            stuff.ForEach( _obj =>
            {
                switch (_obj)
                {
                case string _:
                    Assert.AreEqual( stuff[0], _obj );
                    break;

                case int _:
                    Assert.AreEqual( stuff[1], _obj );
                    break;

                case DateTime _:
                    Assert.AreEqual( stuff[2], _obj );
                    break;

                default:
                    Assert.Fail( "No Matching Object Type" );
                    break;
                }
            } );
        }
    }
}
