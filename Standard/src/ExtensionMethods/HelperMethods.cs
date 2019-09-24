using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Morpheus
{
    /// <summary>
    /// This class contains "helper" methods for varying things.
    /// </summary>
    public static class CHelper
    {
        /// <summary>
        /// This is a very commonly used REGEX that simply identifies numbers of any length. It
        /// will not recognize a number with commas in it as a single number. For example,
        /// "1,234" will be recognized as two numbers- "1" and "234".
        /// </summary>
        public static Regex NumberFindingRegex = new Regex( @"-?\d+", RegexOptions.Compiled );
        private static readonly Regex sm_getFirstNumberRegex = new Regex( @"-?\d+", RegexOptions.Compiled );

        /// <summary>
        /// A bit more flexible than <see cref="int.Parse(string)"/> , this routine will find
        /// the first integer in the string and turn that into an integer.
        /// </summary>
        /// <param name="_stringWithNumber">A string with an integer embedded in it.</param>
        /// <returns>The first number found in the string as an integer</returns>
        public static int GetFirstNumberInString( this string _stringWithNumber )
        {
            var m = sm_getFirstNumberRegex.Match( _stringWithNumber );
            if (!m.Success)
            {
                throw new FormatException(
                    "The argument passed to this method does not have a number embedded within it" );
            }
            return int.Parse( m.Value );
        }


        /// <summary>
        /// Convert an array of values, each assumed to be ".ToString"'able, into a
        /// comma-separated list of values, represented as a string.
        /// </summary>
        /// <param name="_array">The array to convert</param>
        /// <returns>A string representing the comma-separated list of values.</returns>
        public static string ConvertArrayToString( this Array _array )
        {
            if (_array == null)
                return "";

            var str = new StringBuilder();
            var len = _array.Length;
            var lowerBound = _array.GetLowerBound( 0 );
            for (var i = 0; i < len; i++)
            // This is fractionally (.6%) faster than foreach in this implementation, so use it.
            {
                var o = _array.GetValue( i + lowerBound );
                str.Append( o.ToString() );
                str.Append( "," );
            }

            if (str.Length > 0)
                str.Length--;

            return str.ToString();
        }

        /// <summary>
        /// Convert a string containing a comma-separated list of values into an array of values
        /// of the type specified
        /// </summary>
        /// <param name="_string">The comma-separated list</param>
        /// <param name="_arrayType">The type of each element in the array to return.</param>
        /// <returns>An Array object containing the values retrieved from the CSList</returns>
        public static Array ConvertStringToArray( this string _string, Type _arrayType )
        {
            if (_string == null)
                return null;
            if (_string == "")
                return Array.CreateInstance( _arrayType, 0 );

            var vals = _string.Split( ',' );

            var arr = Array.CreateInstance( _arrayType, vals.Length );
            for (var i = 0; i < vals.Length; i++)
            {
                arr.SetValue( Convert.ChangeType( vals[i], _arrayType ), i );
            }

            return arr;
        }

        /// <summary>
        /// Convert a string containing a comma-separated list of values into an array of values
        /// of the type specified
        /// </summary>
        /// <typeparam name="T">
        /// The Type that each element in the separated-list-string
        /// </typeparam>
        /// <param name="_string">The comma-separated list</param>
        /// <param name="_separator">
        /// The Separator used to distinguish individual elements in the list
        /// </param>
        /// <returns>The array created from the elements in the list.</returns>
        public static T[] ConvertStringToArray<T>( this string _string, char _separator )
        {
            if (_string == null)
                return null;
            if (_string == "")
                return new T[0];

            var vals = _string.Split( _separator );
            var elemType = typeof( T );

            var arr = new T[vals.Length];
            for (var i = 0; i < vals.Length; i++)
            {
                arr[i] = (T) Convert.ChangeType( vals[i], elemType );
            }

            return arr;
        }

        /// <summary>
        /// Use this method successively to extract individual strings that have been separated
        /// by some "separator" character. The first call should pass -1 in for _index.
        /// </summary>
        /// <remarks>
        /// This method is 8%-10% faster than using String.Split . It is written to minimize
        /// overheads from arrays, enumerators, etc. An alternate method was written that used
        /// "yield", but that was found to be SLOWER than String.Split . As such, if performance
        /// is truly an issue, this method is available, but it is clunky to use and
        /// String.Split should be used in general circumstances.
        /// </remarks>
        /// <example>
        /// <code>
        /// string sentence = "Take thy beak from out my heart, and take thy form from off my door";
        /// string token;
        /// int len = 0, idx = -1;
        /// do
        /// {
        ///     token = CHelper.ParseNextSegment( sentence, ' ', ref idx, ref len );
        ///     if (token != null)
        ///     {
        ///         // The token is usable in this block
        ///     }
        /// } while (token != null);
        /// </code>
        /// </example>
        /// <param name="_string">The string that is being parsed</param>
        /// <param name="_index">
        /// The "current index"- The index in the string where the current substring starts.
        /// Must pass in -1 to initialize the routine.
        /// </param>
        /// <param name="_length">The Length of the current substring</param>
        /// <param name="_separator">
        /// The separator used to separate substrings from each other
        /// </param>
        /// <returns>The "next" substring, or NULL if there are no more substrings</returns>
        public static string ParseNextSegment( string _string, char _separator, ref int _index, ref int _length )
        {
            if (_index == -1)
            {
                _index = 0;
                _length = _string.IndexOf( _separator );
            }
            else
            {
                _index += _length + 1;
            }

            if (_index >= _string.Length) // end of string
                return null;

            var idxNext = _string.IndexOf( _separator, _index );
            if (idxNext >= _index)
            {
                _length = idxNext - _index;
                return _string.Substring( _index, _length );
            }
            else // if (idxNext == -1 && _index < _string.Length)
            {
                _length = _string.Length - _index;
                return _string.Substring( _index );
            }
        }


        /// <summary>
        /// Given a "simple" array and a new object to add to the array, increase the simple
        /// array's length by 1 and set the last element in the new array to the "new" object.
        /// This involves a new allocation and copy operation for the array, so this should be
        /// used only as a non-performant convenience method.
        /// </summary>
        /// <typeparam name="TElementType">The Type of the array</typeparam>
        /// <param name="_array">
        /// The array itself which is to be lengthened and have the item added
        /// </param>
        /// <param name="_newObject">
        /// The object to add to the end of the lengthened array
        /// </param>
        public static void AppendElementToArray<TElementType>( ref TElementType[] _array, TElementType _newObject )
        {
            var len = (_array == null) ? 0 : _array.Length;
            Array.Resize<TElementType>( ref _array, len + 1 );
            _array[len] = _newObject;
        }

        /// <summary>
        /// Compress one byte array into another
        /// </summary>
        /// <param name="_bytes">The array to compress</param>
        /// <param name="_offset">
        /// The offset into the array from where to start compressing
        /// </param>
        /// <param name="_length">The number of bytes, starting at _offset, to compress</param>
        /// <returns>A byte[] containing exactly those compressed bytes</returns>
        public static byte[] Compress( this byte[] _bytes, int _offset, int _length )
        {
            var memstr = new MemoryStream();
            var compressor = new DeflateStream( memstr, CompressionMode.Compress );
            compressor.Write( _bytes, _offset, _length );
            compressor.Close();
            return memstr.ToArray();
        }

        /// <summary>
        /// Compress a byte array into a new byte array
        /// </summary>
        /// <param name="_bytes">The array to compress</param>
        /// <returns>A byte[] containing exactly those compressed bytes</returns>
        public static byte[] Compress( this byte[] _bytes ) => Compress( _bytes, 0, _bytes.Length );

        /// <summary>
        /// Helper function that will return a compressed byte[] from a string.
        /// </summary>
        /// <param name="_string">The unicode string</param>
        /// <returns>A byte[] containing the string after its been compressed</returns>
        public static byte[] CompressString( this string _string )
        {
            var stringAsBytes = Encoding.Unicode.GetBytes( _string );
            return Compress( stringAsBytes, 0, stringAsBytes.Length );
        }

        /// <summary>
        /// Decompress an array of bytes into the "original" byte array
        /// </summary>
        /// <param name="_compressedBytes">
        /// The bytes that have been compressed with a <see cref="DeflateStream"/> (GZIP)
        /// </param>
        /// <returns>The uncompressed bytes in an array</returns>
        public static byte[] Decompress( this byte[] _compressedBytes )
        {
            var cmpByteStream = new MemoryStream( _compressedBytes );

            using (var decompressor = new DeflateStream( cmpByteStream, CompressionMode.Decompress ))
            {
                var defaultSize = _compressedBytes.Length;
                defaultSize = Math.Max( 4096, defaultSize );
                defaultSize = Math.Min( 65536, defaultSize );
                var tempArray = new byte[defaultSize];

                using (var output = new MemoryStream( defaultSize ))
                {
                    int count;
                    do
                    {
                        count = decompressor.Read( tempArray, 0, defaultSize );
                        output.Write( tempArray, 0, count );
                    }
                    while (count == defaultSize);

                    return output.ToArray();
                }
            }
        }

        /// <summary>
        /// Helper function to turn a string that was compressed using "CompressString" back
        /// into a string
        /// </summary>
        /// <param name="_compressedString">
        /// The bytes resulting from the "CompressSTring" operation
        /// </param>
        /// <returns>The original string that had been compressed</returns>
        public static string DecompressString( this byte[] _compressedString )
        {
            var uncompressedBytes = Decompress( _compressedString );
            return Encoding.Unicode.GetString( uncompressedBytes );
        }




        /// <summary>
        /// Transfer the bytes from one stream to another
        /// </summary>
        /// <param name="_source">The stream containing the bytes to transfer</param>
        /// <param name="_destination">The stream to receive the bytes</param>
        /// <returns>The actual number of bytes transferred</returns>
        public static int TransferStream( this Stream _source, Stream _destination ) => TransferStream( _source, _destination, 65536, -1 );

        /// <summary>
        /// Transfer the bytes from one stream to another
        /// </summary>
        /// <param name="_source">The stream containing the bytes to transfer</param>
        /// <param name="_destination">The stream to receive the bytes</param>
        /// <param name="_bufferSize">
        /// The size of the buffer used as temporary storage as the bytes are transferred
        /// </param>
        /// <returns>The actual number of bytes transferred</returns>
        public static int TransferStream( this Stream _source, Stream _destination, int _bufferSize ) => TransferStream( _source, _destination, _bufferSize, -1 );

        /// <summary>
        /// Transfer the bytes from one stream to another
        /// </summary>
        /// <param name="_source">The stream containing the bytes to transfer</param>
        /// <param name="_destination">The stream to receive the bytes</param>
        /// <param name="_bufferSize">
        /// The size of the buffer used as temporary storage as the bytes are transferred
        /// </param>
        /// <param name="_count">
        /// The number of bytes to transfer. If this is -1, all bytes in the source stream will
        /// be read.
        /// </param>
        /// <returns>The actual number of bytes transferred</returns>
        public static int TransferStream( this Stream _source, Stream _destination, int _bufferSize, int _count )
        {
            if (_count == 0)
                return 0;

            var buffer = new byte[_bufferSize];
            int bytesRead, totalBytesRead = 0, bytesLeft = _count;

            do
            {
                if (_count == -1)
                    bytesLeft = _bufferSize;

                var bytesToRead = Math.Min( bytesLeft, _bufferSize );
                bytesRead = _source.Read( buffer, 0, bytesToRead );
                if (bytesRead > 0)
                {
                    _destination.Write( buffer, 0, bytesRead );
                    bytesLeft -= bytesRead;
                    totalBytesRead += bytesRead;
                }
            }
            while (bytesRead > 0);

            return totalBytesRead;
        }

        /// <summary>
        /// Add some token to a filename between the extension and the name. For example, adding
        /// "BACKUP" to "T.TXT" yields "T.BACKUP.TXT"
        /// </summary>
        /// <param name="_filename">The filename</param>
        /// <param name="_whatToAdd">
        /// The token to add to the filename (before the extension)
        /// </param>
        /// <returns>The resulting filename</returns>
        public static string AddSomethingToFilename( string _filename, string _whatToAdd )
        {
            var s = Path.GetFileNameWithoutExtension( _filename );
            s += "." + _whatToAdd;
            s += Path.GetExtension( _filename );
            s = Path.Combine( Path.GetDirectoryName( _filename ), s );
            s = Path.Combine( Path.GetPathRoot( _filename ), s );
            return s;
        }


        /// <summary>
        /// Find a byte[] substring within a larger byte[]
        /// </summary>
        /// <param name="_source">The array of bytes to search</param>
        /// <param name="_toFind">The bytes to find within the source byte array</param>
        /// <returns>
        /// The index of the "_toFind" array in the source array, or -1 if nothing was found.
        /// </returns>
        public static int FindByteSubstring( this byte[] _source, byte[] _toFind ) => _source.FindByteSubstring( _toFind, 0 );

        /// <summary>
        /// Find a byte[] substring within a larger byte[]. Will never find null arrays or
        /// zero-length arrays (returns -1)
        /// </summary>
        /// <param name="_source">The array of bytes to search</param>
        /// <param name="_toFind">The bytes to find within the source byte array</param>
        /// <param name="_startIndex">
        /// The first index within the source array to start looking
        /// </param>
        /// <returns>
        /// The index of the "_toFind" array in the source array, or -1 if nothing was found.
        /// </returns>
        public static int FindByteSubstring( this byte[] _source, byte[] _toFind, int _startIndex )
        {
            if (_toFind == null || _toFind.Length == 0)
                return -1;
            if (_startIndex < 0)
                throw new ArgumentException( "_startIndex can't be negative" );
            if (_startIndex + _toFind.Length > _source.Length)
                return -1;

            var lastIdx = _source.Length - _toFind.Length;

            for (var srcIdx = _startIndex; srcIdx <= lastIdx; srcIdx++)
            {
                var found = true;

                for (var findIdx = 0; findIdx < _toFind.Length; findIdx++)
                {
                    var srcByte = _source[srcIdx + findIdx];
                    if (srcByte != _toFind[findIdx])
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                    return srcIdx;
            }

            return -1;
        }


        /// <summary>
        /// Convert all whitespace into spaces, and remove duplicates, and remove whitespace
        /// before and after the string
        /// </summary>
        /// <param name="_string">The string to "clean up"</param>
        /// <returns>A new string with the modifications requested</returns>
        /// <remarks>
        /// <code>
        /// var testStr = " \t \n This  \tIs  \tIt\n!   ";
        /// var expected = "This Is It !";
        /// var actual = testStr.RemoveDuplicateWhitespace();
        /// Assert.AreEqual( expected, actual );
        /// </code>
        /// </remarks>
        public static string RemoveDuplicateWhitespace( this string _string )
        {
            if (_string == null)
                throw new ArgumentNullException();

            var str = new StringBuilder( _string.Length );
            var whitespaceAdded = true; // start as true to prevent whitespace at beginning of string
            for (var i = 0; i < _string.Length; i++)
            {
                var ch = _string[i];

                if (ch < ' ' || char.IsWhiteSpace( ch )) // if char is WS...
                {
                    if (whitespaceAdded) // AND we've already added WS
                    {
                        continue; // then skip the char
                    }
                    else
                    {
                        whitespaceAdded = true; // add the whitespace, and set the flag
                        ch = ' '; // All WS converted to spaces
                    }
                }
                else // the char isn't WS, so add it and...
                {
                    whitespaceAdded = false; // set the flag to false, allowing one WS to be added
                }

                str.Append( ch );
            }

            while (whitespaceAdded && str.Length > 0) // If the last thing added was WS AND there was at least 1 char added...
            {
                str.Length--; // remove the trailing WS
                var ch = str[str.Length - 1];
                whitespaceAdded = ch < ' ' || char.IsWhiteSpace( ch );
            }

            return str.ToString();
        }

        /// <summary>
        /// Convert a byte array to a string that represents the byte array encoded as Base64
        /// </summary>
        /// <param name="_array">The bytes that are to be converted into a Base64 string</param>
        /// <returns>A string representing the Base64 version of the binary data.</returns>
        public static string ToBase64( this byte[] _array ) => Convert.ToBase64String( _array );

        /// <summary>
        /// Convert a string into the byte array equivalent
        /// </summary>
        /// <param name="_string">
        /// The string to convert into a byte array. The string must be a Base64 representation.
        /// </param>
        /// <returns>
        /// The byte array containing the original data before it was converted to Base64
        /// </returns>
        public static byte[] FromBase64( this string _string ) => Convert.FromBase64String( _string );


        /// <summary>
        /// Turn a double into a metric string. At this time, this routine only adds a prefix if
        /// the value is millis or smaller (down to yocto's for now)
        /// </summary>
        /// <param name="_value">
        /// The value to convert. Should be very small (less than 1 or so) to take advantage of
        /// this method.
        /// </param>
        /// <param name="_units">
        /// If present, this string shall be appended to the string.
        /// </param>
        /// <returns>A string containing the value converted to a metric string</returns>
        public static string MakeMetricString( this double _value, string _units = "" )
        {
            var prefixes = new[] { "", "m", "µ", "n", "p", "f", "a", "z", "y" };

            var x = _value;
            for (var i = 0; i < prefixes.Length; i++)
            {
                var str = x.ToString( "N9" );
                if (str[0] != '0')
                    return $"{str}{prefixes[i]}{_units}";

                x *= 1000;
            }
            return _value.ToString( "N0" );
        }


    }
}
