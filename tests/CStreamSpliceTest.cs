using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;


namespace Morpheus.Standard.UnitTests
{
    /// <summary>
    /// This is a test class for CStreamSplice and is intended to contain all CStreamSplice Unit Tests
    ///</summary>
    [TestClass()]
    public class CStreamSpliceTest
    {
        private MemoryStream m_memStream = null;
        private StreamSplice m_splice = null;

        [TestInitialize]
        public void Initialize()
        {
            if (m_memStream == null)
            {
                m_memStream = new MemoryStream();
                for (var i = 0; i < 200; i++)
                    m_memStream.WriteByte( (byte) i );

                m_memStream.Position = 100;
                m_splice = new StreamSplice( m_memStream, 50 );
            }
        }

        [TestMethod]
        public void TestSplice()
        {
            Assert.AreEqual<long>( 50, m_splice.Length, "Length of Splice" );

            var buf = new byte[1024];
            var len = m_splice.Read( buf, 0, buf.Length );
            Assert.AreEqual<int>( 50, len, "# of bytes read is wrong" );

            for (var i = 0; i < len; i++)
                Assert.AreEqual<byte>( (byte) (i + 100), buf[i], "Byte at index is wrong: " + i );

            m_splice.Position = 25;
            len = m_splice.Read( buf, 0, buf.Length );
            Assert.AreEqual<int>( 25, len, "# of bytes wrong after positioning #1" );

            for (var i = 0; i < len; i++)
                Assert.AreEqual<byte>( (byte) (i + 125), buf[i], "Byte at index is wrong: " + i );

            m_splice.Seek( 10, SeekOrigin.Begin );
            Assert.AreEqual<long>( 10, m_splice.Position, "Position after Seek-Begin" );

            m_splice.Seek( 10, SeekOrigin.End );
            Assert.AreEqual<long>( 40, m_splice.Position, "Position after Seek-End" );

            m_splice.Seek( -10, SeekOrigin.Current );
            Assert.AreEqual<long>( 30, m_splice.Position, "Position after Seek-Current" );

            Assert.IsTrue( m_splice.CanRead, "CanRead" );
            Assert.IsTrue( m_splice.CanSeek, "CanSeek" );
            Assert.IsFalse( m_splice.CanWrite, "CanWrite" );

            m_splice.Position = 13;
            m_splice.Seek( 3, (SeekOrigin) 665 );
            Assert.AreEqual<long>( 13, m_splice.Position, "Position after invalid SeekOrigin" );

            m_splice.Flush();
            Assert.AreEqual<long>( 13, m_splice.Position, "Position after Flush" );
        }

        [TestMethod]
        public void TestTruncatedRead()
        {
            var buf = new byte[25];

            m_splice.Position = 0;
            var len = m_splice.Read( buf, 5, 100 );
            Assert.AreEqual<int>( 20, len, "Length of bytes read from truncated buffer" );
        }

        [TestMethod]
        [ExpectedException( typeof( NotImplementedException ), "Expected Exception for Write" )]
        public void TestWrite() => m_splice.Write( new byte[] { 1, 2, 3 }, 0, 3 );

        [TestMethod]
        [ExpectedException( typeof( NotImplementedException ), "Expected Exception for SetLength" )]
        public void TestSetLength() => m_splice.SetLength( 10 );

        [TestMethod]
        [ExpectedException( typeof( ArgumentOutOfRangeException ), "Expected Exception for invalid Position" )]
        public void TestInvalidPosition() => m_splice.Position = 1000;

        [TestMethod]
        [ExpectedException( typeof( ArgumentOutOfRangeException ), "Expected Exception for invalid Position Negative" )]
        public void TestInvalidPositionNegative() => m_splice.Position = -1000;

        [TestMethod]
        [ExpectedException( typeof( ArgumentOutOfRangeException ), "Expected Exception for negative Read offset" )]
        public void TestInvalidRead()
        {
            var buf = new byte[1000];
            m_splice.Position = 0;
            m_splice.Read( buf, -1, 10 );
        }

        private class CTestStream : Stream
        {
            public bool _CanRead = false;
            public override bool CanRead => _CanRead;

            public bool _CanSeek = false;
            public override bool CanSeek => _CanSeek;

            public bool _CanWrite = false;
            public override bool CanWrite => _CanWrite;

            public override void Flush() => throw new NotImplementedException();

            public override long Length => throw new NotImplementedException();

            public override long Position
            {
                get => throw new NotImplementedException();
                set => throw new NotImplementedException();
            }

            public override int Read( byte[] buffer, int offset, int count ) => throw new NotImplementedException();

            public override long Seek( long offset, SeekOrigin origin ) => throw new NotImplementedException();

            public override void SetLength( long value ) => throw new NotImplementedException();

            public override void Write( byte[] buffer, int offset, int count ) => throw new NotImplementedException();
        }


        [TestMethod]
        [ExpectedException( typeof( ArgumentException ), "Expected Exception from Constructor" )]
        public void TestWrite1()
        {
            var t = new CTestStream();
            var splice = new StreamSplice( t, 0 );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentException ), "Expected Exception from Constructor" )]
        public void TestWrite2()
        {
            var t = new CTestStream
            {
                _CanRead = true
            };
            var splice = new StreamSplice( t, 0 );
        }

    }
}
