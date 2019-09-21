using System;
using System.IO;

namespace Morpheus
{
    /// <summary>
    /// A stripped down version of <see cref="MemoryStream"/> that allows the underlying
    /// buffer to be reassigned at will.
    /// </summary>
    public class CMemoryStream : Stream
    {
        /// <summary>
        /// Reference to the external buffer to use as a stream
        /// </summary>
        private byte[] m_buffer;

        /// <summary>
        /// The origin in the external buffer for the stream
        /// </summary>
        private int m_origin;

        /// <summary>
        /// The length of data within the external buffer that is valid to return
        /// </summary>
        private int m_length;

        /// <summary>
        /// The current position in the "stream" (offset from m_origin)
        /// </summary>
        private int m_position;

        /// <summary>
        /// When set, disable the ability to "Write" to this stream.
        /// </summary>
        private readonly bool m_readOnly = false;


        /// <summary>
        /// Default constructor creates a zero-length array as the "stream"
        /// </summary>
        public CMemoryStream()
        {
            SetBuffer( new byte[0] );
        }

        /// <summary>
        /// Initialize the CMemoryReaderStream with an entire byte[]
        /// </summary>
        /// <param name="_buffer">The byre[] to use with this object</param>
        public CMemoryStream( byte[] _buffer )
        {
            SetBuffer( _buffer );
        }

        /// <summary>
        /// Initialize the CMemoryReaderStream with a portion of a byte[]
        /// </summary>
        /// <param name="_buffer">The byre[] to use with this object</param>
        /// <param name="_offset">The offset into _buffer to start the stream</param>
        /// <param name="_length">The length of data in the buffer to use</param>
        public CMemoryStream( byte[] _buffer, int _offset, int _length )
        {
            SetBuffer( _buffer, _offset, _length );
        }

        /// <summary>
        /// Initialize the CMemoryReaderStream with a portion of a byte[]
        /// </summary>
        /// <param name="_buffer">The byre[] to use with this object</param>
        /// <param name="_offset">The offset into _buffer to start the stream</param>
        /// <param name="_length">The length of data in the buffer to use</param>
        /// <param name="_readOnly">Used to make this stream a "Read-only" stream.</param>
        public CMemoryStream( byte[] _buffer, int _offset, int _length, bool _readOnly )
        {
            SetBuffer( _buffer, _offset, _length );
            m_readOnly = _readOnly;
        }

        /// <summary>
        /// Set the internal buffer using an entire byte[]
        /// </summary>
        /// <param name="_buffer">The byre[] to use with this object</param>
        public void SetBuffer( byte[] _buffer )
        {
            if (_buffer == null)
                throw new ArgumentNullException( "_buffer must be non-null" );
            SetBuffer( _buffer, 0, _buffer.Length );
        }

        /// <summary>
        /// Set the buffer for the CMemoryReaderStream with a portion of a byte[]
        /// </summary>
        /// <param name="_buffer">The byre[] to use with this object</param>
        /// <param name="_offset">The offset into _buffer to start the stream</param>
        /// <param name="_length">The length of data in the buffer to use</param>
        public void SetBuffer( byte[] _buffer, int _offset, int _length )
        {
            if (_buffer == null)
                throw new ArgumentNullException( "_buffer must be non-null" );
            if (_length < 0 || _offset < 0)
                throw new ArgumentException( "Length and Offset must be positive integers", "_length, _offset" );
            if (_offset + _length > _buffer.Length)
            {
                throw new ArgumentException( "The count plus the offset would exceed the buffer size.",
                                             "_count, _offset" );
            }
            if (m_readOnly)
            {
                throw new InvalidOperationException(
                    "May not use SetBuffer on a stream that's been constructed as ReadOnly." );
            }

            m_buffer = _buffer;
            m_origin = _offset;
            m_length = _length;
            m_position = 0;
        }

        /// <summary>
        /// Can you read from this stream?
        /// </summary>
        public override bool CanRead => true;

        /// <summary>
        /// Can you write to this stream?
        /// </summary>
        public override bool CanWrite => !m_readOnly;

        /// <summary>
        /// Can you Seek within this stream?
        /// </summary>
        public override bool CanSeek => true;

        /// <summary>
        /// How many bytes are left in this stream
        /// </summary>
        public override long Length => m_length;

        /// <summary>
        /// The number of bytes available to read in the stream.
        /// </summary>
        public int BytesAvailable => m_length - m_position;

        /// <summary>
        /// The number of bytes into the stream that the current position is.
        /// </summary>
        public override long Position
        {
            get => m_position;
            set
            {
                m_position = (int) value;
                if (m_position < 0)
                    m_position = 0;
                if (m_position > m_length)
                    m_position = m_length;
            }
        }

        /// <summary>
        /// Move the internal pointer to a specified position.
        /// </summary>
        /// <param name="_offset">
        /// The offset from the _origin to move the pointer to
        /// </param>
        /// <param name="_origin">
        /// Describes how to move the pointer based on the offset
        /// </param>
        /// <returns>The new pointer value.</returns>
        public override long Seek( long _offset, SeekOrigin _origin )
        {
            if (_offset > 0xffffffff)
            {
                throw new ArgumentOutOfRangeException( "_offset",
                                                       "The offset is too large for this operation on this object (must be an int compatible)" );
            }

            var offset = (int) _offset;

            switch (_origin)
            {
            case SeekOrigin.Begin:
                Position = offset;
                break;

            case SeekOrigin.Current:
                Position += offset;
                break;

            case SeekOrigin.End:
                Position = m_length - offset;
                break;

            default:
                throw new ArgumentException( "Invalid value for 'origin'", "_origin" );
            }

            return Position;
        }

        /// <summary>
        /// Read from the stream into a subsection of a byte[]
        /// </summary>
        /// <param name="_buffer">The byte[] to read into</param>
        /// <param name="_offset">
        /// The position in the byte[] of the first byte to read
        /// </param>
        /// <param name="_count">
        /// The number of bytes to read into the byte[]. Fewer bytes may be read than
        /// requested.
        /// </param>
        /// <returns>The number of bytes actually read into the _buffer.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when _buffer is null.
        /// </exception>
        public override int Read( byte[] _buffer, int _offset, int _count )
        {
            if (_buffer == null)
                throw new ArgumentNullException( "_buffer" );

            var bytesToRead = (m_length - m_position < _count) ? m_length - m_position : _count;

            // Putting in these specific checks helps performance for BinaryReader-type
            // access by well over 30%
            if (bytesToRead == 1)
            {
                var pos = m_origin + m_position;
                m_position += 1;

                _buffer[_offset] = m_buffer[pos];

                return 1;
            }

            if (bytesToRead == 2)
            {
                var pos = m_origin + m_position;
                m_position += 2;

                _buffer[_offset] = m_buffer[pos];
                _buffer[_offset + 1] = m_buffer[pos + 1];

                return 2;
            }

            if (bytesToRead == 4)
            {
                var pos = m_origin + m_position;
                m_position += 4;

                _buffer[_offset] = m_buffer[pos];
                _buffer[_offset + 1] = m_buffer[pos + 1];
                _buffer[_offset + 2] = m_buffer[pos + 2];
                _buffer[_offset + 3] = m_buffer[pos + 3];

                return 4;
            }

            if (bytesToRead <= 0)
                return 0;

            if (bytesToRead <= 8)
            {
                for (var i = 0; i < bytesToRead; i++)
                {
                    _buffer[_offset + i] = m_buffer[m_origin + m_position + i];
                }
            }
            else
            {
                Buffer.BlockCopy( m_buffer, m_origin + m_position, _buffer, _offset, bytesToRead );
            }

            m_position += bytesToRead;
            return bytesToRead;
        }

        /// <summary>
        /// Write data to the underlying byte[]
        /// </summary>
        /// <param name="_buffer">The data to write to the byte[]</param>
        /// <param name="_offset">The offset into the data to write</param>
        /// <param name="_count">The number of bytes to write into the byte[]</param>
        public override void Write( byte[] _buffer, int _offset, int _count )
        {
            if (m_readOnly)
                throw new InvalidOperationException( "May not Write to a stream that's been constructed as ReadOnly." );

            if (_buffer == null)
                throw new ArgumentNullException( "_buffer" );

            var pos = m_origin + m_position;

            if (_count == 4)
            {
                m_buffer[pos] = _buffer[_offset];
                m_buffer[pos + 1] = _buffer[_offset + 1];
                m_buffer[pos + 2] = _buffer[_offset + 2];
                m_buffer[pos + 3] = _buffer[_offset + 3];

                m_position += 4;
                return;
            }

            if (_count == 2)
            {
                m_buffer[pos] = _buffer[_offset];
                m_buffer[pos + 1] = _buffer[_offset + 1];

                m_position += 2;
                return;
            }

            if (_count == 1)
            {
                m_buffer[pos] = _buffer[_offset];

                m_position += 1;
                return;
            }

            if (_count <= 0)
                return;

            if (_count <= 8)
            {
                for (var i = 0; i < _count; i++)
                {
                    m_buffer[pos + i] = _buffer[_offset + i];
                }
            }
            else
            {
                Buffer.BlockCopy( _buffer, _offset, m_buffer, m_position + m_origin, _count );
            }

            m_position += _count;
        }


        /// <summary>
        /// Flushing this buffer does nothing.
        /// </summary>
        public override void Flush()
        {
        }

        /// <summary>
        /// Set the stream length. Check to see if the underlying array can handle it.
        /// </summary>
        /// <param name="_newLength">The new "length" of the stream</param>
        public override void SetLength( long _newLength )
        {
            if (m_origin + _newLength > m_buffer.Length)
            {
                throw new ArgumentOutOfRangeException( "The new length would exceed the size of the underlying array.",
                                                       "_newLength" );
            }
            if (m_readOnly)
            {
                throw new InvalidOperationException(
                    "May not use SetLength on a stream that's been constructed as ReadOnly." );
            }

            m_length = (int) _newLength;
        }

        /// <summary>
        /// Copy the contents of the underlying array into a new byte[] specifically sized
        /// to the Length of this stream.
        /// </summary>
        /// <returns>a byte[] copy of the underlying stream data.</returns>
        public byte[] GetBytes()
        {
            var retval = new byte[Length];
            Buffer.BlockCopy( m_buffer, m_origin, retval, 0, (int) Length );
            return retval;
        }
    }
}
