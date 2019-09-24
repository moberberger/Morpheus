using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Morpheus
{
    /// <summary>
    /// A helper to do various simple operations on XML documents Elements
    /// </summary>
    public static class XmlExtensions
    {
        /// <summary>
        /// Helper function to add an attribute, including the name and value, to an Xml node
        /// </summary>
        /// <param name="_node">The node that is receiving the attribute</param>
        /// <param name="_name">The name of the attribute</param>
        /// <param name="_value">The value of the attribute, which will be turned into a string with ".ToString"</param>
        public static void AddAttribute( this XmlNode _node, string _name, object _value )
        {
            var a = _node.OwnerDocument.CreateAttribute( _name );
            a.Value = _value.ToString();
            _node.Attributes.Append( a );
        }

        /// <summary>
        /// Helper function to add an attribute, including the name and value, to an Xml node
        /// </summary>
        /// <param name="_node">The node that is receiving the attribute</param>
        /// <param name="_name">The name of the attribute</param>
        /// <param name="_value">The value of the attribute, which will be turned into a string with ".ToString"</param>
        public static void AddAttribute( this XmlNode _node, string _name, string _value )
        {
            var a = _node.OwnerDocument.CreateAttribute( _name );
            a.Value = _value;
            _node.Attributes.Append( a );
        }

        /// <summary>
        /// Get the value of the named attribute. Return NULL if that name doesn't exist in the attributes.
        /// </summary>
        /// <param name="_node">The node (presumably an XmlElement) that contains the attribute whose value is interesting to the application</param>
        /// <param name="_name">The name of the attribute</param>
        /// <returns>The value of the attribute whose name is passed in, or NULL if that attribute does not exist on the node</returns>
        public static string GetAttributeValue( this XmlNode _node, string _name )
        {
            if (_node == null)
            {
                throw new ArgumentNullException(
                    "The node is not allowed to be null when querying for an Attribute Value" );
            }

            var a = _node.Attributes[_name];
            return a?.Value;
        }

        /// <summary>
        /// Get the value of the named attribute. Return NULL if that name doesn't exist in the attributes.
        /// </summary>
        /// <param name="_node">The node (presumably an XmlElement) that contains the attribute whose value is interesting to the application</param>
        /// <param name="_name">The name of the attribute</param>
        /// <returns>The value of the attribute whose name is passed in, or NULL if that attribute does not exist on the node</returns>
        public static string GetRequiredAttribute( this XmlNode _node, string _name )
        {
            if (_node == null)
            {
                throw new ArgumentNullException(
                    "The node is not allowed to be null when querying for an Attribute Value" );
            }

            var a = _node.Attributes[_name];
            if (a == null)
                throw new ArgumentException( "Argument does not exist.", _name );

            return a.Value;
        }

        /// <summary>
        /// Determine if a named attribute is found in an element's attribute list.
        /// </summary>
        /// <param name="_node">The node to search for the attribute</param>
        /// <param name="_name">The name of the attribute to try to find</param>
        /// <returns>TRUE if an attribute exists with the name specified</returns>
        public static bool HasAttribute( this XmlNode _node, string _name )
        {
            if (_node == null)
            {
                throw new ArgumentNullException(
                    "The node is not allowed to be null when querying for an Attribute Value" );
            }

            return _node.Attributes.GetNamedItem( _name ) == null ? false : true;
        }


        /// <summary>
        /// Given any Xml node, create a new Element node using the document that owns the passed in node.
        /// </summary>
        /// <param name="_anyNode">Any node in the document, or the document itself</param>
        /// <param name="_name">The name of the element to be created.</param>
        /// <returns>A newly created XmlElement with the nodename of <paramref name="_name"/>.</returns>
        public static XmlElement CreateElement( this XmlNode _anyNode, string _name )
        {
            // Get a reference to the owner document first
            if (!(_anyNode is XmlDocument doc))
                doc = _anyNode.OwnerDocument;

            // Once we have the XmlDocument, its trivial
            var escapedName = EncodeString( _name );
            return doc.CreateElement( escapedName );
        }

        /// <summary>
        /// Given any Xml node, create a new Element node using the document that owns the passed in node. Add "InnerText" to the element
        /// </summary>
        /// <param name="_parent">Any node in the document, or the document itself.</param>
        /// <param name="_name">The name of the element to be created.</param>
        /// <param name="_innerTextObject">An object whose ".ToString()" value will be used to create the "InnerText" of the element</param>
        /// <returns>A newly created XmlElement with the nodename of <paramref name="_name"/>.</returns>
        public static XmlElement CreateSimpleElement( this XmlNode _parent, string _name, object _innerTextObject )
        {
            var e = CreateElement( _parent, _name );
            e.InnerText = _innerTextObject.ToString();
            return e;
        }

        /// <summary>
        /// Add an element to a parent node, naming the element in the process
        /// </summary>
        /// <param name="_parentNode">The parent node to contain the new element</param>
        /// <param name="_elementName">The name of the new element</param>
        /// <returns>The element that was added</returns>
        public static XmlElement AddElement( this XmlNode _parentNode, string _elementName )
        {
            var e = CreateElement( _parentNode, _elementName );
            _parentNode.AppendChild( e );
            return e;
        }

        /// <summary>
        /// Add an element to a parent node, naming the element in the process
        /// </summary>
        /// <param name="_parentNode">The parent node to contain the new element</param>
        /// <param name="_elementName">The name of the new element</param>
        /// <param name="_elementValue">The object whose .ToString value will be inserted into the new element</param>
        /// <returns>The element that was added</returns>
        public static XmlElement AddElement( this XmlNode _parentNode, string _elementName, object _elementValue )
        {
            var e = CreateSimpleElement( _parentNode, _elementName, _elementValue );
            _parentNode.AppendChild( e );
            return e;
        }

        /// <summary>
        /// Add a new element with a specific name to the parent node, along with a single attribute on that element which has a value
        /// </summary>
        /// <param name="_parentNode">The parent XmlElement to receive the new element</param>
        /// <param name="_elementName">The name of the new element</param>
        /// <param name="_attributeName">The name of the attribute</param>
        /// <param name="_attributeValue">The value for the attribute</param>
        /// <returns>The element that was created</returns>
        public static XmlElement AddElementWithAttribute( this XmlNode _parentNode,
                                                          string _elementName,
                                                          string _attributeName,
                                                          object _attributeValue )
        {
            if (!(_parentNode is XmlElement || _parentNode is XmlDocument))
                throw new ArgumentException( $"Parent Node must be an XmlElement or an XmlDocument, not a {_parentNode.GetType()}" );

            var e = CreateElement( _parentNode, _elementName );
            AddAttribute( e, _attributeName, _attributeValue );
            _parentNode.AppendChild( e );
            return e;
        }

        /// <summary>
        /// Given a node, presumably an XmlElement, find a child node whose name is specified and return the ".InnerText" of that node
        /// </summary>
        /// <param name="_root">The "parent node" to search</param>
        /// <param name="_nodeName">The name of the child-node to find</param>
        /// <returns>The "innerText" of the child-node if its found, otherwise NULL</returns>
        public static string GetElementValue( this XmlNode _root, string _nodeName )
        {
            var node = _root.SelectSingleNode( _nodeName );
            return node?.InnerText;
        }

        /// <summary>
        /// Given a node, presumably an XmlElement, find a child node whose name is specified and return the ".InnerText" of that node
        /// </summary>
        /// <param name="_root">The "parent node" to search</param>
        /// <param name="_nodeName">The name of the child-node to find</param>
        /// <returns>The "innerText" of the child-node if its found, otherwise NULL</returns>
        public static string GetRequiredElement( this XmlNode _root, string _nodeName )
        {
            var node = _root.SelectSingleNode( _nodeName );
            if (node == null)
                throw new ArgumentException( "Required Element not present- ", _nodeName );

            return node.InnerText;
        }

        /// <summary>
        /// Remove an attribute from a node.
        /// </summary>
        /// <param name="_node">The node that supposedly contains the attribute</param>
        /// <param name="_attributeName">The name of the attribute to remove</param>
        /// <returns>TRUE if the attribute was removed, FALSE if it was not (or didn't exist)</returns>
        public static bool RemoveAttribute( this XmlNode _node, string _attributeName )
        {
            if (_node is XmlDocument)
                _node = (_node as XmlDocument).DocumentElement;

            var attr = _node.Attributes[_attributeName];
            if (attr == null)
                return false;

            _node.Attributes.Remove( attr );
            return true;
        }

        /// <summary>
        /// Return a formatted string from an XmlDocument
        /// </summary>
        /// <param name="_document">The document to get the string for</param>
        /// <returns>The formatted XML for the document</returns>
        public static string GetFormattedString( this XmlDocument _document )
        {
            var s = new StringWriter();
            _document.Save( s );
            return s.ToString();
        }

        /// <summary>
        /// Find the "Depth" of an XmlNode.
        /// </summary>
        /// <param name="_node">The node to analyze</param>
        /// <returns>the "Depth" of the document- XmlText IS considered for depth.</returns>
        public static int Depth( this XmlNode _node )
        {
            if (_node == null)
                return 0;
            if (_node is XmlDocument)
                _node = ((XmlDocument) _node).DocumentElement;
            if (!(_node is XmlElement))
                return 1;

            var elem = (XmlElement) _node;
            var childDepth = 0;
            foreach (XmlNode node in elem.ChildNodes)
            {
                var d = Depth( node );
                if (d > childDepth)
                    childDepth = d;
            }
            return 1 + childDepth;
        }

        /// <summary>
        /// Count the number of XmlElements in the document.
        /// </summary>
        /// <param name="_node">The node to analyze</param>
        /// <returns>The number of XmlElements found in the document</returns>
        public static int ElementCount( this XmlNode _node )
        {
            if (_node == null)
                return 0;
            if (_node is XmlDocument)
                _node = ((XmlDocument) _node).DocumentElement;
            if (!(_node is XmlElement))
                return 0;

            var count = 0;
            var elem = (XmlElement) _node;
            foreach (XmlNode node in elem.ChildNodes)
            {
                if (node is XmlElement)
                    count += ElementCount( node );
            }
            return 1 + count; // Add in "this" node, which at this point in the code is assumed to be an element itself.
        }


        private static readonly Regex sm_replacer = new Regex( @"[ !@#$%^&*()+=_\-,.<>/?\\|`~]", RegexOptions.Compiled );

        /// <summary>
        /// This routine will try to turn a string into one that can be used as an element or attribute name by replacing invalid characters with "_"
        /// This is a destructive operation that will leave the length of the string unchanged, but it will not preserve any information about what the
        /// original character was.
        /// </summary>
        /// <param name="_name"></param>
        /// <returns></returns>
        public static string ForceValidName( string _name ) => sm_replacer.Replace( _name, "_" );

        /// <summary>
        /// This will convert a variable into a value that is usable as an XML element name. Unlike similar routines (XmlConvert.EncodeName,
        /// SecurityElement.Escape), this routine is useful when trying to turn a .NET variable/field/property name into an XmlElement name.
        /// 
        /// It assumes that, since the period (.) is an operator and not a part of a member name, the period is a better escape char
        /// than the underscore (_) used by XmlConvert.EncodeName. Also, since the escape chars used by SecurityElement.Escape are not
        /// valid XML element name characters, they can't be used either.
        /// 
        /// For now, only angle brackets and the period is "escaped". This is because auto-generated backing fields for Properties use 
        /// angle brackets in their names, and dots (periods) are not part of a valid field name.
        /// </summary>
        /// <remarks>
        /// A better choice altogether would be the use of the dash (-) as an escape character, as it cannot possibly ever be used in a
        /// .NET identifier. However, the XML specification prevents the dash from being the first token in an XmlElement name. Therefore,
        /// and since a major reason for the creation of this routine is to represent .NET reflected Fields as XML elements, and since
        /// auto-generated property backing fields take the form of <![CDATA[    <PropertyName>k__backingField    ]]>, there would 
        /// almost always need to be an escape character as the first character in a string.
        /// 
        /// http://www.w3.org/TR/2006/REC-xml-20060816/#NT-Names
        /// 
        /// </remarks>
        /// <param name="_name">The string needing escape chars to be used as an XmlElement name</param>
        /// <returns>A string with escape chars inserted</returns>
        public static string EncodeString( string _name )
        {
            var str = new StringBuilder( _name.Length + 32 ); // allow for a few substitutions

            for (var i = 0; i < _name.Length; i++)
            {
                var ch = _name[i];
                switch (ch) // Yes, a data-driven approach would be more elegant, but this is definitely faster to execute
                {
                case '<':
                    str.Append( "_.l" );
                    break;
                case '>':
                    str.Append( "_.g" );
                    break;
                case '.':
                    str.Append( "_.d" );
                    break;
                default:
                    str.Append( ch );
                    break;
                }
            }

            return str.ToString();
        }

        private enum EParseState { Normal, NeedSecondEscape, NeedTerminatingEscapeChar }

        /// <summary>
        /// Given a string encoded using <see cref="EncodeString"/>, decode the string into its original form
        /// </summary>
        /// <param name="_name">The string to decode, presumed to be used as the name of an XML element</param>
        /// <returns>The decoded string, presumably derived from an XML element</returns>
        public static string DecodeString( string _name )
        {
            var str = new StringBuilder( _name.Length ); // return value will never be longer than the encoded version
            var state = EParseState.Normal;
            const char firstEscape = '_';
            const char secondEscape = '.';

            for (var i = 0; i < _name.Length; i++)
            {
                var ch = _name[i];
                switch (state)
                {
                case EParseState.Normal:

                    if (ch == firstEscape)
                        state = EParseState.NeedSecondEscape;
                    else
                        str.Append( ch );
                    break;

                case EParseState.NeedSecondEscape:

                    if (ch == secondEscape) // The third char will trigger a decode, if its valid
                    {
                        state = EParseState.NeedTerminatingEscapeChar;
                    }
                    else if (ch == firstEscape) // there were two first escape chars in a row, so output one of them
                    {
                        str.Append( firstEscape ); // and don't change the state
                    }
                    else // Not either escape char, so treat this and the previous as literals
                    {
                        str.Append( _name[i - 1] ).Append( ch );
                        state = EParseState.Normal;
                    }
                    break;

                case EParseState.NeedTerminatingEscapeChar:

                    switch (ch)
                    {
                    case 'l':
                        str.Append( '<' );
                        state = EParseState.Normal;
                        break;
                    case 'g':
                        str.Append( '>' );
                        state = EParseState.Normal;
                        break;
                    case 'd':
                        str.Append( '.' );
                        state = EParseState.Normal;
                        break;

                    case firstEscape: // We found another FirstEscape, meaning that the previous two escape chars should be output literally
                        str.Append( firstEscape ).Append( secondEscape ); // Append the preceeding escape chars and pretend that THIS escape starts the escape sequence
                        state = EParseState.NeedSecondEscape;
                        break;

                    default:    // Unrecognized char- even if its the second escape char, treat the previous two chars (which are firstEscape and secondEscape) 
                        //  as verbatim (along with this one)
                        str.Append( firstEscape ).Append( secondEscape ).Append( ch ); // Append the preceeding escape chars along with this char
                        state = EParseState.Normal;
                        break;
                    }
                    break;

                default:
                    break;
                }
            }

            // Finalize- output any "pending" escape chars verbatim
            if (state != EParseState.Normal)
                str.Append( firstEscape );
            if (state == EParseState.NeedTerminatingEscapeChar)
                str.Append( secondEscape );

            return str.ToString();
        }

    }
}