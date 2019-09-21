using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Morpheus.Standard.UnitTests.DataStructs
{
    [TestClass]
    [TestCategory( "Data Structures" )]
    public class KdTreeTest
    {
        public class City
        {
            public double Lat, Lng;
            public int Id;

            public static double GetAxisValue( City _city, int _axis )
            {
                if (_city == null)
                    throw new ArgumentNullException();
                if (_axis == 0)
                    return _city.Lat;
                if (_axis == 1)
                    return _city.Lng;
                throw new ArgumentOutOfRangeException( "_axis", _axis, "Axis needs to be 0 or 1" );
            }

            public static double GetDistance( City _one, City _two )
            {
                if (_one == null)
                    throw new ArgumentNullException( "_one" );
                if (_two == null)
                    throw new ArgumentNullException( "_two" );

                var dx = _one.Lat - _two.Lat;
                var dy = _one.Lng - _two.Lng;
                return dx * dx + dy * dy;
            }

            public static double RealDistance( City _one, City _two )
            {
                var dx = _one.Lat - _two.Lat;
                var dy = _one.Lng - _two.Lng;
                return Math.Sqrt( dx * dx + dy * dy );
            }
        }


        [TestMethod]
        public void AddToTreeTest()
        {
            const int COUNT = 1000;
            var rng = new Random( 120 ); // Some arbitrary seed ensuring that all sequences for all test runs are identical (and therefore NOT random)

            var tree = new KDTree<City>( 2, City.GetAxisValue, City.GetDistance );
            tree.RebuildTree( COUNT ); // pre-allocate memory to try to avoid use of the sparse leaves in the tree

            var list = new List<City>();

            for (var i = 0; i < COUNT; i++)
            {
                var city = new City() { Id = i, Lat = rng.NextDouble() * 10, Lng = rng.NextDouble() * 10 };
                tree.Add( city );
                list.Add( city );
            }

            VerifyTreeToList( tree, list );

            var tree2 = new KDTree<City>( list, 2, City.GetAxisValue, City.GetDistance );
            VerifyTreeToList( tree2, list );
        }




        private static void VerifyTreeToList( KDTree<City> tree, List<City> list )
        {

            for (var x = 0.0; x <= 10; x += 0.25)
            {
                for (var y = 0.0; y <= 10; y += 0.25)
                {
                    var city = new City() { Lat = x, Lng = y };

                    var treeClosest = tree.FindNearest( city );
                    var listClosest = list.Smallest( _c => City.RealDistance( _c, city ) );

                    Assert.AreEqual( listClosest.Id, treeClosest.Id, "The closest was not generated equally" );
                }
            }
        }
    }
}
