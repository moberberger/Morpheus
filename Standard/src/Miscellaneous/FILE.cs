namespace Morpheus;

/// <summary>
/// This class helps an application find a file that is either in the application's current
/// running directory OR its somewhere "up" the directory structure of the Current Directory.
/// </summary>
/// <remarks>
/// Usually, when a program is "run" from within Visual Studio, it is run from the
/// "[appName]/bin/debug" subdirectory. Many times you would like to place a data file in the
/// "[appName]" directory and not have to worry about messing around trying to find it. This
/// class helps make finding that file a painless process.
/// </remarks>
public static class FILE
{
    /// <summary>
    /// Find a file somewhere in the current directory -OR- in any one of the parent directories
    /// of the current directory
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
    /// NULL if we're looking at the "top" directory already and the file wasn't found there, or
    /// the full filename including path if the file WAS found in this directory, OR the
    /// recursive result of calling this function for the directory "above" this directory.
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


    /// <summary>
    /// Create a new Version of a file if it exists. Allows CreateNew without replacing existing
    /// file. Versioned filenames inject a semi-colon + numeric string immediately before the
    /// extension. E.g.
    /// 
    /// D:\TMP\SomeFile;5.TXT
    /// 
    /// This is version 5 of the file. The highest version is the most recently created version.
    /// </summary>
    /// <param name="filename">The filename to create a version of, if it exists</param>
    /// <returns>
    /// The Version Number assigned to the file if it existed, 0 if the file didn't exist (not
    /// an error!)
    /// </returns>
    public static int VersionFile( string filename )
    {
        if (!File.Exists( filename ))
            return 0;

        var dir = Path.GetDirectoryName( filename );
        var fname = Path.GetFileNameWithoutExtension( filename );
        var ext = Path.GetExtension( filename );

        if (string.IsNullOrWhiteSpace( dir ))
            dir = Environment.CurrentDirectory;

        var tmp = fname + ";*" + ext;
        var files = Directory.GetFiles( dir, tmp );
        var versions = files.SelectWithRegex( @";(\d+)\.[^.]*$" );
        var maxVersion = versions.IsEmpty() ? 0 : versions.Select( v => int.Parse( v ) ).Max();
        var newVersion = maxVersion + 1;
        var versionedFname = $"{fname};{newVersion}";
        var vf2 = Path.Combine( dir, versionedFname );
        var vf3 = Path.ChangeExtension( vf2, ext );

        File.Move( filename, vf3 );
        return newVersion;
    }

    /// <summary>
    /// Add some token to a filename between the extension and the name. For example, adding
    /// "BACKUP" to "T.TXT" yields "T.BACKUP.TXT"
    /// </summary>
    /// <param name="_filename">The filename</param>
    /// <param name="_whatToAdd">The token to add to the filename (before the extension)</param>
    /// <returns>The resulting filename</returns>
    public static string AddSomethingToFilename( string _filename, string _whatToAdd )
    {
        if (string.IsNullOrWhiteSpace( _whatToAdd )) return _filename;

        var s = Path.GetFileNameWithoutExtension( _filename );
        if (_whatToAdd[0] != '.') s += ".";
        s += _whatToAdd;
        s += Path.GetExtension( _filename );
        s = Path.Combine( Path.GetDirectoryName( _filename ), s );
        s = Path.Combine( Path.GetPathRoot( _filename ), s );
        return s;
    }

}
