using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Xml;


namespace Morpheus.Standard.UnitTests
{
    /// <summary>
    /// This class tests the CXmlHelper class from MorpheusUtil
    /// </summary>
    [TestClass]
    public class CXmlHelperTest
    {
        private XmlDocument m_xml;

        [TestInitialize]
        public void InitializeTest()
        {
            m_xml = new XmlDocument();
            m_xml.LoadXml( "<a>Ape<b>Bear</b><b type='pig'>Boar<c>Cat</c></b><d>Dog<e>Elephant</e></d></a>" );
        }


        [TestMethod]
        [TestCategory( "XmlHelper" )]
        public void AddAttributeTest()
        {
            var e = m_xml.DocumentElement;
            e = e["b"];
            const string v1 = "furry";
            e.AddAttribute( v1, true );

            var node = m_xml.DocumentElement.SelectSingleNode( @"/a/b[@furry]/attribute::furry" );
            Assert.AreEqual<string>( "True", node.Value, "Getting the 'Furry' attribute" );

            var lookHere = m_xml.DocumentElement.SelectSingleNode( @"//e" );
            lookHere.AddAttribute( "trunk", "yes" );
            node = m_xml.DocumentElement.SelectSingleNode( @"/a/d/e/attribute::trunk" );
            Assert.AreEqual<string>( "yes", node.Value, "Elephant's Trunk" );

            var actual = lookHere.GetAttributeValue( "trunk" );
            Assert.AreEqual<string>( "yes", actual, "its wrong from the GetAttributeValue method" );
            actual = lookHere.GetAttributeValue( "hoser" );
            Assert.IsNull( actual, "The 'horse' shouldn't exist." );

            actual = lookHere.GetRequiredAttribute( "trunk" );
            Assert.AreEqual<string>( "yes", actual, "The Trunk was there" );

            try
            {
                XmlExtensions.GetAttributeValue( null, "homer" );
                Assert.Fail( "Expected exception of type '" + typeof( ArgumentNullException ).Name + "' but none was thrown." );
            }
            catch (ArgumentNullException) { }
            catch (Exception __e)
            {
                Assert.Fail( "Expected exception of type ArgumentNullException, but " + __e.GetType().ToString() + " was generated instead." );
            }

            try
            {
                XmlExtensions.GetRequiredAttribute( null, "homer" );
                Assert.Fail( "Expected exception of type '" + typeof( ArgumentNullException ).Name + "' but none was thrown." );
            }
            catch (ArgumentNullException) { }
            catch (Exception __e)
            {
                Assert.Fail( "Expected exception of type ArgumentNullException, but " + __e.GetType().ToString() + " was generated instead." );
            }

            try
            {
                lookHere.GetRequiredAttribute( "homer" );
                Assert.Fail( "Expected exception of type '" + typeof( ArgumentException ).Name + "' but none was thrown." );
            }
            catch (ArgumentException) { }
            catch (Exception __e)
            {
                Assert.Fail( "Expected exception of type ArithmeticException, but " + __e.GetType().ToString() + " was generated instead." );
            }
        }

        [TestMethod]
        [TestCategory( "XmlHelper" )]
        public void HasAttributeTest()
        {
            var node = m_xml.SelectSingleNode( @"//*[@type]" );
            Assert.IsTrue( XmlExtensions.HasAttribute( node, "type" ), "Expected an attribute on the node in question." );
            Assert.IsFalse( XmlExtensions.HasAttribute( node, "genre" ), "Did not expect the attribute." );

            try
            {
                XmlExtensions.HasAttribute( null, "homer" );
                Assert.Fail( "Expected exception of type '" + typeof( ArgumentNullException ).Name + "' but none was thrown." );
            }
            catch (ArgumentNullException) { }
            catch (Exception __e)
            {
                Assert.Fail( "Expected exception of type ArgumentNullException, but " + __e.GetType().ToString() + " was generated instead." );
            }
        }

        [TestMethod]
        [TestCategory( "XmlHelper" )]
        public void CreateElementTest()
        {
            var e1 = m_xml.CreateElement( "homer" );
            Assert.AreEqual<string>( "homer", e1.Name, "Name of new node is wrong" );

            var node = m_xml.SelectSingleNode( @"//*[@type]" );
            var e2 = node.CreateElement( "simpson" );
            Assert.AreEqual<string>( "simpson", e2.Name, "Name of node based on non-XmlDocument is wrong." );

            var e3 = e2.CreateSimpleElement( "Moe", "is PIMP" );
            Assert.AreEqual<string>( "Moe", e3.Name, "Name of element w/ inner text is wrong" );
            Assert.AreEqual<string>( "is PIMP", e3.InnerText, "Inner text is wrong." );
        }

        [TestMethod]
        [TestCategory( "XmlHelper" )]
        public void AddElementTest()
        {
            var doc = new XmlDocument();
            var simpson = XmlExtensions.AddElement( doc, "Simpson" );
            XmlExtensions.AddElement( simpson, "Homer", "Father" );
            XmlExtensions.AddElement( simpson, "Marge", "Mother" );

            Assert.AreEqual<string>( "Simpson", doc.DocumentElement.Name, "DocElement name is wrong" );
            Assert.AreEqual<int>( 2, doc.DocumentElement.ChildNodes.Count, "Number of child nodes is wrong." );
            Assert.AreEqual<string>( "Homer", doc.DocumentElement.ChildNodes[0].Name, "Name of 1st node is wrong" );
            Assert.AreEqual<string>( "Father", doc.DocumentElement.ChildNodes[0].InnerText, "Contents of 1st node is wrong" );
            Assert.AreEqual<string>( "Marge", doc.DocumentElement.ChildNodes[1].Name, "Name of 2nd node is wrong" );
            Assert.AreEqual<string>( "Mother", doc.DocumentElement.ChildNodes[1].InnerText, "Contents of 2nd node is wrong" );

            XmlExtensions.AddElementWithAttribute( simpson, "Bart", "Child", 1 );
            Assert.AreEqual<string>( "Bart", doc.DocumentElement.ChildNodes[2].Name, "Bart is wrong" );
            Assert.AreEqual<string>( "Child", doc.DocumentElement.ChildNodes[2].Attributes[0].Name, "Name of Bart's attribute is wrong." );
            Assert.AreEqual<string>( "1", doc.DocumentElement.ChildNodes[2].Attributes[0].Value, "Value of Bart's attribute is wrong." );
        }

        [TestMethod]
        [TestCategory( "XmlHelper" )]
        public void AddElementToDocTest()
        {
            var doc = new XmlDocument();
            XmlExtensions.AddElementWithAttribute( doc, "Griffin", "Dysfunctional", "Very" );
            Assert.AreEqual<string>( "Griffin", doc.DocumentElement.Name, "Name of the main element is wrong" );
            Assert.AreEqual<string>( "Dysfunctional", doc.DocumentElement.Attributes[0].Name, "Name of attribute is wrong." );
            Assert.AreEqual<string>( "Very", doc.DocumentElement.Attributes[0].Value, "Value of the attribute is wrong" );

            try
            {
                XmlNode node = doc.DocumentElement.Attributes[0];
                XmlExtensions.AddElementWithAttribute( node, "This", "Should", "not work!" );
                Assert.Fail( "Expected exception of type '" + typeof( ArgumentException ).Name + "' but none was thrown." );
            }
            catch (ArgumentException) { }
            catch (Exception __e)
            {
                Assert.Fail( "Expected exception of type ArgumentException, but " + __e.GetType().ToString() + " was generated instead." );
            }
        }

        [TestMethod]
        [TestCategory( "XmlHelper" )]
        public void GetElementValueTest()
        {
            var actual = XmlExtensions.GetElementValue( m_xml, "/a/b" );
            Assert.AreEqual<string>( "Bear", actual, "The value of /a/b is wrong" );

            var shouldBeNull = XmlExtensions.GetElementValue( m_xml, "/a/ff" );
            Assert.IsNull( shouldBeNull, "Should be null when the element can't be found" );
        }

        [TestMethod]
        [TestCategory( "XmlHelper" )]
        public void GetRequiredElementTest()
        {
            var actual = XmlExtensions.GetRequiredElement( m_xml, "/a/b" );
            Assert.AreEqual<string>( "Bear", actual, "The value of /a/b is wrong" );

            try
            {
                XmlExtensions.GetRequiredElement( m_xml, "/a/dddd" );
                Assert.Fail( "Expected exception of type '" + typeof( ArgumentException ).Name + "' but none was thrown." );
            }
            catch (ArgumentException) { }
            catch (Exception __e)
            {
                Assert.Fail( "Expected exception of type ArgumentException, but " + __e.GetType().ToString() + " was generated instead." );
            }
        }

        [TestMethod]
        [TestCategory( "XmlHelper" )]
        public void RemoveAttributeTest()
        {
            var node = m_xml.SelectSingleNode( "//*[@type]" );
            Assert.AreEqual<string>( "BoarCat", node.InnerText, "Expected something else for the inner text of the node with a 'type' attribute" );
            Assert.AreEqual<int>( 1, node.Attributes.Count, "Count of attributes is wrong" );

            var removed = XmlExtensions.RemoveAttribute( node, "type" );
            Assert.AreEqual<int>( 0, node.Attributes.Count, "Count of attributes is wrong after removing one" );
            Assert.IsTrue( removed, "Something should have been removed." );

            removed = XmlExtensions.RemoveAttribute( m_xml, "notThere" );
            Assert.IsFalse( removed, "Nothing should have been removed" );
        }

        [TestMethod]
        [TestCategory( "XmlHelper" )]
        public void DepthTest()
        {
            var actual = XmlExtensions.Depth( m_xml );
            Assert.AreEqual<int>( 4, actual, "Depth is not right" );

            actual = XmlExtensions.Depth( null );
            Assert.AreEqual<int>( 0, actual, "Null should have zero depth" );
        }

        [TestMethod]
        [TestCategory( "XmlHelper" )]
        public void ElementCountTest()
        {
            var actual = XmlExtensions.ElementCount( m_xml );
            Assert.AreEqual<int>( 6, actual, "Element count is wrong" );

            actual = XmlExtensions.ElementCount( null );
            Assert.AreEqual<int>( 0, actual, "Null should have zero elements" );

            var node = m_xml.SelectSingleNode( "//*[@type]" );
            actual = XmlExtensions.ElementCount( node.Attributes[0] );
            Assert.AreEqual<int>( 0, actual, "An attribute node should not be counted as an element." );
        }

        [TestMethod]
        [TestCategory( "XmlHelper" )]
        public void EncodeDecodeStringTest()
        {
            string str, enc, dec, exp;

            str = "Hello There";
            enc = XmlExtensions.EncodeString( str );
            Assert.AreEqual( str, enc, "Encoded String 1" );
            dec = XmlExtensions.DecodeString( enc );
            Assert.AreEqual( str, dec, "Decoded String 1" );

            str = "Greater > Less < Dot . Done!";
            exp = "Greater _.g Less _.l Dot _.d Done!";
            enc = XmlExtensions.EncodeString( str );
            Assert.AreEqual( exp, enc, "Encoded String 2" );
            dec = XmlExtensions.DecodeString( enc );
            Assert.AreEqual( str, dec, "Decoded String 2" );

            str = "<hello>";
            exp = "_.lhello_.g";
            enc = XmlExtensions.EncodeString( str );
            Assert.AreEqual( exp, enc, "Encoded String 3" );
            dec = XmlExtensions.DecodeString( enc );
            Assert.AreEqual( str, dec, "Decoded String 3" );
        }

        [TestMethod]
        [TestCategory( "XmlHelper" )]
        public void DecodeMalformedStringTest()
        {
            string str, dec, exp;

            str = "Hello . There";
            dec = XmlExtensions.DecodeString( str );
            Assert.AreEqual( str, dec, "Decoded String 1" );

            str = "Hello .l There";
            dec = XmlExtensions.DecodeString( str );
            Assert.AreEqual( str, dec, "Decoded String 2" );

            str = "Hello .? There";
            dec = XmlExtensions.DecodeString( str );
            Assert.AreEqual( str, dec, "Decoded String 3" );

            str = "Hello .?. There";
            dec = XmlExtensions.DecodeString( str );
            Assert.AreEqual( str, dec, "Decoded String 4" );

            str = "Hello_.";
            dec = XmlExtensions.DecodeString( str );
            Assert.AreEqual( str, dec, "Decoded String 5" );

            str = "Hello..l";
            dec = XmlExtensions.DecodeString( str );
            Assert.AreEqual( str, dec, "Decoded String 6" );


            str = "Hello __.l";
            exp = "Hello _<";
            dec = XmlExtensions.DecodeString( str );
            Assert.AreEqual( exp, dec, "Decoded string 7" );

            str = "Hello_World";
            dec = XmlExtensions.DecodeString( str );
            Assert.AreEqual( str, dec, "Decoded String 8" );


            str = "Hello_._.dWorld";
            exp = "Hello_..World";
            dec = XmlExtensions.DecodeString( str );
            Assert.AreEqual( exp, dec, "Decoded String 9" );

            str = "Hello_.World";
            dec = XmlExtensions.DecodeString( str );
            Assert.AreEqual( str, dec, "Decoded String 10" );

        }



        [TestMethod]
        [TestCategory( "XmlHelper" )]
        public void FinishCodeCoverageCheese() => XmlExtensions.GetFormattedString( m_xml );





    }
}
