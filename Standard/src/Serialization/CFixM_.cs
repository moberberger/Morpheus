using System.Reflection;

namespace Morpheus
{
    /// <summary>
    /// This class is used to globally check a field name to see if it starts with "m_" followed
    /// by a lower-case character. If it does, then that name is changed to remove the m_ and switch
    /// the lower-case char to an upper-case char.
    /// </summary>
    /// <example>
    /// Field Name      Changed To      Reason
    /// ----------------------------------------------------------------------------------------------
    /// Name            Name            No change- there's no "m_"
    /// address         address         No change- there's no "m_"
    /// m_age           Age             Changed because the field follows the Morpheus coding standard
    /// m_x             X               Changed because even single-char fields are OK
    /// m_              m_              There is no third char, so nothing is changed
    /// m_Name          m_Name          No change because the third char is not lowercase
    /// </example>
    /// <remarks>
    /// The Morpheus coding standard demands that private fields begin with "m_" and use a camel-case 
    /// descriptor thereafter.
    /// </remarks>
    public class CFixM_ : IFieldRenamer
    {
        /// <summary>
        /// This method will check the name of a field for the "m_" prefix and change it to something 
        /// "more readable" if reasonable.
        /// </summary>
        /// <param name="_fieldName">The name of the field to convert</param>
        /// <param name="_fieldInfo">The <see cref="FieldInfo"/> for the field</param>
        /// <returns>The new name of the field (may be the same as the original!)</returns>
        public string ConvertFieldName( string _fieldName, FieldInfo _fieldInfo ) => ConvertName( _fieldName );

        /// <summary>
        /// This method will check the name of a field for the "m_" prefix and change it to something 
        /// "more readable" if reasonable.
        /// </summary>
        /// <param name="_name">The name of the field to try to change</param>
        /// <returns>The new name of the field (may be the same as the original!)</returns>
        public static string ConvertName( string _name )
        {
            // Check the length- the string cannot match if its not at least 3 chars long
            if (_name.Length < 3)
                return _name;

            // Check to see if the first, second and third characters match the m_ pattern
            if (_name[0] != 'm' || _name[1] != '_' || !char.IsLower( _name[2] ))
                return _name;

            // This field name passes the test, so build the new name and return.
            var firstChar = (char) (_name[2] - 'a' + 'A');
            var rest = _name.Substring( 3 );

            return firstChar + rest;
        }
    }
}