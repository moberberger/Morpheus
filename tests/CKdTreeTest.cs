using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Morpheus.Standard.UnitTests
{
    [TestClass]
    public class CKdTreeTest
    {
        public class CCity
        {
            public double Lat, Lng;
            public int Id;

            public static double GetAxisValue( CCity _city, int _axis )
            {
                if (_city == null)
                    throw new ArgumentNullException();
                if (_axis == 0)
                    return _city.Lat;
                if (_axis == 1)
                    return _city.Lng;
                throw new ArgumentOutOfRangeException( "_axis", _axis, "Axis needs to be 0 or 1" );
            }

            public static double GetDistance( CCity _one, CCity _two )
            {
                if (_one == null)
                    throw new ArgumentNullException( "_one" );
                if (_two == null)
                    throw new ArgumentNullException( "_two" );

                var dx = _one.Lat - _two.Lat;
                var dy = _one.Lng - _two.Lng;
                return dx * dx + dy * dy;
            }

            public static double RealDistance( CCity _one, CCity _two )
            {
                var dx = _one.Lat - _two.Lat;
                var dy = _one.Lng - _two.Lng;
                return Math.Sqrt( dx * dx + dy * dy );
            }
        }


        [TestMethod]
        [TestCategory( "DataStructs" )]
        public void AddToTreeTest()
        {
            const int COUNT = 1000;
            var rng = new Random( 120 ); // Some arbitrary seed ensuring that all sequences for all test runs are identical (and therefore NOT random)

            var tree = new CKDTree<CCity>( 2, CCity.GetAxisValue, CCity.GetDistance );
            tree.RebuildTree( COUNT ); // pre-allocate memory to try to avoid use of the sparse leaves in the tree

            var list = new List<CCity>();

            for (var i = 0; i < COUNT; i++)
            {
                var city = new CCity() { Id = i, Lat = rng.NextDouble() * 10, Lng = rng.NextDouble() * 10 };
                tree.Add( city );
                list.Add( city );
            }

            VerifyTreeToList( tree, list );

            var tree2 = new CKDTree<CCity>( list, 2, CCity.GetAxisValue, CCity.GetDistance );
            VerifyTreeToList( tree2, list );
        }




        private static void VerifyTreeToList( CKDTree<CCity> tree, List<CCity> list )
        {

            for (var x = 0.0; x <= 10; x += 0.25)
            {
                for (var y = 0.0; y <= 10; y += 0.25)
                {
                    var city = new CCity() { Lat = x, Lng = y };

                    var treeClosest = tree.FindNearest( city );
                    var listClosest = list.Smallest( _c => CCity.RealDistance( _c, city ) );

                    Assert.AreEqual( listClosest.Id, treeClosest.Id, "The closest was not generated equally" );
                }
            }
        }
    }
}
