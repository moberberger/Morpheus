using System;
using System.Collections.Generic;
using System.IO;

namespace Morpheus
{
    /// <summary>
    /// helper class to translate a filename between using volume labels and device letters
    /// preceding the ":"
    /// </summary>
    /// <remarks>
    /// By way of example, if your "C" drive had the label "System", and your "D" drive has the
    /// label "Data"...
    /// 
    /// <code>
    /// static void Main( string[] args )
    /// {
    ///     Console.WriteLine( CVolumeLabelDeviceLetter.ToVolumeLabel( "d:" ) );
    ///     Console.WriteLine( CVolumeLabelDeviceLetter.ToDeviceLetter( "System:" ) );
    /// }
    /// </code>
    /// 
    /// would create the following output:
    /// 
    /// <code>
    /// 
    /// Data:
    /// C:
    /// 
    /// </code>
    /// 
    /// NOTE- The colons are required!
    /// </remarks>
    public static class VolumeLabelDeviceLetter
    {
        /// <summary>
        /// Translate a full path name that includes a volume label into a "proper"
        /// path/filename where the volume label has been replaced with the drive letter of the
        /// volume. ASSUME that any single letter device name is a drive letter.
        /// </summary>
        /// <param name="_filename">
        /// The filename (or directory name) that contains a volume label instead of a drive
        /// letter
        /// </param>
        /// <returns>
        /// A full file/path with a drive letter substituted for a volume label, if one exists
        /// </returns>
        public static string ToDeviceLetter( string _filename )
        {
            var firstColon = _filename.IndexOf( Path.VolumeSeparatorChar );
            if (firstColon < 2)
                return _filename;

            var label = _filename.Substring( 0, firstColon );

            // Now look through the resulting list for the volume label found in the string
            foreach (var di in GetSortedDrives())
            {
                if (di.IsReady)
                {
                    if (di.VolumeLabel.ToLower() == label.ToLower())
                        return di.Name.Substring( 0, 1 ) + _filename.Substring( firstColon );
                }
            }

            throw new DirectoryNotFoundException( "Volume Label Not Found: " + _filename );
        }

        /// <summary>
        /// Given a filename or directory name, check to see if there's a drive letter in it,
        /// and if there is, then replace that drive letter with the volume label for the drive
        /// (if it exists).
        /// </summary>
        /// <param name="_filename">The filename to translate</param>
        /// <returns>
        /// The filename with a valid device letter corresponding to a device with a volume
        /// label replaced with that volume label.
        /// </returns>
        public static string ToVolumeLabel( string _filename )
        {
            var firstColon = _filename.IndexOf( Path.VolumeSeparatorChar );
            if (firstColon != 1) // The separator better be at index 1, or its not a drive letter
                return _filename;

            var letter = _filename.Substring( 0, firstColon );

            // Now look through the resulting list for the volume label found in the string
            foreach (var di in GetSortedDrives())
            {
                if (di.IsReady)
                {
                    if (di.Name.Substring( 0, 1 ).ToLower() == letter.ToLower())
                    {
                        if (string.IsNullOrEmpty( di.VolumeLabel ))
                            return _filename;
                        else
                            return di.VolumeLabel + _filename.Substring( firstColon );
                    }
                }
            }

            return _filename;
        }


        /// <summary>
        /// Helper routine to build a list of reverse-sorted drives. Reverse-sorting places the
        /// "A" drive at the end, and since this is mostly "not ready" and causes a program to
        /// pause, we don't want to put it at the beginning of the list so its queried each and
        /// every time.
        /// </summary>
        /// <returns></returns>
        public static List<DriveInfo> GetSortedDrives()
        {
            // Create a new list and sort it BACKWARDS to avoid the "pause" caused by the floppy
            // drive's "IsReady" query for what should be most valid volume label lookups
            var drives = new List<DriveInfo>( DriveInfo.GetDrives() );
            drives.Sort( delegate ( DriveInfo _a, DriveInfo _b ) { return _b.Name.CompareTo( _a.Name ); } );
            return drives;
        }


    }
}
