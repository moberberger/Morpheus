using System;
using System.Xml;

namespace Morpheus
{
    /// <summary>
    /// This class implements <see cref="IExternalSurrogate"/> and provides an alternate format for 
    /// DateTime structures. This format follows the UTC "Complete date plus hours, minutes and seconds"
    /// </summary>
    public class CUtcDateTimeSurrogate : IExternalSurrogate
    {
        private static readonly CUtcDateTimeSurrogate sm_serializationSurrogate = new CUtcDateTimeSurrogate();

        /// <summary>
        /// The IExternalSurrogate that can be used to serialize and deserialize DateTime structures.
        /// </summary>
        public static IExternalSurrogate Surrogate => sm_serializationSurrogate;

        /// <summary>
        /// The DateTime customer format string for the UTC "Complete date plus hours, minutes and seconds"
        /// </summary>
        public const string UTC_COMPLETE_DATE_TIME_FORMAT = "yyyy-MM-ddTHH:mm:sszzz";

        /// <summary>
        /// Turn a DateTime struct into a complete UTC format.
        /// </summary>
        /// <param name="_dateTime">The DateTime struct to convert</param>
        /// <returns>A string representing the DateTime in UTC (complete) format.</returns>
        public static string ToString( DateTime _dateTime ) => _dateTime.ToString( UTC_COMPLETE_DATE_TIME_FORMAT );

        /// <summary>
        /// Turn a string assumed to contain a date/time in complete UTC form into a DateTime struct.
        /// </summary>
        /// <param name="_dateTime">The string version of the DateTime</param>
        /// <returns>The DateTime form of the string</returns>
        public static DateTime FromString( string _dateTime ) => DateTime.ParseExact( _dateTime, UTC_COMPLETE_DATE_TIME_FORMAT, null );

        /// <summary>
        /// This is the SerializationSurrogate used to turn a <see cref="DateTime"/> structure into a UTC
        /// "Complete date plus hours, minutes and seconds" string.
        /// </summary>
        /// <param name="_object">The (presumed to be) DateTime object</param>
        /// <param name="_useType">Treat the "_object" parameter as if it were of this type</param>
        /// <param name="_parentNode">The node to place the UTC string into</param>
        /// <param name="_serializer">The serializer- not used.</param>
        /// <returns>"true", because this routine completely serializes the DateTime</returns>
        public bool Serialize( object _object, Type _useType, XmlElement _parentNode, CSerializer _serializer )
        {
            var dt = (DateTime) _object;
            _parentNode.InnerText = ToString( dt );
            return true;
        }

        /// <summary>
        /// This is the Deserializer to turn a UTC "Complete date plus hours, minutes and seconds" string
        /// back into a DateTime structure.
        /// </summary>
        /// <param name="_workingObject">The object that is to receive the new DateTime structure</param>
        /// <param name="_parentNode">The node whose InnerText contains the UTC string</param>
        /// <param name="_deserializer">The deserializer- not used.</param>
        /// <returns>"true", because this routine completely deserializes the DateTime</returns>
        public bool Deserialize( CWorkingObject _workingObject, XmlElement _parentNode, CDeserializer _deserializer )
        {
            var asString = _parentNode.InnerText;
            var dt = FromString( asString );
            _workingObject.Set( dt );
            return true;
        }
    }
}