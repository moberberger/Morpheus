namespace Morpheus;

public static class ISO8601
{
    public static string ToIso8601( DateTime dateTime ) =>
        dateTime.ToUniversalTime().ToString( "yyyy-MM-dd'T'HH:mm:ss'Z'" );

    public static DateTime FromIso8601( string iso8601 ) =>
        DateTime.Parse( iso8601 );
}
