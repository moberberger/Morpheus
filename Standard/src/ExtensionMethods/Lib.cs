using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Linq.Expressions;
using Morpheus.ProbabilityGeneratorNS;

/// <summary>
/// A collection of various static functions with no strong correlation.
/// 
/// Should be viewed as "helper" or "convenience" functions, not OO design.
/// </summary>
public static class Lib
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
    /// 
    /// </summary>
    /// <param name="output1"></param>
    /// <param name="output2"></param>
    public static void Swap<T>( ref T val1, ref T val2 )
    {
        T tmp = val1;
        val1 = val2;
        val2 = tmp;
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
            arr[i] = (T)Convert.ChangeType( vals[i], elemType );
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
    ///     token = Morpheus.ParseNextSegment( sentence, ' ', ref idx, ref len );
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


    /// <summary>
    /// TODO: Protect this static!
    /// </summary>
    private readonly static Dictionary<string, Type> sm_typeCrossReference = new Dictionary<string, Type>();

    /// <summary>
    /// Helper method to try to resolve a type name.
    /// </summary>
    /// <remarks>
    /// Will try to overcome the inherent limitation of Type.GetType that only looks in
    /// mscorlib and in the current assembly. This will be addressed by iterating through
    /// all loaded assemblies if Type.GetType does not return anything.
    /// </remarks>
    /// <param name="_name">The name to resolve</param>
    /// <returns>
    /// The type associated with the name if it exists, or NULL if no type can be found.
    /// </returns>
    public static Type BetterGetType( string _name )
    {
        var t = Type.GetType( _name );
        if (t != null)
            return t;

        foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
        {
            t = a.GetType( _name );
            if (t != null)
                return t;
        }

        var idx = _name.IndexOf( ", Version=" );
        if (idx > 0)
            return BetterGetType( _name.Substring( 0, idx ) );

        return null;
    }

    /// <summary>
    /// Helper method to try to resolve a type name using a combination of a "cache" of
    /// already-found names and the search through all assemblies for a matching type name
    /// </summary>
    /// <remarks>
    /// Because of the order in which assemblies get loaded, the "foreach" loop in
    /// <see cref="BetterGetType(string)"/> usually loops through all the system (microsoft)
    /// assemblies before it gets to your assemblies. As such, this cache was implemented to
    /// "remember" if you've passed in any given type name and to look that name up
    /// immediately before trying to loop through all of the loaded assemblies. This is not
    /// the default behavior because its possible, when doing a lot of "custom" assembly
    /// loading, the application may rely on a new search every time (this argument may be
    /// fallacious).
    /// </remarks>
    /// <param name="_name">
    /// The "full name" of the class, WITHOUT ASSEMBLY QUALIFICATION
    /// </param>
    /// <param name="_useCacheOfTypes">
    /// When TRUE, use the cache before looping through all assemblies to find the type
    /// </param>
    /// <returns>
    /// The type associated with the name if it exists, or NULL if no type can be found.
    /// </returns>
    public static Type BetterGetType( string _name, bool _useCacheOfTypes )
    {
        // Below, check the parameter first, and if its false, then force the looping
        // anyways.
        if (!_useCacheOfTypes || !sm_typeCrossReference.TryGetValue( _name, out var retval ))
        {
            retval = BetterGetType( _name );
            sm_typeCrossReference[_name] = retval;
            // regardless of the flag, calling this method WILL add the info to the cache
        }

        return retval;
    }


    /// <summary>
    /// Get all loaded types that have a specified attribute and that attribute matches a
    /// filter condition
    /// </summary>
    /// <typeparam name="T">The Type of the attribute that is required</typeparam>
    /// <param name="_filter">
    /// The filter to use on the attributes before a Type is returned. If null, then no
    /// filter is applied and all found attributes are returned.
    /// </param>
    /// <returns>
    /// All Types in all loaded assemblies that have the specified attribute that conform to
    /// the spefified filter
    /// </returns>
    public static IEnumerable<Type> GetTypesWithAttribute<T>( Func<T, bool> _filter = null )
    {
        var retval = new List<Type>();

        foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                foreach (var t in a.GetTypes())
                {
                    foreach (var attr in t.GetCustomAttributes( typeof( T ), false ))
                    {
                        if (_filter == null || _filter( (T)attr ))
                        {
                            retval.Add( t );
                            break;
                        }

                    }
                }
            }
            catch { }
        }

        return retval;
    }


    /// <summary>
    /// Search all-or-current assembly for all public static functions that are decorated
    /// with a given attribute.
    /// </summary>
    /// <typeparam name="T">The Type of the Attribute to search for</typeparam>
    /// <param name="_allLoadedAssemblies">
    /// TRUE- Look in all loaded assemblies, FALSE- Look only in the caller's assembly
    /// </param>
    /// <returns>
    /// A Dictionary with <see cref="MethodInfo"/> objects as keys and Attributes (of type
    /// T) as values.
    /// </returns>
    public static Dictionary<MethodInfo, T> GetStaticFunctionsWithAttribute<T>( bool _allLoadedAssemblies )
        where T : Attribute
    {
        var assemblies = new List<Assembly>();
        if (_allLoadedAssemblies)
            assemblies.AddRange( AppDomain.CurrentDomain.GetAssemblies() );
        else
            assemblies.Add( Assembly.GetCallingAssembly() );

        var retval = new Dictionary<MethodInfo, T>();

        foreach (var a in assemblies)
        {
            try
            {
                foreach (var t in a.GetTypes())
                {
                    foreach (var m in t.GetMethods( BindingFlags.Static | BindingFlags.Public ))
                    {
                        var ca = m.GetCustomAttributes( typeof( T ), false );
                        if (ca.Length > 0)
                            retval[m] = ca[0] as T;
                    }

                }
            }
            catch { }
        }

        return retval;
    }


    /// <summary>
    /// Find out if a particular member has a specific attribute. Don't use this method if
    /// you need the actual attribute objects- use the GetCustomAttributes method of the
    /// member, as usual.
    /// </summary>
    /// <param name="_member">The member to check</param>
    /// <param name="_attribute">The Attribute that you're looking for</param>
    /// <returns>TRUE if the attribute exists on the member, FALSE if not.</returns>
    public static bool HasAttribute( this MemberInfo _member, Type _attribute )
    {
        var attrs = _member.GetCustomAttributes( _attribute, false );
        return attrs.Length > 0;
    }

    /// <summary>
    /// Find out if a particular member has a specific attribute. Don't use this method if
    /// you need the actual attribute objects- use the GetCustomAttributes method of the
    /// member, as usual.
    /// </summary>
    /// <typeparam name="TAttrType">The Attribute that you're looking for</typeparam>
    /// <param name="_member">The member to check</param>
    /// <returns>TRUE if the attribute exists on the member, FALSE if not</returns>
    public static bool HasAttribute<TAttrType>( this MemberInfo _member )
        where TAttrType : Attribute
    {
        var attrs = _member.GetCustomAttributes( typeof( TAttrType ), false );
        return attrs.Length > 0;
    }

    /// <summary>
    /// Return an attribute associated with a member, or null if no such attribute exists on
    /// the member. Returns the "first" attribute if multiple attributes exist.
    /// </summary>
    /// <typeparam name="TAttrType">The Type of the attribute to find</typeparam>
    /// <param name="_member">The member to look for attributes on</param>
    /// <returns>
    /// NULL if the attribute isn't associated with the member, or the first attribute found
    /// on the member if one is.
    /// </returns>
    public static TAttrType GetSingleAttribute<TAttrType>( this MemberInfo _member )
        where TAttrType : Attribute
    {
        var attrs = _member.GetCustomAttributes( typeof( TAttrType ), false );
        if (attrs.Length == 0)
            return null;
        return attrs[0] as TAttrType;
    }


    /// <summary>
    /// Create a new instance of an object using the constructor parameters specified.
    /// </summary>
    /// <param name="_type">The Type of the object to create</param>
    /// <param name="_params">
    /// The parameters to pass to the constructor. NULL implies the default constructor
    /// </param>
    /// <returns>
    /// An object of the type specified constructed using the parameters specified
    /// </returns>
    public static object CreateWithConstructor( this Type _type, params object[] _params )
    {
        var paramTypes = _params.Select( _o => _o.GetType() ).ToArray();
        var constructor = _type.GetConstructor( paramTypes );

        return constructor.Invoke( _params );
    }



















    /// <summary>
    /// A Helper function that will not only allocate an array of type T, but it will also
    /// construct objects for each element in the array using whatever parameters for the
    /// constructor were specified by the caller. The same rules for constructor parameters
    /// apply here as do for <see cref="CreateWithConstructor"/>
    /// </summary>
    /// <typeparam name="T">
    /// The Type of objects in the array to create- All objects are of this type, so this
    /// can't be an interface or abstract type
    /// </typeparam>
    /// <param name="_size">
    /// The number of elements in the array- All of these elements will be constructed
    /// </param>
    /// <param name="_constructorParams">
    /// A list of parameters which are to be passed to the constructors for all objects in
    /// the array
    /// </param>
    /// <returns>
    /// A new array of type T containing objects of type T constructed using the specified
    /// constructor parameters
    /// </returns>
    public static T[] CreatePopulatedArray<T>( int _size, params object[] _constructorParams )
    {
        var arr = new T[_size];
        var typ = typeof( T );

        for (var i = 0; i < _size; i++)
            arr[i] = (T)CreateWithConstructor( typ, _constructorParams );

        return arr;
    }

    /// <summary>
    /// A Helper function that will not only allocate an array of type T, but it will also
    /// construct objects for each element in the array using whatever constructor function
    /// provided by the caller
    /// </summary>
    /// <typeparam name="T">
    /// The Type of objects in the array to create- All objects are of this type, so this
    /// can't be an interface or abstract type
    /// </typeparam>
    /// <param name="_size">
    /// The number of elements in the array- All of these elements will be constructed
    /// </param>
    /// <param name="_creator">The function used to create new objects of type T</param>
    /// <returns>
    /// A new array of type T containing objects of type T constructed using the specified
    /// creation function
    /// </returns>
    public static T[] CreatePopulatedArray<T>( int _size, Func<T> _creator )
    {
        var arr = new T[_size];

        for (var i = 0; i < _size; i++)
            arr[i] = _creator();

        return arr;
    }

    /// <summary>
    /// A Helper function that will not only allocate an array of type T, but it will also
    /// construct objects for each element in the array using whatever constructor function
    /// provided by the caller
    /// </summary>
    /// <typeparam name="T">
    /// The Type of objects in the array to create- All objects are of this type, so this
    /// can't be an interface or abstract type
    /// </typeparam>
    /// <param name="_size">
    /// The number of elements in the array- All of these elements will be constructed
    /// </param>
    /// <param name="_creator">
    /// The function used to create new objects of type T. The index of the element to be
    /// created is passed to this function.
    /// </param>
    /// <returns>
    /// A new array of type T containing objects of type T constructed using the specified
    /// creation function
    /// </returns>
    public static T[] CreatePopulatedArray<T>( int _size, Func<int, T> _creator )
    {
        var arr = new T[_size];

        for (var i = 0; i < _size; i++)
            arr[i] = _creator( i );

        return arr;
    }





    /// <summary>
    /// Determines if a Type inherits a specific Interface. This handles generic interfaces
    /// correctly, in that passing in typeof( <see cref="IEnumerable{T}"/> ) will check the
    /// Type against any specific versions of <see cref="IEnumerable{T}"/>
    /// </summary>
    /// <param name="_type">The Type to check for the interface.</param>
    /// <param name="_interfaceType">The Type of the Interface to look for</param>
    /// <returns>TRUE if the Type has the specified interface in its heirarchy.</returns>
    public static bool ImplementsInterface( this Type _type, Type _interfaceType )
    {
        if (!_interfaceType.IsInterface)
            throw new ArgumentException( $"Type '{_interfaceType}' is not an Interface." );

        if (_type.IsInterface)
        {
            if (IsInterfaceMatch( _type, _interfaceType ))
                return true;
        }

        var interfaces = _type.GetInterfaces();
        for (var i = 0; i < interfaces.Length; i++) // much faster than foreach on Arrays
        {
            var _interface = interfaces[i];

            if (IsInterfaceMatch( _interface, _interfaceType ))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Determine if the two types represent matching interfaces. Interfaces match always
    /// when they're the same <see cref="Type"/> . Also, interfaces also match if one is an
    /// un-bound generic and the other is a specific generic.
    /// </summary>
    /// <param name="_interfaceToTest"></param>
    /// <param name="_interfaceType"></param>
    /// <returns></returns>
    public static bool IsInterfaceMatch( Type _interfaceToTest, Type _interfaceType )
    {
        if (_interfaceToTest == _interfaceType)
            return true;

        if (_interfaceType.IsGenericTypeDefinition)
        {
            if (_interfaceToTest.IsGenericType && _interfaceToTest.GetGenericTypeDefinition() == _interfaceType)
                return true;
        }
        return false;
    }


    /// <summary>
    /// This method will retrieve all fields in the Type's hierarchy, including private
    /// fields found in a superclass that would otherwise be hidden when using the
    /// GetFields() method (even with BindingFlags.FlattenHierarchy). This will return
    /// public and non-public instance fields, but not any static fields.
    /// </summary>
    /// <param name="_type">The Type to return all fields for</param>
    /// <returns>
    /// An enumeration of all fields of a Type, including private fields on superclasses
    /// </returns>
    public static IEnumerable<FieldInfo> GetAllFields( this Type _type )
    {
        var query = from typ in _type.GetTypesInInheritanceChain( true, false )
                    from fi in typ.GetFields( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly )
                    select fi;
        return query;
    }

    /// <summary>
    /// Return an enumeration of the provided Type plus all types considered "BaseTypes"
    /// (SuperClasses) of that Type. The returned values will be most-derived to
    /// least-derived, with "this type" being the first returned value (if requested) and
    /// typeof(object) being the last returned value (if requested)
    /// </summary>
    /// <param name="_type">
    /// The Type that this function will return the inheritance chain for
    /// </param>
    /// <param name="_includeThisType">
    /// If TRUE, then _type will be returned first. If FALSE, then _type.BaseType will be
    /// return first
    /// </param>
    /// <param name="_includeSystemObject">
    /// If TRUE, then typeof(System.Object) will be returned as the last element. If FALSE,
    /// then the Type derived directly from System.Object is returned last.
    /// </param>
    /// <returns>
    /// An enumeration of Types in a Type's inheritance chain, in order of the most-derived
    /// to the least-derived Types.
    /// </returns>
    public static IEnumerable<Type> GetTypesInInheritanceChain( this Type _type, bool _includeThisType = true, bool _includeSystemObject = false )
    {
        if (_type != typeof( object ))
        {
            if (_includeThisType)
                yield return _type;

            for (var typ = _type.BaseType; typ != typeof( object ); typ = typ.BaseType)
                yield return typ;
        }

        if (_includeSystemObject)
            yield return typeof( object );
    }







    /// <summary>
    /// Gets MemberInfo even if the referenced member is a primitive value nested inside a
    /// UnaryExpression which is a Convert operation.
    /// </summary>
    /// <param name="_body">
    /// The Expression Body which is evaluated as a Member expression
    /// </param>
    /// <returns>
    /// A FieldInfo or PropertyInfo representing the member described in the expression.
    /// </returns>
    public static MemberInfo GetMemberInfo( this Expression _body )
    {
        var memberExpr = _body as MemberExpression;

        if (_body is UnaryExpression)
        {
            var unaryBody = _body as UnaryExpression;
            if (unaryBody.NodeType != ExpressionType.Convert)
                throw new ArgumentException( "A Non-Convert Unary Expression was found." );

            memberExpr = unaryBody.Operand as MemberExpression;
            if (memberExpr == null)
                throw new ArgumentException( "The target of the Convert operation was not a MemberExpression." );
        }
        else if (memberExpr == null)
        {
            throw new ArgumentException( "The Expression must identify a single member." );
        }

        var member = memberExpr.Member;
        if (!(member is FieldInfo || member is PropertyInfo))
            throw new ArgumentException( "The member specified was not a Field or Property: " + member.GetType() );

        return memberExpr.Member;
    }

    /// <summary>
    /// Wrapper around GetMemberInfo that assures a Field is returned
    /// </summary>
    /// <param name="_body">The expression identifying a field</param>
    /// <returns>A FieldInfo object for the identified field</returns>
    public static FieldInfo GetFieldInfo( this Expression _body )
    {
        var member = _body.GetMemberInfo();
        if (!(member is FieldInfo))
            throw new ArgumentException( "The specified member is not a Field: " + member.GetType() );

        return member as FieldInfo;
    }

    /// <summary>
    /// Wrapper around GetMemberInfo that assures a Property is returned
    /// </summary>
    /// <param name="_body">The expression identifying a property</param>
    /// <returns>A PropertyInfo object for the identified property</returns>
    public static PropertyInfo GetPropertyInfo( this Expression _body )
    {
        var member = _body.GetMemberInfo();
        if (!(member is PropertyInfo))
            throw new ArgumentException( "The specified member is not a Property: " + member.GetType() );

        return member as PropertyInfo;
    }

    /// <summary>
    /// Get a MemberInfo object for an expression. Allows the expression to be constructed
    /// as a parameter to this method.
    /// </summary>
    /// <typeparam name="T">
    /// The Type of the object declaring the interesting field or property
    /// </typeparam>
    /// <param name="_expr">An expression identifying a member on type T</param>
    /// <returns>A MemberInfo object for the identified member</returns>
    public static MemberInfo GetMemberInfo<T>( Expression<Func<T, object>> _expr ) => _expr.Body.GetMemberInfo();

    /// <summary>
    /// Get a FieldInfo object for an expression. Allows the expression to be constructed as
    /// a parameter to this method.
    /// </summary>
    /// <typeparam name="T">
    /// The Type of the object declaring the interesting field
    /// </typeparam>
    /// <param name="_expr">An expression identifying a field on type T</param>
    /// <returns>A FieldInfo object for the identified field</returns>
    public static FieldInfo GetFieldInfo<T>( Expression<Func<T, object>> _expr ) => _expr.Body.GetFieldInfo();

    /// <summary>
    /// Get a PropertyInfo object for an expression. Allows the expression to be constructed
    /// as a parameter to this method.
    /// </summary>
    /// <typeparam name="T">
    /// The Type of the object declaring the interesting property
    /// </typeparam>
    /// <param name="_expr">An expression identifying a property on type T</param>
    /// <returns>A PropertyInfo object for the identified property</returns>
    public static PropertyInfo GetPropertyInfo<T>( Expression<Func<T, object>> _expr ) => _expr.Body.GetPropertyInfo();



    /// <summary>
    /// Given what's assumed to be an individual getter or setter method for a property,
    /// figure out what the actual <see cref="PropertyInfo"/> object is for the property
    /// that the accessor belongs to
    /// </summary>
    /// <param name="_method">The actual get or set method to analyse</param>
    /// <returns>
    /// NULL if the parameter is not a get or set method, or the <see cref="PropertyInfo"/>
    /// object if it is.
    /// </returns>
    public static PropertyInfo GetPropertyInfo( this MethodBase _method )
    {
        var method = _method as MethodInfo;
        if (method == null) return null;

        var takesArg = method.GetParameters().Length == 1;
        var hasReturn = method.ReturnType != typeof( void );
        if (takesArg == hasReturn) return null;
        if (takesArg) // takesArg -> SET operation
        {
            return method.DeclaringType.GetProperties().FirstOrDefault( _prop => _prop.GetSetMethod() == method );
        }
        else // hasReturn -> GET operation
        {
            return method.DeclaringType.GetProperties().FirstOrDefault( _prop => _prop.GetGetMethod() == method );
        }
    }


    /// <summary>
    /// Set the value of a property or field on an object using the name of the member
    /// </summary>
    /// <param name="_object">The object containing the member to set</param>
    /// <param name="_memberName">The name of the member to set</param>
    /// <param name="_value">The value to assign to the member</param>
    /// <param name="_includeFields">If TRUE, then fields will be searched</param>
    /// <param name="_includeProperties">If TRUE, then properties will be searched</param>
    /// <param name="_includePrivate">
    /// If TRUE, then access modifiers (public/private/etc) will be ignored.
    /// </param>
    /// <returns>
    /// TRUE if the value was set, FALSE if there was no member with the specified name.
    /// </returns>
    public static bool SetMemberValue( this object _object, string _memberName, object _value, bool _includeFields = true, bool _includeProperties = true, bool _includePrivate = false )
    {
        if (_object == null)
            throw new ArgumentNullException( "_object" );
        if (string.IsNullOrEmpty( _memberName ))
            throw new ArgumentNullException( "_memberName" );

        var typ = _object.GetType();
        var bindFlag = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
        if (_includePrivate)
            bindFlag |= BindingFlags.NonPublic;

        if (_includeFields)
        {
            var fi = typ.GetField( _memberName, bindFlag );
            if (fi != null)
            {
                var cnvVal = Convert.ChangeType( _value, fi.FieldType );
                fi.SetValue( _object, cnvVal );
                return true;
            }
        }

        if (_includeProperties)
        {
            var pi = typ.GetProperty( _memberName, bindFlag );
            if (pi != null)
            {
                var cnvVal = Convert.ChangeType( _value, pi.PropertyType );
                pi.SetValue( _object, cnvVal, null );
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// A meta-class (sentinel) for describing a specific type of situation
    /// </summary>
    public class MemberNotFound { }

    /// <summary>
    /// The sentinel value used to denote that a member is not found
    /// </summary>
    public static MemberNotFound MEMBER_NOT_FOUND { get; } = new MemberNotFound();

    /// <summary>
    /// Get the value of a property or field on an object using the name of the member
    /// </summary>
    /// <param name="_object">The object containing the member to get</param>
    /// <param name="_memberName">The name of the member to get</param>
    /// <param name="_includeFields">If TRUE, then fields will be searched</param>
    /// <param name="_includeProperties">If TRUE, then properties will be searched</param>
    /// <param name="_includePrivate">
    /// If TRUE, then access modifiers (public/private/etc) will be ignored.
    /// </param>
    /// <returns>
    /// The value of the member specified, or <see cref="MEMBER_NOT_FOUND"/> if there was no
    /// member on the object
    /// </returns>
    public static object GetMemberValue( this object _object, string _memberName, bool _includeFields = true, bool _includeProperties = true, bool _includePrivate = false )
    {
        if (_object == null)
            throw new ArgumentNullException( "_object" );
        if (string.IsNullOrEmpty( _memberName ))
            throw new ArgumentNullException( "_memberName" );

        var typ = _object.GetType();
        var bindFlag = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
        if (_includePrivate)
            bindFlag |= BindingFlags.NonPublic;

        if (_includeFields)
        {
            var fi = typ.GetField( _memberName, bindFlag );
            if (fi != null)
            {
                return fi.GetValue( _object );
            }
        }

        if (_includeProperties)
        {
            var pi = typ.GetProperty( _memberName, bindFlag );
            if (pi != null)
            {
                return pi.GetValue( _object, null );
            }
        }

        return MEMBER_NOT_FOUND;
    }


    public static IEnumerable<int> ForLoop( int count )
    {
        for (int i = 0; i < count; i++)
            yield return i;
    }

    public static IEnumerable<int> ForLoop( int start, int end, int step = 1 )
    {
        for (int i = start; i < end; i += step)
            yield return i;
    }

    public static IEnumerable<T> Repeat<T>( int count, Func<T> action )
    {
        while (count-- > 0)
            yield return action();
    }

    public static IEnumerable<T> Repeat<T>( int count, Func<int, T> action )
    {
        for (int i = 0; i < count; i++)
            yield return action( i );
    }
}
