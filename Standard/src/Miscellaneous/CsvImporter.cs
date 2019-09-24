using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;


namespace Morpheus
{
    /// <summary>
    /// This class is designed to parse a string (stream, file) into its components. It does
    /// support the return of actual data objects with fields / properties populated from each
    /// line of the CSV file based on the column names in the CSV.
    /// 
    /// The application can "alias" column names to alternative field/property names.
    /// 
    /// The application can send in a (single, presumably large) string, a stream, or a
    /// filename.
    /// 
    /// The application can receive an enumeration of populated objects as well as a
    /// list-of-lists of strings representing the data.
    /// 
    /// The class uses <see cref="Convert.ChangeType(object, Type)"/> to coerce strings into the
    /// appropriate data type for the fields/properties of a data object.
    /// 
    /// This class does not (yet) support fixed-width columns- only data separated by specific
    /// separators can be parsed.
    /// 
    /// This class treats quoted data elements correctly by not considering any separator
    /// characters as special when enclosed in quotes. It does not handle escaped-quotes (yet)
    /// e.g.
    /// 
    /// "the quote \" character is escaped"
    /// 
    /// would parse as
    /// 
    /// the quote\
    /// 
    /// The class strips whitespace found on either side of a separator character
    /// 
    /// The class will ignore any blank lines found. A "blank line" contains either nothing or
    /// only whitespace. If the separator for data elements is itself a whitespace character,
    /// then multiple such separators in a line will prevent the "skipping" of the line.
    /// </summary>
    public class CsvImporter
    {
        /// <summary>
        /// This delegate can be used to handle exceptions in the input stream
        /// </summary>
        /// <param name="_stringToConvert">What was being converted</param>
        /// <param name="_resultingType">
        /// The Type that the string was supposed to be converted to
        /// </param>
        /// <param name="_memberName">The column name for the conversion</param>
        /// <returns></returns>
        public delegate object DFormatExceptionHandler( string _stringToConvert, Type _resultingType, string _memberName );

        /// <summary>
        /// Those characters which may separate data elements from each other. The application
        /// may set this value.
        /// </summary>
        public string ColumnSeparator = ",";

        /// <summary>
        /// The character to replace any spaces in a column name with. For instance, you may
        /// specify an underline to replace spaces, making "First Name" result in a
        /// field/property called "First_Name". Specifying an empty string (default) simply
        /// removes spaces from column names.
        /// </summary>
        public string ColumnNameSpaceReplacement = "";

        /// <summary>
        /// Flag telling the importer to remove literal quotes from data. When FALSE, any quoted
        /// data element will be returned
        /// </summary>
        public bool RemoveQuotes = true;

        /// <summary>
        /// Called (with a StringBuilder) when the header is read from the CSV
        /// </summary>
        public event Action<StringBuilder> OnHeaderRead;

        /// <summary>
        /// Called when a line of text has been read from the CSV. Returns the actual line of
        /// text, unparsed.
        /// </summary>
        public event Action<StringBuilder> OnLineRead;

        /// <summary>
        /// Called when a column name will not be used because there is no corresponding field
        /// in the object.
        /// 
        /// This has no negative effect if the CSV is not to be coerced into objects.
        /// </summary>
        public event Action<string> OnUnusableColumn;

        /// <summary>
        /// A function that will handle a conversion error by returning what the conversion of
        /// the string should be, given the expected type and member name. This handler is
        /// called only after an exception is thrown, so it should only be used sparingly and
        /// not as a "general purpose data conversion" mechanism.
        /// </summary>
        public DFormatExceptionHandler OnFormatError;

        /// <summary>
        /// A collection of column names, as they appear in the CSV
        /// </summary>
        private List<string> m_columnNames;

        /// <summary>
        /// All of the data found in the CSV, parsed into individual data element strings
        /// </summary>
        private List<List<string>> m_data;


        /// <summary>
        /// The default constructor
        /// </summary>
        public CsvImporter()
        {
        }


        /// <summary>
        /// Parse a string (in the form of a StringBuilder) into its constituent parts
        /// </summary>
        /// <param name="_line">The string to parse</param>
        /// <param name="_separator">The character(s) to use as separators</param>
        /// <param name="_removeQuotes">If set, quoted strings have the quotes removed</param>
        /// <returns>An enumeration of strings defined by _line and _separator</returns>
        public static IEnumerable<string> ParseString( StringBuilder _line, string _separator, bool _removeQuotes )
        {
            if (string.IsNullOrEmpty( _separator ))
                throw new ArgumentNullException( "The separator string must contain at least one separator character" );
            if (_line == null)
                throw new ArgumentNullException( "Cannot specify a NULL line" );

            var current = new StringBuilder();
            var inQuotes = false;

            // Loop through the whole string
            for (var i = 0; i < _line.Length; i++)
            {
                var ch = _line[i];

                // If we are currently inside of a quoted string, do this...
                if (inQuotes)
                {
                    if (ch == '\"') // we've reached the end of the quoted string
                    {
                        inQuotes = false; // reset the state flag
                        if (!_removeQuotes) // and append the trailing quote if we're not supposed to remove it
                            current.Append( ch );
                    }
                    else // we are simply inside of a quoted string
                    {
                        current.Append( ch );
                    }
                }
                else // not in quotes
                {
                    if (_separator.Contains( ch )) // Check to see if the current char is a separator char
                    {
                        RemoveTrailingWhitespace( current );
                        yield return current.ToString();

                        current.Length = 0;
                    }
                    else if (char.IsWhiteSpace( ch ))
                    {
                        if (current.Length != 0) // only append if its not whitespace, effectively trimming leading whitespace
                            current.Append( ch );
                    }
                    else if (ch == '\"' && current.Length == 0) // a Quote... If we're at the beginning of a token,
                    {                                           // process as quote, otherwise process as just another char
                        inQuotes = true;
                        if (!_removeQuotes) // only add it if we're not removing quotes
                            current.Append( ch );
                    }
                    else // treat as an include-able character
                    {
                        current.Append( ch );
                    }
                }
            }
            if (_line.Length > 0) // only perform end-of-loop processing if there was something in the line to start with
            {
                RemoveTrailingWhitespace( current );
                yield return current.ToString();
            }
        }

        /// <summary>
        /// Simple routine to remove whitespace from the end of a StringBuilder
        /// </summary>
        /// <param name="_string">The string to remove whitespace from</param>
        public static void RemoveTrailingWhitespace( StringBuilder _string )
        {
            if (_string == null)
                throw new ArgumentNullException( "Cannot remove trailing whitespace from NULL" );

            while (_string.Length > 0 && char.IsWhiteSpace( _string[_string.Length - 1] ))
                _string.Length--;
        }


        /// <summary>
        /// Translate the column names found in a CSV into new names based on a table of aliases
        /// </summary>
        /// <param name="_aliases">The aliases for the column names</param>
        /// <returns>
        /// A new list of names based on the column names overlaid with the aliases
        /// </returns>
        private List<string> TranslateColumnNames( Dictionary<string, string> _aliases )
        {
            var colNames = new List<string>( m_columnNames );
            if (_aliases != null)
            {
                for (var i = 0; i < colNames.Count; i++)
                {
                    var cn = colNames[i];
                    if (_aliases.TryGetValue( cn, out var newName ))
                        colNames[i] = newName;
                }
            }

            return colNames;
        }

        /// <summary>
        /// Map a list of names to fields or properties of a <see cref="System.Type"/> ,
        /// returning a list of <see cref="System.Reflection.MemberInfo"/> objects. A null entry
        /// in the list means that there was no corresponding field/property in the Type based
        /// on the column name
        /// </summary>
        /// <param name="_columnNames">
        /// The column names used to look up fields/properties in the Type
        /// </param>
        /// <param name="_type">The Type that will receive CSV values</param>
        /// <returns>
        /// A list of MemberInfo objects corresponding to the fields/properties in the Type
        /// </returns>
        private List<MemberInfo> GetUsableMappings( IList<string> _columnNames, Type _type )
        {
            var memberInfos = new List<MemberInfo>();

            for (var i = 0; i < _columnNames.Count; i++)
            {
                var colName = _columnNames[i];

                var fi = _type.GetField( colName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
                if (fi != null)
                {
                    memberInfos.Add( fi );
                    continue;
                }

                var pi = _type.GetProperty( colName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
                if (pi != null)
                {
                    memberInfos.Add( pi );
                    continue;
                }

                OnUnusableColumn?.Invoke( colName );

                memberInfos.Add( null ); // no corresponding member for the column name
            }

            return memberInfos;
        }


        /// <summary>
        /// Import a CSV file based on a <see cref="StreamReader"/>
        /// </summary>
        /// <param name="_stream">The stream to import</param>
        public void ImportStream( TextReader _stream )
        {
            m_columnNames = null;
            m_data = null;
            if (ColumnNameSpaceReplacement == null)
                ColumnNameSpaceReplacement = "";

            var header = _stream.ReadLine(); // Header must be the first line in the stream
            if (header != null)
            {
                var str = new StringBuilder( header );
                OnHeaderRead?.Invoke( str );
                ProcessHeader( str );

                string line;
                while ((line = _stream.ReadLine()) != null)
                {
                    str.Length = 0;
                    str.Append( line );

                    OnLineRead?.Invoke( str );

                    ProcessLine( str );
                }
            }
        }

        /// <summary>
        /// Parse and process the "header" row of a CSV file
        /// </summary>
        /// <param name="_header">
        /// A <see cref="StringBuilder"/> containing the header row
        /// </param>
        private void ProcessHeader( StringBuilder _header )
        {
            var fields = ParseString( _header, ColumnSeparator, RemoveQuotes );
            var colNames = fields.Select( _fn => _fn.Replace( " ", ColumnNameSpaceReplacement ) );
            m_columnNames = colNames.ToList();
            m_data = new List<List<string>>();
        }

        /// <summary>
        /// Parse and process a non-header row of a CSV file (a data row)
        /// </summary>
        /// <param name="_line">A <see cref="StringBuilder"/> containing the data row</param>
        private void ProcessLine( StringBuilder _line )
        {
            var fields = ParseString( _line, ColumnSeparator, RemoveQuotes );
            var entry = fields.ToList();
            if (entry.Count > 1 || (entry.Count == 1 && !string.IsNullOrEmpty( entry[0] )))
                m_data.Add( entry );
        }



        /// <summary>
        /// Import a CSV file from the operating system
        /// </summary>
        /// <param name="_filename">The filename of the CSV file</param>
        public void ImportFile( string _filename )
        {
            using (var infile = File.OpenText( _filename ))
            {
                ImportStream( infile );
            }
        }

        /// <summary>
        /// Treat a string as a CSV file, and import it
        /// </summary>
        /// <param name="_csvFileContents">
        /// Presumably, the contents of a CSV file pre-read into a string
        /// </param>
        public void ImportString( string _csvFileContents )
        {
            using (var instr = new StringReader( _csvFileContents ))
            {
                ImportStream( instr );
            }
        }


        /// <summary>
        /// Create a collection of data objects of a specified Type based on the data imported
        /// into this CSV importer object
        /// </summary>
        /// <remarks>
        /// Calling this method may invoke the "OnUnusableColumn" event, if it is specified,
        /// once for each column name that does not have a corresponding field or property
        /// defined in the specified class "T"
        /// </remarks>
        /// <typeparam name="T">The Type of each data element to return</typeparam>
        /// <returns>
        /// An enumeration of the contents of the CSV file converted into objects of Type "T"
        /// </returns>
        public IEnumerable<T> GetData<T>() => GetData<T>( null );

        /// <summary>
        /// Create a collection of data objects of a specified Type based on the data imported
        /// into this CSV importer object
        /// </summary>
        /// <remarks>
        /// Calling this method may invoke the "OnUnusableColumn" event, if it is specified,
        /// once for each column name that does not have a corresponding field or property
        /// defined in the specified class "T"
        /// </remarks>
        /// <typeparam name="T">The Type of each data element to return</typeparam>
        /// <param name="_aliases">
        /// A dictionary (string->string) that tells the converter when to change the name of a
        /// CSV column because the corresponding member in "T" has a different name
        /// </param>
        /// <returns>
        /// An enumeration of the contents of the CSV file converted into objects of Type "T"
        /// </returns>
        public IEnumerable<T> GetData<T>( Dictionary<string, string> _aliases )
        {
            var colNames = TranslateColumnNames( _aliases );
            var usableMappings = GetUsableMappings( colNames, typeof( T ) );
            object val;

            foreach (var line in m_data)
            {
                var obj = Activator.CreateInstance<T>();

                for (var i = 0; i < usableMappings.Count; i++)
                {
                    var mi = usableMappings[i];
                    if (mi != null)
                    {
                        var fi = mi as FieldInfo;
                        if (fi != null)
                        {
                            if (fi.FieldType == typeof( string ))
                                val = line[i];
                            else
                                val = HandleConversion( line[i], fi.FieldType, fi.Name );

                            fi.SetValue( obj, val );
                        }
                        else
                        {
                            var pi = mi as PropertyInfo;
                            if (pi != null)
                            {
                                if (pi.PropertyType == typeof( string ))
                                    val = line[i];
                                else
                                    val = HandleConversion( line[i], pi.PropertyType, pi.Name );

                                pi.SetValue( obj, val, null );
                            }
                        }
                    }
                }

                yield return obj;
            }
        }

        /// <summary>
        /// Handle the conversion of a string to the expected Type within the destination object
        /// by calling the built-in <see cref="Convert.ChangeType(object, Type)"/> method. If
        /// this method throws a format exception, then call the handler specified (if it
        /// exists) or re-throw the exeption
        /// </summary>
        /// <param name="_stringValue">The value from the CSV file to convert</param>
        /// <param name="_convertTo">The System.Type of the value to convert TO</param>
        /// <param name="_fieldName">
        /// The name of the field within the CSV file that we're converting
        /// </param>
        /// <returns>
        /// An object of type _convertTo that is representative of the string value
        /// </returns>
        private object HandleConversion( string _stringValue, Type _convertTo, string _fieldName )
        {
            try
            {
                return Convert.ChangeType( _stringValue, _convertTo );
            }
            catch (FormatException)
            {
                if (OnFormatError == null)
                    throw;
                return OnFormatError( _stringValue, _convertTo, _fieldName );
            }
        }

        /// <summary>
        /// Return the list of column names
        /// </summary>
        /// <returns>A list of column names</returns>
        public List<string> GetColumnNames() => m_columnNames;

        /// <summary>
        /// Return the raw data rows found in the file, as an array of arrays of strings
        /// </summary>
        /// <returns></returns>
        public List<List<string>> GetData() => m_data;
    }
}
