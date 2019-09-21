using System.Reflection;

namespace Morpheus
{
    /// <summary>
    /// This class is used to remove trailing underscore from Protobuf fields, assuring that the first letter of the
    /// field name is capitalized.
    /// </summary>
    public class ProtobufFieldRenamer : IFieldRenamer
    {
        /// <summary>
        /// This method will check the name of a field for the "_" postfix and change it to something 
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
            var len = _name.Length;
            if (_name[len - 1] == '_')
                _name = _name.Substring( 0, len - 1 );

            if (char.IsLower( _name[0] ))
                _name = char.ToUpper( _name[0] ) + _name.Substring( 1, _name.Length - 1 );

            return _name;
        }
    }
}
