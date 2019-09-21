using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Morpheus
{
    /// <summary>
    /// Generate random numbers from a provided byte array. Double values can be decoded as
    /// either <see cref="ulong"/> s or directly as <see cref="double"/> s. This makes a
    /// difference when using this class for testing, as the user may want to stuff
    /// <see cref="double"/> s into a <see cref="MemoryStream"/> using a
    /// <see cref="BinaryWriter"/> , which would encode them differently than the standard
    /// Decode works. So, you can specify whether or not to decode <see cref="double"/> values
    /// using either <see cref="BinaryReader"/> or the default <see cref="ulong"/> ratio model.
    /// </summary>
    public class NotRandomFromBytes : Random
    {
        private MemoryStream m_stream;
        private BinaryReader m_reader;
        private bool m_useBinaryReaderForDouble;


        /// <summary>
        /// Construct using a byte array. Optionally, indicate how to decode
        /// <see cref="double"/> values.
        /// </summary>
        /// <param name="bytes">The byte array forming the basis for</param>
        /// <param name="_useBinaryReaderForDoubles">
        /// If TRUE, then read <see cref="double"/> values as if they came from
        /// <see cref="BinaryWriter"/>
        /// </param>
        public NotRandomFromBytes( byte[] bytes, bool _useBinaryReaderForDoubles = false )
        {
            m_stream = new MemoryStream( bytes );
            m_reader = new BinaryReader( m_stream );
            m_useBinaryReaderForDouble = _useBinaryReaderForDoubles;
        }

        /// <summary>
        /// Remove enough bytes from the underlying stream to fill the buffer passed in.
        /// </summary>
        /// <param name="_buffer">The buffer to fill with bytes from the stream</param>
        public override void NextBytes( byte[] _buffer )
        {
            var bytesRead = m_reader.Read( _buffer, 0, _buffer.Length );
            if (bytesRead < _buffer.Length)
                throw new EndOfStreamException();
        }


        /// <summary>
        /// Use the LerpZeroToOne method
        /// </summary>
        /// <returns></returns>
        public override double NextDouble()
        {
            if (m_useBinaryReaderForDouble)
                return m_reader.ReadDouble();
            else
                return m_reader.ReadUInt64().LerpZeroToOne();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override double Sample() => NextDouble();
    }
}
