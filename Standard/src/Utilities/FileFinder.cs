using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Morpheus
{
    /// <summary>
    /// This class helps an application find a file that is either in the application's current
    /// running directory OR its somewhere "up" the directory structure of the Current
    /// Directory.
    /// </summary>
    /// <remarks>
    /// Usually, when a program is "run" from within Visual Studio, it is run from the
    /// "[appName]/bin/debug" subdirectory. Many times you would like to place a data file in
    /// the "[appName]" directory and not have to worry about messing around trying to find it.
    /// This class helps make finding that file a painless process.
    /// </remarks>
    public class FileFinder
    {
        /// <summary>
        /// Find a file somewhere in the current directory -OR- in any one of the parent
        /// directories of the current directory
        /// </summary>
        /// <param name="_filename">
        /// The filename to look for. All directory information is STRIPPED from the filename.
        /// </param>
        /// <param name="_useExecutableDirectory">
        /// Tells the routine to make sure the directory containing this program's executable is
        /// used to find files.
        /// </param>
        /// <returns>
        /// The full pathname for the file if it is found, or NULL if no file with that name was
        /// found anywhere in the hierarchy.
        /// </returns>
        public static string FindFileUpHierarchy( string _filename, bool _useExecutableDirectory = false )
        {
            var dir = _useExecutableDirectory
                            ? System.Reflection.Assembly.GetExecutingAssembly().Location
                            : Directory.GetCurrentDirectory();
            var fname = Path.GetFileName( _filename );
            return FindFileUpHierarchyRecursive( dir, fname );
        }

        /// <summary>
        /// Internal function used to recursively find the file in this hierarchy
        /// </summary>
        /// <param name="_directoryName">The current directory that we're looking at</param>
        /// <param name="_filename">
        /// The filename of the file, with all directory information stripped already.
        /// </param>
        /// <returns>
        /// NULL if we're looking at the "top" directory already and the file wasn't found
        /// there, or the full filename including path if the file WAS found in this directory,
        /// OR the recursive result of calling this function for the directory "above" this
        /// directory.
        /// </returns>
        private static string FindFileUpHierarchyRecursive( string _directoryName, string _filename )
        {
            var fullName = Path.Combine( _directoryName, _filename );
            if (File.Exists( fullName ))
                return fullName;

            var dname = Path.GetDirectoryName( _directoryName );
            if (string.IsNullOrEmpty( dname ))
                return null;

            return FindFileUpHierarchyRecursive( dname, _filename );
        }
    }
}
