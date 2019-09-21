using System;
using System.IO;

namespace Morpheus
{
    /// <summary>
    /// A StreamSplice is a section of a larger stream. The limitations of this sub-stream are
    /// enforced by this splice to make sure consumers do not move the "position" out of bounds
    /// of the splice.
    /// </summary>
    public class StreamSplice : Stream
    {
        private readonly Stream m_baseStream;
        private readonly long m_basePosition;
        private readonly long m_length;

        /// <summary>
        /// Construct a "splice" of a Stream that starts at the current Position of the stream
        /// passed in, and has a specific length.
        /// </summary>
        /// <param name="_stream">
        /// The "base" stream, generally a larger stream that this object breaks into a smaller
        /// part of.
        /// </param>
        /// <param name="_length">The length of this stream-splice.</param>
        public StreamSplice( Stream _stream, long _length )
        {
            if (!_stream.CanRead || !_stream.CanSeek)
                throw new ArgumentException( "Not allowed to construct a StreamSplice out of a stream that cannot be read or seek'ed." );

            m_baseStream = _stream;
            m_basePosition = m_baseStream.Position;
            m_length = _length;
        }


        /// <summary>
        /// For a stream-splice, always TRUE
        /// </summary>
        public override bool CanRead => true;

        /// <summary>
        /// For a stream-splice, always TRUE
        /// </summary>
        public override bool CanSeek => true;

        /// <summary>
        /// For a stream-splice, always FALSE
        /// </summary>
        public override bool CanWrite => false;

        /// <summary>
        /// This does nothing to a stream-splice
        /// </summary>
        public override void Flush()
        {
        }

        /// <summary>
        /// Gets the total Length of the Stream-splice
        /// </summary>
        public override long Length => m_length;

        /// <summary>
        /// Gets / Sets the Position within the StreamSplice.
        /// </summary>
        public override long Position
        {
            get => m_baseStream.Position - m_basePosition;
            set
            {
                if (value < 0 || value > m_length)
                    throw new ArgumentOutOfRangeException( "The position must be set to a value between 0 and " + m_length );

                m_baseStream.Position = m_basePosition + value;
            }
        }

        /// <summary>
        /// Sets the Position of the StreamSplice based on a "SeekOrigin".
        /// </summary>
        /// <param name="_offset">The offset for the seek operation</param>
        /// <param name="_origin">Information about where the offset is to be applied</param>
        /// <returns>The Postion within the stream after the offset is applied.</returns>
        public override long Seek( long _offset, SeekOrigin _origin )
        {
            switch (_origin)
            {
            case SeekOrigin.Begin:
                Position = _offset;
                break;
            case SeekOrigin.Current:
                Position += _offset;
                break;
            case SeekOrigin.End:
                Position = m_length - _offset;
                break;
            default:
                break;
            }

            return Position;
        }

        /// <summary>
        /// Unsupported by the StreamSplice
        /// </summary>
        /// <param name="_value">The new length of the stream</param>
        public override void SetLength( long _value ) => throw new NotImplementedException( "Changing the length of a StreamSplice is not allowable." );


        /// <summary>
        /// Read a section of the StreamSplice and return the number of bytes read
        /// </summary>
        /// <param name="_buffer">The buffer to hold the data read from the stream</param>
        /// <param name="_offset">
        /// The offset from the beginning of the buffer where to start saving the bytes from the
        /// stream
        /// </param>
        /// <param name="_count">The number of bytes to read from the stream</param>
        /// <returns>The number of bytes actually read from the stream</returns>
        public override int Read( byte[] _buffer, int _offset, int _count )
        {
            if (_offset < 0 || _offset > _buffer.Length)
                throw new ArgumentOutOfRangeException( "The offset must be between 0 and the length of the input buffer." );

            var bytesLeft = (int) (m_length - Position);
            if (_count > bytesLeft)
                _count = bytesLeft;

            if (_offset + _count > _buffer.Length)
                _count = _buffer.Length - _offset;

            return m_baseStream.Read( _buffer, _offset, _count );
        }

        /// <summary>
        /// Unsupported by the StreamSplice
        /// </summary>
        /// <param name="_buffer"></param>
        /// <param name="_offset"></param>
        /// <param name="_count"></param>
        public override void Write( byte[] _buffer, int _offset, int _count ) => throw new NotImplementedException( "Writing to a StreamSplice is not allowed." );
    }
}
