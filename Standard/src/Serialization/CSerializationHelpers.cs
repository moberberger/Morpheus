using System;
using System.IO;
using System.Text;
using System.Xml;


namespace Morpheus
{
    /// <summary>
    /// This class contains methods, mainly static, to serialize and deserialize objects to/from formats other than 
    /// XmlNodes and derivatives thereof.
    /// </summary>
    public class CSerializationHelpers
    {
        /// <summary>
        /// This helper will use a default CSerializer to serialize an object to a named file. It will overwrite that file.
        /// </summary>
        /// <param name="_object">The object to serializat</param>
        /// <param name="_filename">The name of the file to save the serialized object to</param>
        public static void ToFile( object _object, string _filename )
        {
            var s = new CSerializer();
            ToFile( _object, _filename, s );
        }

        /// <summary>
        /// This helper will use a default CSerializer to serialize an object to a named file. It will overwrite that file.
        /// It will use a specified <see cref="CSerializationContext"/> to control the serialization
        /// </summary>
        /// <param name="_object">The object to serializat</param>
        /// <param name="_filename">The name of the file to save the serialized object to</param>
        /// <param name="_context">The Serialization Context to use for the serialization</param>
        public static void ToFile( object _object, string _filename, CSerializationContext _context = null )
        {
            var s = new CSerializer( _context );
            ToFile( _object, _filename, s );
        }

        /// <summary>
        /// This helper will use a default CSerializer to serialize an object to a named file. It will overwrite that file.
        /// </summary>
        /// <param name="_object">The object to serializat</param>
        /// <param name="_filename">The name of the file to save the serialized object to</param>
        /// <param name="_serializer">The Serializer to use in the serialization of the object</param>
        public static void ToFile( object _object, string _filename, CSerializer _serializer )
        {
            var doc = _serializer.Serialize( _object );
            doc.Save( _filename );
        }


        /// <summary>
        /// This helper will turn an object into a properly formatted XML document, as a string.
        /// </summary>
        /// <param name="_object">The object to turn into a string</param>
        /// <param name="_context">The Serialization Context to use, if the default one won't work.</param>
        /// <returns>The object serialized as a string-form XML document</returns>
        public static string ToString( object _object, CSerializationContext _context = null )
        {
            var ser = new CSerializer( _context );
            var xml = ser.Serialize( _object );
            xml.DocumentElement.Attributes.RemoveAll();
            var str = new StringWriter();
            xml.Save( str );
            return str.ToString();
        }


        /// <summary>
        /// Retrieve a deserialized object from a file
        /// </summary>
        /// <typeparam name="T">The Type of the object that is to be deserialized</typeparam>
        /// <param name="_filename">The name of the file that contains the information for deserialization</param>
        /// <param name="_context">Serialization context for the operation</param>
        /// <returns>The object that was deserialized</returns>
        public static T FromFile<T>( string _filename, CSerializationContext _context = null )
        {
            var d = new CDeserializer( _context );
            return FromFile<T>( _filename, d );
        }

        /// <summary>
        /// Given a filename and a deserializer, return the object that was created from deserializing the Xml in the file
        /// </summary>
        /// <typeparam name="T">The Type of the object that is to be deserialized</typeparam>
        /// <param name="_filename">The name of the file that contains the information for deserialization</param>
        /// <param name="_deserializer">The Deserializer to use for the deserialization</param>
        /// <returns>The object that was deserialized</returns>
        public static T FromFile<T>( string _filename, CDeserializer _deserializer )
        {
            var doc = new XmlDocument();
            doc.Load( _filename );
            return _deserializer.Deserialize<T>( doc );
        }

    }
}
