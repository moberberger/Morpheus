using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;

namespace Morpheus.Standard.UnitTests
{
    [TestClass]
    public class HelperTest
    {
        [TestMethod]
        public void TestConvertArrayToString()
        {
            var actual = Lib.ConvertArrayToString( new int[] { 1, 2, 3 } );
            var expected = "1,2,3";

            Assert.AreEqual<string>( expected, actual, "Array not converted correctly" );

            actual = Lib.ConvertArrayToString( null );
            expected = "";
            Assert.AreEqual<string>( expected, actual, "String not correct for a null array" );
        }

        [TestMethod]
        public void TestConvertStringToArray()
        {
            Assert.AreEqual( null, Lib.ConvertStringToArray( null, typeof( string ) ), "NULL Conversion incorrect" );

            var actual = (string[]) Lib.ConvertStringToArray( "", typeof( string ) );
            Assert.AreEqual<int>( 0, actual.Length, "Length of array is not correct for zero-len string" );

            var actual2 = (int[]) Lib.ConvertStringToArray( "1,22, 333", typeof( int ) );
            Assert.AreEqual<int>( 3, actual2.Length, "Length of INT array incorrect" );
            Assert.AreEqual<int>( 1, actual2[0], "Index 0 is incorrect" );
            Assert.AreEqual<int>( 22, actual2[1], "Index 0 is incorrect" );
            Assert.AreEqual<int>( 333, actual2[2], "Index 0 is incorrect" );
        }

        [TestMethod]
        public void TestConvertStringToArrayGeneric()
        {
            Assert.AreEqual( null, Lib.ConvertStringToArray<string>( null, ',' ), "NULL Conversion incorrect" );

            var actual = Lib.ConvertStringToArray<string>( "", ',' );
            Assert.AreEqual<int>( 0, actual.Length, "Length of array is not correct for zero-len string" );

            var actual2 = Lib.ConvertStringToArray<int>( "1|22| 333", '|' );
            Assert.AreEqual<int>( 3, actual2.Length, "Length of INT array incorrect" );
            Assert.AreEqual<int>( 1, actual2[0], "Index 0 is incorrect" );
            Assert.AreEqual<int>( 22, actual2[1], "Index 0 is incorrect" );
            Assert.AreEqual<int>( 333, actual2[2], "Index 0 is incorrect" );
        }

        [TestMethod]
        public void TestParseNextSegment()
        {
            TestParseNextSegment( "" );
            TestParseNextSegment( "homer", "homer" );
            TestParseNextSegment( "The pool is dark", "The", "pool", "is", "dark" );
        }

        public void TestParseNextSegment( string _sentence, params string[] _results )
        {
            string token;
            int len = 0, idx = -1;
            var resultIdx = 0;
            do
            {
                token = Lib.ParseNextSegment( _sentence, ' ', ref idx, ref len );
                if (token != null)
                {
                    Assert.AreEqual<string>( _results[resultIdx++], token, "Result at index " + (resultIdx - 1) + " is not correct." );
                }
            } while (token != null);

            Assert.AreEqual<int>( _results.Length, resultIdx, "The number of results found was incorrect" );
        }

        [TestMethod]
        public void TestAppendElementToArray()
        {
            var arr = new int[3] { 1, 2, 3 };
            Lib.AppendElementToArray<int>( ref arr, 4 );

            Assert.AreEqual<int>( 4, arr.Length, "Length of resultant array is wrong" );
            Assert.AreEqual<int>( 1, arr[0], "Index 0 is incorrect" );
            Assert.AreEqual<int>( 2, arr[1], "Index 1 is incorrect" );
            Assert.AreEqual<int>( 3, arr[2], "Index 2 is incorrect" );
            Assert.AreEqual<int>( 4, arr[3], "Index 3 is incorrect" );

            arr = null;
            Lib.AppendElementToArray<int>( ref arr, 111 );
            Assert.AreEqual<int>( 1, arr.Length, "New length from NULL array is incorrect" );
            Assert.AreEqual<int>( 111, arr[0], "Added element is incorrect from NULL array" );
        }


        [TestMethod]
        public void TestGetFirstNumberInString()
        {
            var x = Lib.GetFirstNumberInString( "homer is 45 years old" );
            Assert.AreEqual<int>( 45, x, "Incorrect number found in string" );
        }

        [TestMethod]
        [ExpectedException( typeof( FormatException ) )]
        public void TestInvalidFirstNumberInString() => Lib.GetFirstNumberInString( "homer is cool" );


        [TestMethod]
        public void TestCompression()
        {
            var msg =
                "The quick brown fox jumped over the lazy dog. To whom it may concern. Eat more meat. How can you have any pudding if you don't eat your meat? ";

            var compressed = Lib.CompressString( msg );
            Console.WriteLine( "Compressed Length: " + compressed.Length );

            var result = Lib.DecompressString( compressed );
            Assert.AreEqual( msg, result, "The resulting string is invalid." );

            var one = new byte[1];
            one[0] = 0x55;

            var oneCompressed = Lib.Compress( one );
            Console.WriteLine( "One byte in an array Compressed Length:" + oneCompressed.Length );

            var bResult = Lib.Decompress( oneCompressed );
            Assert.AreEqual( 1, bResult.Length, "Expected one byte decompressed" );
            Assert.AreEqual( (byte) 0x55, bResult[0], "The byte was wrong" );
        }

        [TestMethod]
        public void TestTransferStreamFull()
        {
            const int SIZE = 1024;
            var source = new byte[SIZE];
            var rng = new Random( 10910 );
            rng.NextBytes( source );

            var sSource = new MemoryStream( source );

            const int OFFSET = SIZE / 100;
            var expectedCount = SIZE - OFFSET;

            sSource.Position = OFFSET;
            var sDest = new MemoryStream();
            Lib.TransferStream( sSource, sDest, 64 );

            Assert.AreEqual<long>( expectedCount, sDest.Length, "Length of destination stream is wrong" );
            var dest = sDest.GetBuffer();

            for (var i = 0; i < expectedCount; i++)
            {
                Assert.AreEqual( source[OFFSET + i], dest[i], "Incorrect byte at index: " + i );
            }
        }

        [TestMethod]
        public void TestTransferStreamFull2()
        {
            const int SIZE = 1024;
            var source = new byte[SIZE];
            var rng = new Random( 10910 );
            rng.NextBytes( source );

            var sSource = new MemoryStream( source );

            const int OFFSET = SIZE / 100;
            var expectedCount = SIZE - OFFSET;

            sSource.Position = OFFSET;
            var sDest = new MemoryStream();
            Lib.TransferStream( sSource, sDest );

            Assert.AreEqual<long>( expectedCount, sDest.Length, "Length of destination stream is wrong" );
            var dest = sDest.GetBuffer();

            for (var i = 0; i < expectedCount; i++)
            {
                Assert.AreEqual( source[OFFSET + i], dest[i], "Incorrect byte at index: " + i );
            }
        }

        [TestMethod]
        public void TestTransferStreamPartial()
        {
            const int SIZE = 1024;
            var source = new byte[SIZE];
            var rng = new Random( 10910 );
            rng.NextBytes( source );

            var sSource = new MemoryStream( source );

            const int OFFSET = SIZE / 100;
            const int COUNT = SIZE / 5;

            sSource.Position = OFFSET;
            var sDest = new MemoryStream();
            Lib.TransferStream( sSource, sDest, 64, COUNT );

            Assert.AreEqual<long>( COUNT, sDest.Length, "Length of destination stream is wrong" );
            var dest = sDest.GetBuffer();

            for (var i = 0; i < COUNT; i++)
            {
                Assert.AreEqual( source[OFFSET + i], dest[i], "Incorrect byte at index: " + i );
            }
        }

        [TestMethod]
        public void TestTransferStreamZero()
        {
            const int SIZE = 1024;
            var source = new byte[SIZE];
            var rng = new Random( 10910 );
            rng.NextBytes( source );

            var sSource = new MemoryStream( source );

            const int OFFSET = SIZE / 100;
            const int COUNT = 0;

            sSource.Position = OFFSET;
            var sDest = new MemoryStream();
            Lib.TransferStream( sSource, sDest, 64, COUNT );

            Assert.AreEqual<long>( COUNT, sDest.Length, "Length of destination stream is wrong" );
        }

        [TestMethod]
        public void FindByteSubstringTest()
        {
            byte[] main = { 1, 2, 1, 1, 2, 1, 3, 2, 1 };
            byte[] t1 = { 2, 1, 1 };
            byte[] t2 = { 1, 2, 1, 3 };
            byte[] t3 = { 3, 2, 1, 4 };
            byte[] t4 = { 2, 1, 3 };

            var idx = main.FindByteSubstring( t1 );
            Assert.AreEqual( 1, idx, "String 1" );
            idx = main.FindByteSubstring( t2 );
            Assert.AreEqual( 3, idx, "String 2" );
            idx = main.FindByteSubstring( t3 );
            Assert.AreEqual( -1, idx, "String 3 not found" );
            idx = main.FindByteSubstring( t4 );
            Assert.AreEqual( 4, idx, "String 4" );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentException ) )]
        public void FindByteSubstringExceptionsTest()
        {
            byte[] main = { 1, 2, 1, 1, 2, 1, 3, 2, 1 };

            var idx = main.FindByteSubstring( null );
            Assert.AreEqual( -1, idx, "Null Find String" );
            idx = main.FindByteSubstring( new byte[] { } );
            Assert.AreEqual( -1, idx, "Empty Find String" );
            idx = main.FindByteSubstring( main, 1 );
            Assert.AreEqual( -1, idx, "Substring too long for starting position" );

            main.FindByteSubstring( main, -1 );
        }

        [TestMethod]
        public void AddSomethingToFilenameTest()
        {
            var s = Lib.AddSomethingToFilename( @"c:\temp\holy.txt", "cow" );
            Assert.AreEqual( @"c:\temp\holy.cow.txt", s, "Didn't add COW correctly" );
        }


        [TestMethod]
        public void RemoveDuplicateWhitespaceTest()
        {
            var testStr = " \t \n This  \tIs  \tIt\n!   ";
            var expected = "This Is It !";
            var actual = testStr.RemoveDuplicateWhitespace();
            Assert.AreEqual( expected, actual );
        }




    }
}
