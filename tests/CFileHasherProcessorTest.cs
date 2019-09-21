using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security.Cryptography;


namespace Morpheus.Standard.UnitTests
{
    [TestClass()]
    public class CFileHasherProcessorTest
    {
        // Uncomment this to test the full file processing
        //byte[] m_buffer = new byte[60 * 1024 * 1024];
        //string m_filename;

        //[TestInitialize]
        //public void InitializeBigFile()
        //{
        //    Random rng = new Random();
        //    rng.NextBytes( m_buffer );

        //    m_filename = CCrypto.BytesToString( Guid.NewGuid().ToByteArray() );
        //    FileInfo fi = new FileInfo( m_filename );
        //    Console.WriteLine( fi.FullName );
        //    FileStream f = fi.Create();
        //    f.Write( m_buffer, 0, m_buffer.Length );
        //    f.Close();
        //}

        //[TestCleanup]
        //public void CleanupBigFile()
        //{
        //    File.Delete( m_filename );
        //}


        ///// <summary>
        ///// Test the functionality of both classes using a large (time consuming) process
        ///// </summary>
        //[TestMethod()]
        //public void TestBasicFunctionality()
        //{
        //    HashAlgorithm hash = new MD5CryptoServiceProvider();
        //    int hashSize = hash.HashSize / 8;

        //    CFileHasher hasher = new CFileHasher( hash );
        //    hasher.InternalBufferSize = 1024 * 1024;

        //    int inits = 0, processes = 0, finishes = 0;

        //    hasher.OnInitialize += () => inits++;
        //    hasher.OnProcessBytes += ( arr, off, cnt ) => processes++;
        //    hasher.OnFinished += () => finishes++;


        //    hasher.ProcessFile( m_filename, true );
        //    Assert.AreEqual<long>( m_buffer.Length, hasher.Count, "Size of file should equal size of buffer" );
        //    byte[] actualHash = new byte[hashSize];
        //    Array.Copy( hasher.HashValue, actualHash, hashSize );

        //    hash.Initialize();
        //    byte[] expected = hash.ComputeHash( m_buffer );

        //    Assert.AreEqual<int>( hash.HashSize, hasher.HashSize, "Hash Sizes should be equal" );
        //    for (int i = 0; i < hashSize; i++)
        //    {
        //        Assert.AreEqual<byte>( expected[i], actualHash[i], "Hash values are wrong at index " + i );
        //    }

        //    int expectedChunks = m_buffer.Length / hasher.InternalBufferSize;
        //    Assert.AreEqual<int>( expectedChunks, hasher.ChunksRead, "Chunks Read" );
        //    Assert.AreEqual<int>( expectedChunks, hasher.EstimatedChunks, "Estimated Chunks" );
        //    Assert.IsNull( hasher.AsyncException, "Async Exception" );
        //    Assert.IsFalse( hasher.HasIoException, "Has IO Exception" );
        //    Assert.IsFalse( hasher.HasProcessingException, "Has Processing Exception" );
        //    Assert.IsTrue( hasher.ProcessingPercentage < 100, "Processing Percentage" );

        //    Assert.AreEqual<int>( 1, inits, "Init Count" );
        //    Assert.AreEqual<int>( 1, finishes, "Finish Count" );
        //    Assert.AreEqual<int>( expectedChunks, processes, "Processes Count" );
        //}


        //[TestMethod]
        //public void TestConstructorsDefault()
        //{
        //    CFileHasher_Accessor ha = new CFileHasher_Accessor();
        //    HashAlgorithm h = new MD5CryptoServiceProvider();

        //    byte[] buf = new byte[1024];
        //    new Random().NextBytes( buf );

        //    byte[] expected = h.ComputeHash( buf );
        //    byte[] actual = ha.m_hashAlgorithm.ComputeHash( buf );

        //    Assert.AreEqual<int>( expected.Length, actual.Length, "Length" );
        //    for (int i = 0; i < expected.Length; i++)
        //        Assert.AreEqual<byte>( expected[i], actual[i], "Incorrect hash byte at index " + i );
        //}

        //[TestMethod]
        //public void TestConstructorsSha1()
        //{
        //    CFileHasher_Accessor ha = new CFileHasher_Accessor("SHA1");
        //    HashAlgorithm h = new SHA1CryptoServiceProvider();

        //    byte[] buf = new byte[1024];
        //    new Random().NextBytes( buf );

        //    byte[] expected = h.ComputeHash( buf );
        //    byte[] actual = ha.m_hashAlgorithm.ComputeHash( buf );

        //    Assert.AreEqual<int>( expected.Length, actual.Length, "Length" );
        //    for (int i = 0; i < expected.Length; i++)
        //        Assert.AreEqual<byte>( expected[i], actual[i], "Incorrect hash byte at index " + i );
        //}

    }
}
