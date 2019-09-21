using System;
using System.Security.Cryptography;

namespace Morpheus
{
    /// <summary>
    /// This class encapsulates a CRC-16 algorithm based on the CCITT polynomial.
    /// </summary>
    /// <remarks>
    /// This is a table-based algorithm which was tested as much faster than a purely
    /// bit-manipulation method.
    /// 
    /// This algorithm is faster than using <see cref="MD5CryptoServiceProvider"/> when the
    /// size of the data is approximately 5k or less. For data sizes greater than 5k-bytes,
    /// the MD5 calculation becomes faster.
    /// 
    /// The larger the dataset, the better MD5 performs compared to this algorithm.
    /// 
    /// Another significant difference is that MD5 produces a 16-byte result, where CCITT-16
    /// produces a 2-byte result.
    /// </remarks>
    public class CCrc16_CCITT
    {
        private static readonly uint[] sm_crcTable = new uint[256];

        static CCrc16_CCITT()
        {
            for (uint i = 0; i < 256; i++)
            {
                var CRC16 = i;
                for (var n = 0; n < 8; n++)
                {
                    if ((CRC16 & 1) != 0)
                        CRC16 = (CRC16 >> 1) ^ 0x8408;
                    else
                        CRC16 >>= 1;
                }
                sm_crcTable[i] = CRC16;
            }
        }

        /// <summary>
        /// Calculate the CCITT-CRC16 over an entire byte array
        /// </summary>
        /// <param name="_data">The data to calculate the CRC over</param>
        /// <returns>The CRC16 for the data in the array</returns>
        public static ushort CalculateCrc( byte[] _data ) => CalculateCrc( _data, 0, _data.Length );

        /// <summary>
        /// Calculate the CCITT-CRC16 over a subset of a byte array
        /// </summary>
        /// <param name="_data">The data to calculate the CRC over</param>
        /// <param name="_offset">
        /// The offset into the buffer where to start calculating the CRC
        /// </param>
        /// <param name="_length">The number of bytes to include in the CRC</param>
        /// <returns>The CRC16 for the data in the array</returns>
        public static ushort CalculateCrc( byte[] _data, int _offset, int _length )
        {
            if (_offset < 0 || _offset > _data.Length)
                throw new ArgumentException( "_offset needs to be between 0 and _data.Length, inclusive" );

            if (_length < 0)
                _length = _data.Length;

            var lastIndex = Math.Min( _offset + _length, _data.Length );

            uint CRC = 0xffff;

            for (var i = _offset; i < lastIndex; i++)
            {
                uint C = _data[i];
                CRC = ((CRC >> 8) & 0x00ff) ^ sm_crcTable[(CRC ^ C) & 0x00ff];
            }
            return (ushort) (CRC ^ 0xffff);
        }

        /// <summary>
        /// This override will calculate the CCITT-CRC16 for a USHORT.
        /// </summary>
        /// <remarks>
        /// This method is used, for instance, to create the "pair" for a framing operation.
        /// For instance, the parameter to this method would represent the length of the
        /// frame and the return value of this method is appended that length and placed at
        /// the beginning of the frame.
        /// </remarks>
        /// <param name="_number">The 2-byte number to take the CRC of</param>
        /// <returns>The CCITT-CRC of the ushort parameter</returns>
        public static ushort CalculateCrc( ushort _number )
        {
            uint CRC = 0xffff;

            var C = (uint) (_number & 0xff);
            CRC = ((CRC >> 8) & 0x00ff) ^ sm_crcTable[(CRC ^ C) & 0x00ff];

            C = (uint) ((_number >> 8) & 0xff);
            CRC = ((CRC >> 8) & 0x00ff) ^ sm_crcTable[(CRC ^ C) & 0x00ff];

            return (ushort) (CRC ^ 0xffff);
        }

        /// <summary>
        /// This override will calculate the CCITT-CRC16 for an Int32.
        /// </summary>
        /// <remarks>
        /// For example, the parameter to this method would represent the length of
        /// something that, coupled with its CRC, may be a checksum of sorts
        /// </remarks>
        /// <param name="_number">
        /// The 4-byte number to take the CRC of. Sign is irrelevant.
        /// </param>
        /// <returns>The CCITT-CRC of number supplied</returns>
        public static ushort CalculateCrc( int _number )
        {
            uint CRC = 0xffff;

            var C = (uint) (_number & 0xff);
            CRC = ((CRC >> 8) & 0x00ff) ^ sm_crcTable[(CRC ^ C) & 0x00ff];

            C = (uint) ((_number >> 8) & 0xff);
            CRC = ((CRC >> 8) & 0x00ff) ^ sm_crcTable[(CRC ^ C) & 0x00ff];

            C = (uint) ((_number >> 16) & 0xff);
            CRC = ((CRC >> 8) & 0x00ff) ^ sm_crcTable[(CRC ^ C) & 0x00ff];

            C = (uint) ((_number >> 24) & 0xff);
            CRC = ((CRC >> 8) & 0x00ff) ^ sm_crcTable[(CRC ^ C) & 0x00ff];

            return (ushort) (CRC ^ 0xffff);
        }
    }
}
