using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Morpheus.Standard.UnitTests
{
    /// <summary>
    /// Tests the Morpheus version of the <see cref="MemoryStream"/> class. Principal differences deal with the
    /// ability to flexibly "wrap" existing byte[] objects.
    /// </summary>
    [TestClass]
    public class CMemoryStreamTest
    {
        private static readonly Random sm_rng = new Random();

        private const int BUFFER_SIZE = 1024;
        private const int START_INDEX = 47;
        private const int SAMPLE_SIZE = 73;

        private CMemoryStream m_memStream;
        private byte[] m_buffer;

        private readonly byte[] m_buf1 = new byte[]
            {
                1, 2, 3, 4, 5, 6
            };

        private readonly byte[] m_buf2 = new byte[]
            {
                11, 22, 33, 44, 55, 66, 77, 88, 99
            };

        private readonly byte[] m_buf3 = new byte[]
            {
                12, 23, 34
            };


        [TestInitialize]
        public void SetMemStream()
        {
            SetBuffer( BUFFER_SIZE );
            m_memStream = new CMemoryStream( m_buffer );
        }

        private void SetBuffer( int _size )
        {
            m_buffer = new byte[_size];
            sm_rng.NextBytes( m_buffer );
        }


        [TestMethod]
        [TestCategory( "MemoryStream" )]
        public void TestConstructor()
        {
            var str = new CMemoryStream();
            Assert.AreEqual<long>( 0, str.Length, "Length of default stream should be zero" );
            Assert.AreEqual<long>( 0, str.Position, "Position of default stream should be zero" );
        }


        [TestMethod]
        [TestCategory( "MemoryStream" )]
        [ExpectedException( typeof( ArgumentException ), "Expected when Seeking to an invalid OFFSET value" )]
        public void TestInvalidSeek() => m_memStream.Seek( 345, (SeekOrigin) 234 );

        [TestMethod]
        [TestCategory( "MemoryStream" )]
        public void TestNegativeCount()
        {
            var buf = new byte[500];
            var count = m_memStream.Read( buf, 0, -1 );
            Assert.AreEqual<int>( 0, count, "Should read 0 bytes when asking for a negative number of bytes" );
        }

        [TestMethod]
        [TestCategory( "MemoryStream" )]
        public void TestBasicStuff()
        {
            Assert.AreEqual( true, m_memStream.CanRead, "CanRead" );
            Assert.AreEqual( true, m_memStream.CanWrite, "CanWrite" );
            Assert.AreEqual( true, m_memStream.CanSeek, "CanSeek" );
            Assert.AreEqual<long>( m_buffer.Length, m_memStream.Length, "Length" );
            Assert.AreEqual<long>( m_buffer.Length, m_memStream.BytesAvailable, "Bytes Available" );
            Assert.AreEqual<long>( 0, m_memStream.Position, "Default Position" );
        }


        [TestMethod]
        [TestCategory( "MemoryStream" )]
        public void TestReads()
        {
            var buffer = new byte[SAMPLE_SIZE];

            m_memStream.Position = START_INDEX;
            m_memStream.Read( buffer, 0, SAMPLE_SIZE );

            Assert.AreEqual( 0, MemCmp( buffer, 0, m_buffer, START_INDEX, SAMPLE_SIZE ), "Buffer read doesn't match (1)" );

            var buf2 = new byte[SAMPLE_SIZE + 37];
            m_memStream.Position = 0;
            m_memStream.Read( buf2, 37, SAMPLE_SIZE );

            Assert.AreEqual( 0, MemCmp( buf2, 37, m_buffer, 0, SAMPLE_SIZE ), "Buffer read doesn't match (2)" );

            m_memStream.Position = START_INDEX;
            m_memStream.Read( buffer, 0, 1 );
            m_memStream.Read( buffer, 1, 2 );
            m_memStream.Read( buffer, 3, 4 );
            m_memStream.Read( buffer, 7, 7 );
            m_memStream.Read( buffer, 14, 14 );

            Assert.AreEqual( 0, MemCmp( buffer, 0, m_buffer, START_INDEX, 28 ), "Small Buffer reads don't match (3)" );
        }

        [TestMethod]
        [TestCategory( "MemoryStream" )]
        public void TestWrites()
        {
            m_memStream.Write( m_buf1, 0, 3 );
            m_memStream.Write( m_buf2, 0, 3 );
            m_memStream.Write( m_buf3, 0, 3 );

            var expected = new byte[]
                {
                    1, 2, 3, 11, 22, 33, 12, 23, 34
                };
            Assert.AreEqual( 0, MemCmp( expected, 0, m_buffer, 0, expected.Length ), "Expected Stream Contents" );

            m_memStream.Position = START_INDEX;
            m_memStream.Write( m_buf2, 0, m_buf2.Length );
            Assert.AreEqual<long>( START_INDEX + m_buf2.Length, m_memStream.Position, "Stream Position" );
            Assert.AreEqual( 0, MemCmp( m_buf2, 0, m_buffer, START_INDEX, m_buf2.Length ), "Expected buf2 transfer" );
        }

        [TestMethod]
        [TestCategory( "MemoryStream" )]
        public void TestWriteSmall()
        {
            m_memStream.Write( m_buf1, 0, 1 );
            m_memStream.Write( m_buf2, 0, 2 );
            m_memStream.Write( m_buf3, 0, 0 );

            var expected = new byte[] { 1, 11, 22 };

            Assert.AreEqual<long>( expected.Length, m_memStream.Position, "Length is wrong." );

            m_memStream.Position = 0;
            for (var i = 0; i < expected.Length; i++)
                Assert.AreEqual<byte>( expected[i], (byte) m_memStream.ReadByte(), "The byte at index " + i + " is wrong." );
        }


        [TestMethod]
        [TestCategory( "MemoryStream" )]
        public void TestChangingBuffer()
        {
            var buffer = new byte[BUFFER_SIZE];

            m_memStream = new CMemoryStream( m_buf1 );
            m_memStream.Read( buffer, 0, m_buf1.Length );

            Assert.AreEqual<long>( m_buf1.Length, m_memStream.Position, "Position" );
            Assert.AreEqual<long>( m_buf1.Length, m_memStream.Length, "Length" );
            Assert.AreEqual<long>( 0, m_memStream.BytesAvailable, "Bytes Available" );

            Assert.AreEqual( 0, MemCmp( m_buf1, 0, buffer, 0, m_buf1.Length ), "Should have read buf1" );

            m_memStream.SetBuffer( m_buf2 );
            m_memStream.Read( buffer, 0, m_buf2.Length );

            Assert.AreEqual<long>( m_buf2.Length, m_memStream.Position, "Position2" );
            Assert.AreEqual<long>( m_buf2.Length, m_memStream.Length, "Length2" );
            Assert.AreEqual<long>( 0, m_memStream.BytesAvailable, "Bytes Available2" );

            Assert.AreEqual( 0, MemCmp( m_buf2, 0, buffer, 0, m_buf2.Length ), "Should have read buf2" );
        }


        [TestMethod]
        [TestCategory( "MemoryStream" )]
        public void TestInternalOperations()
        {
            m_memStream.SetBuffer( m_buffer, START_INDEX, SAMPLE_SIZE );

            Assert.AreEqual<long>( 0, m_memStream.Position, "Position" );
            Assert.AreEqual<long>( SAMPLE_SIZE, m_memStream.Length, "Length" );
            Assert.AreEqual<long>( SAMPLE_SIZE, m_memStream.BytesAvailable, "Bytes Available" );

            var buffer = new byte[BUFFER_SIZE];
            var bytesRead = m_memStream.Read( buffer, 0, BUFFER_SIZE );
            Assert.AreEqual<long>( SAMPLE_SIZE, bytesRead, "Bytes Read" );

            Assert.AreEqual( 0,
                             MemCmp( buffer, 0, m_buffer, START_INDEX, SAMPLE_SIZE ),
                             "Should have read correct subsection" );

            m_memStream.Position = 5;
            Assert.AreEqual<long>( 5, m_memStream.Position, "New Position" );
            Assert.AreEqual<long>( SAMPLE_SIZE, m_memStream.Length, "New Length" );
            Assert.AreEqual<long>( SAMPLE_SIZE - 5, m_memStream.BytesAvailable, "New Bytes Available" );
        }

        [TestMethod]
        [TestCategory( "MemoryStream" )]
        public void TestSeek()
        {
            m_memStream.Seek( 0, SeekOrigin.Begin );
            Assert.AreEqual<long>( 0, m_memStream.Position, "Position 0 from Begin" );

            m_memStream.Seek( 0, SeekOrigin.End );
            Assert.AreEqual<long>( m_memStream.Length, m_memStream.Position, "Position 0 from End" );

            m_memStream.Position = 10;
            m_memStream.Seek( -4, SeekOrigin.Current );
            Assert.AreEqual<long>( 6, m_memStream.Position, "Position -4 from Current (10)" );
        }

        [TestMethod]
        [TestCategory( "MemoryStream" )]
        public void TestNegativeAndLargePositions()
        {
            m_memStream.Position = -5;
            Assert.AreEqual<long>( 0, m_memStream.Position, "-5 Position" );

            m_memStream.Position = BUFFER_SIZE + 5;
            Assert.AreEqual<long>( BUFFER_SIZE, m_memStream.Position, "+5 Position" );
        }

        [TestMethod]
        [TestCategory( "MemoryStream" )]
        public void TestSetLength()
        {
            m_memStream.SetLength( 5 );

            Assert.AreEqual<long>( 5, m_memStream.Length, "Length is wrong" );
        }

        [TestMethod]
        [TestCategory( "MemoryStream" )]
        public void SetFlush()
        {
            var pos = m_memStream.Position;
            var len = m_memStream.Length;

            m_memStream.Flush(); // on a memory stream, this should do nothing really... definitely not generate errors.

            Assert.AreEqual<long>( pos, m_memStream.Position, "Position wrong after Flush" );
            Assert.AreEqual<long>( len, m_memStream.Length, "Length wrong after Flush" );
        }

        [TestMethod]
        [TestCategory( "MemoryStream" )]
        [ExpectedException( typeof( ArgumentNullException ), "Should be thrown when the BUFFER argument is NULL" )]
        public void TestSetBuffer_NullBuffer1() => m_memStream.SetBuffer( null );

        [TestMethod]
        [TestCategory( "MemoryStream" )]
        [ExpectedException( typeof( ArgumentNullException ), "Should be thrown when the BUFFER argument is NULL" )]
        public void TestSetBuffer_NullBuffer2() => m_memStream.SetBuffer( null, 0, 15 );

        [TestMethod]
        [TestCategory( "MemoryStream" )]
        [ExpectedException( typeof( ArgumentException ), "Should be thrown when the Length argument is Negative" )]
        public void TestSetBuffer_LengthNegative() => m_memStream.SetBuffer( m_buf1, 0, -15 );

        [TestMethod]
        [TestCategory( "MemoryStream" )]
        [ExpectedException( typeof( ArgumentException ), "Should be thrown when the Offset argument is Negative" )]
        public void TestSetBuffer_OffsetNegative() => m_memStream.SetBuffer( m_buf1, -15, 5 );

        [TestMethod]
        [TestCategory( "MemoryStream" )]
        [ExpectedException( typeof( ArgumentException ), "Should be thrown when the Offset + Length argument is too large for underlying array" )]
        public void TestSetBuffer_OffsetLengthTooBig() => m_memStream.SetBuffer( m_buf1, m_buf1.Length, 5 );



        [TestMethod]
        [TestCategory( "MemoryStream" )]
        [ExpectedException( typeof( ArgumentOutOfRangeException ), "Seek to position greater than -int- bounds" )]
        public void TestPosGreaterThanInt() => m_memStream.Seek( 0x100000000, SeekOrigin.Begin );

        [TestMethod]
        [TestCategory( "MemoryStream" )]
        [ExpectedException( typeof( ArgumentOutOfRangeException ), "SetLength was too big" )]
        public void TestSetLengthTooBig() => m_memStream.SetLength( m_memStream.Length * 2 );

        [TestMethod]
        [TestCategory( "MemoryStream" )]
        [ExpectedException( typeof( ArgumentNullException ), "Cannot read to a null buffer reference" )]
        public void TestReadToNullBuffer1()
        {
            m_memStream.Position = 0;
            m_memStream.Read( null, 0, 3 );
        }

        [TestMethod]
        [TestCategory( "MemoryStream" )]
        [ExpectedException( typeof( ArgumentNullException ), "Cannot Write from a null buffer reference" )]
        public void TestWriteFromNullBuffer1()
        {
            m_memStream.Position = 0;
            m_memStream.Write( null, 0, 3 );
        }

        [TestMethod]
        [TestCategory( "MemoryStream" )]
        [ExpectedException( typeof( ArgumentException ), "Wrote more bytes than the write-buffer had in it" )]
        public void TestMoreBytesSpecifiedToWrite() => m_memStream.Write( new byte[10], 0, m_buffer.Length );

        [TestMethod]
        [TestCategory( "MemoryStream" )]
        [
            ExpectedException( typeof( IndexOutOfRangeException ),
                "Wrote more bytes than would fit in the underlying array" )]
        public void TestReadToNullBuffer()
        {
            m_memStream.Seek( 3, SeekOrigin.End );
            m_memStream.Write( new byte[4], 0, 4 );
        }


        [TestMethod]
        [TestCategory( "MemoryStream" )]
        public void TestGetBytes()
        {
            var mem = new CMemoryStream( m_buf2, 2, 5 );
            var newbuf = mem.GetBytes();

            Assert.AreEqual( 0, MemCmp( newbuf, 0, m_buf2, 2, 5 ), "Bytes extracted from m_bu2 are wrong" );

            newbuf[2]++; // relies on the fact that m_buf2[2] is not 255!
            Assert.AreEqual( 1,
                             MemCmp( newbuf, 0, m_buf2, 2, 5 ),
                             "Bytes changed in newbuf should make the two arrays different." );
        }

        [TestMethod]
        [TestCategory( "MemoryStream" )]
        public void TestReadOnlyMemStream()
        {
            var mem = new CMemoryStream( m_buf2, 0, m_buf2.Length, true )
            {
                Position = 0
            };
            var reader = new BinaryReader( mem );
            var x = reader.ReadInt32();
            mem.Seek( -2, SeekOrigin.Current );

            Assert.AreEqual( true, mem.CanRead, "Should be CanRead" );
            Assert.AreEqual( false, mem.CanWrite, "Should not be CanWrite" );
            Assert.AreEqual( true, mem.CanSeek, "Should be CanSeek" );


            try
            {
                mem.SetLength( 4 );
                Assert.Fail( "Should not be able to set the length of a read-only stream." );
            }
            catch (InvalidOperationException)
            {
                // all is well
            }

            try
            {
                mem.WriteByte( 0 );
                Assert.Fail( "Should not be able to write to a readonly stream" );
            }
            catch (InvalidOperationException)
            {
                // all is well
            }

            try
            {
                mem.SetBuffer( m_buf1 );
                Assert.Fail( "Should not be able to set the underlying buffer in a readonly CMemoryStream." );
            }
            catch (InvalidOperationException)
            {
                // all is well
            }
        }


        private int MemCmp( byte[] _first, int _offset1, byte[] _second, int _offset2, int _count )
        {
            for (var i = 0; i < _count; i++)
            {
                if (_first[i + _offset1] != _second[i + _offset2])
                    return 1;
            }
            return 0;
        }
    }
}