using System;
using System.IO;
using System.Xml;

public class CBaseXmlPrinter
{
    public static void Print( XmlDocument _doc )
    {
        var s = new StringWriter();
        var xs = new XmlTextWriter( s )
        {
            Formatting = Formatting.Indented,
            Indentation = 8
        };

        _doc.Save( xs );
        Console.WriteLine( s );
    }

    public static void Print() => Console.WriteLine(
            "\r\n\r\n-----------------------------------------------------------------------------\r\n\r\n" );
}