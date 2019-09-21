using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Morpheus
{
    /// <summary>
    /// This class will calculate the hash value for a given file/stream using either MD5 or whatever
    /// other <see cref="HashAlgorithm"/> specified by the application
    /// </summary>
    public class CFileHasher : CFileProcessor
    {
        /// <summary>
        /// The hash algorithm used by this object instance
        /// </summary>
        private readonly HashAlgorithm m_hashAlgorithm;

        /// <summary>
        /// Default algorithm uses MD5
        /// </summary>
        public CFileHasher()
        {
            m_hashAlgorithm = new MD5CryptoServiceProvider();
        }

        /// <summary>
        /// Specify the type of hashing that the app wants to use
        /// </summary>
        /// <param name="_hashAlgorithm">The hash algorithm used by this instance</param>
        public CFileHasher( HashAlgorithm _hashAlgorithm )
        {
            m_hashAlgorithm = _hashAlgorithm;
            AssertValidHashAlgorithm();
        }

        /// <summary>
        /// Create a hash algorithm from a name. See <see cref="HashAlgorithm.Create(string)"/> for
        /// more information. 
        /// </summary>
        /// <param name="_hashAlgorithmName">"MD5", "SHA[1|256|384|512]", etc.</param>
        public CFileHasher( string _hashAlgorithmName )
        {
            m_hashAlgorithm = HashAlgorithm.Create( _hashAlgorithmName );
            AssertValidHashAlgorithm();
        }

        /// <summary>
        /// Helper function to make sure that the selected hash algorithm supports what it needs to.
        /// </summary>
        private void AssertValidHashAlgorithm()
        {
            if (!m_hashAlgorithm.CanReuseTransform)
            {
                throw new ArgumentException( "The HashAlgorithm supplied (" +
                    m_hashAlgorithm.GetType().ToString() +
                    ") does not support Reuse. It must be re-usable to use it for this function." );
            }

            if (!m_hashAlgorithm.CanTransformMultipleBlocks)
            {
                throw new ArgumentException( "The HashAlgorithm supplied (" +
                    m_hashAlgorithm.GetType().ToString() +
                    ") does not support Multiple Blocks. It must support multiple blocks in order to use it for this function." );
            }
        }

        /// <summary>
        /// Start using the hash algorithm
        /// </summary>
        protected override void Initialize() => m_hashAlgorithm.Initialize();

        /// <summary>
        /// Transform a block of data into the hash
        /// </summary>
        /// <param name="_buffer"></param>
        /// <param name="_offset"></param>
        /// <param name="_count"></param>
        protected override void ProcessBytes( byte[] _buffer, int _offset, int _count ) => m_hashAlgorithm.TransformBlock( _buffer, _offset, _count, _buffer, _offset );

        /// <summary>
        /// Finish off the transformation of the file data
        /// </summary>
        protected override void Finished()
        {
            var endbuf = new byte[0];
            m_hashAlgorithm.TransformFinalBlock( endbuf, 0, 0 );
        }

        /// <summary>
        /// The Hash computed by the processor
        /// </summary>
        public byte[] HashValue => m_hashAlgorithm.Hash;

        /// <summary>
        /// The number of bytes comprizing the hash
        /// </summary>
        public int HashSize => m_hashAlgorithm.HashSize;

        /// <summary>
        /// The hash value as a hex string
        /// </summary>
        public string HashString
        {
            get
            {
                var s = new StringBuilder();
                foreach (var b in HashValue)
                    s.AppendFormat( "{0:x2}", b );
                return s.ToString();
            }
        }
    }
}
