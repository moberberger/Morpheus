using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace Morpheus.Standard.UnitTests
{
    [TestClass]
    public class CCsvImporterTest
    {
        [TestMethod]
        [TestCategory( "CsvImporter" )]
        public void TestRemoveTrailingWhitespace()
        {
            var s = "  hello  ";
            var sb = new StringBuilder( s );

            CsvImporter.RemoveTrailingWhitespace( sb );
            Assert.AreEqual( "  hello", sb.ToString(), "Simple Removal" );

            s = "hello";
            sb = new StringBuilder( s );
            CsvImporter.RemoveTrailingWhitespace( sb );
            Assert.AreEqual( "hello", sb.ToString(), "Removal of nothing" );

            sb = new StringBuilder( "  " );
            CsvImporter.RemoveTrailingWhitespace( sb );
            Assert.AreEqual( "", sb.ToString(), "Results in empty string" );

            try
            {
                CsvImporter.RemoveTrailingWhitespace( null );
            }
            catch (ArgumentNullException)
            {
                return;
            }
            Assert.Fail( "Expected ArgumentNullException from RemoveTrailingWhitespace" );
        }

        [TestMethod]
        [TestCategory( "CsvImporter" )]
        public void ParseStringExceptionTest()
        {
            try
            {
                CsvImporter.ParseString( null, ",", false ).ToList();
            }
            catch (ArgumentNullException)
            {
                try
                {
                    CsvImporter.ParseString( new StringBuilder( "" ), null, false ).ToList();
                }
                catch (ArgumentNullException)
                {
                    return;
                }
            }
            Assert.Fail( "Expected exception when passing in null" );
        }


        [TestMethod]
        [TestCategory( "CsvImporter" )]
        public void ParseStringSimpleTest()
        {
            var sb = new StringBuilder( "  hello  " );
            var en = CsvImporter.ParseString( sb, ",", true ).GetEnumerator();

            Assert.AreEqual( true, en.MoveNext(), "First Token" );
            Assert.AreEqual( "hello", en.Current, "Token Value" );
            Assert.AreEqual( false, en.MoveNext(), "No more Tokens" );

            sb = new StringBuilder( " Bob, \"Billy Bob\"" );
            en = CsvImporter.ParseString( sb, ",", true ).GetEnumerator(); // remove quotes

            Assert.AreEqual( true, en.MoveNext(), "First Token" );
            Assert.AreEqual( "Bob", en.Current, "Token 1 Value" );
            Assert.AreEqual( true, en.MoveNext(), "Second Token" );
            Assert.AreEqual( "Billy Bob", en.Current, "Token 2 Value" );
            Assert.AreEqual( false, en.MoveNext(), "No more Tokens" );

            en = CsvImporter.ParseString( sb, ",", false ).GetEnumerator(); // don't remove quotes

            Assert.AreEqual( true, en.MoveNext(), "First Token" );
            Assert.AreEqual( "Bob", en.Current, "Token 1 Value" );
            Assert.AreEqual( true, en.MoveNext(), "Second Token" );
            Assert.AreEqual( "\"Billy Bob\"", en.Current, "Token 2 Value" );
            Assert.AreEqual( false, en.MoveNext(), "No more Tokens" );
            Assert.AreEqual( false, en.MoveNext(), "No more Tokens #2" );
        }

        [TestMethod]
        [TestCategory( "CsvImporter" )]
        public void EmptyObjectTest()
        {
            var x = new CsvImporter();
            Assert.IsNull( x.GetColumnNames(), "Column Names" );
            Assert.IsNull( x.GetData(), "Get Data" );
        }

        [TestMethod]
        [TestCategory( "CsvImporter" )]
        public void InterestingStringParsingTest()
        {
            // Test "space" separator
            var sb = new StringBuilder( "  hello to  all  " );
            var list = CsvImporter.ParseString( sb, " ", true ).ToList();

            Assert.AreEqual( "", list[0], "0" );
            Assert.AreEqual( "", list[1], "1" );
            Assert.AreEqual( "hello", list[2], "2" );
            Assert.AreEqual( "to", list[3], "3" );
            Assert.AreEqual( "", list[4], "4" );
            Assert.AreEqual( "all", list[5], "5" );
            Assert.AreEqual( "", list[6], "6" );
            Assert.AreEqual( "", list[7], "7" );
            Assert.AreEqual( 8, list.Count, "Count 1" );

        }

        [TestMethod]
        [TestCategory( "CsvImporter" )]
        public void InterestingStringParsingTest2()
        {
            // Test "quote nested" separator
            var sb = new StringBuilder( "\" First, Name\", Last    \t, Name, " );
            var list = CsvImporter.ParseString( sb, ",", true ).ToList();

            Assert.AreEqual( 4, list.Count, "Count" );
            Assert.AreEqual( " First, Name", list[0], "0" );
            Assert.AreEqual( "Last", list[1], "1" );
            Assert.AreEqual( "Name", list[2], "2" );
            Assert.AreEqual( "", list[3], "3" );
        }

        [TestMethod]
        [TestCategory( "CsvImporter" )]
        public void ColumnHeaderTest()
        {
            var s = "First Name, Last  Name, Age";
            var csv = new CsvImporter();
            var onHeaderReadCalled = false;

            csv.OnHeaderRead += _sb =>
                {
                    Assert.AreEqual( s, _sb.ToString(), "Header string should match" );
                    onHeaderReadCalled = true;
                };
            csv.ColumnNameSpaceReplacement = "_";

            csv.ImportString( s );

            Assert.IsTrue( onHeaderReadCalled, "Should have called OnHeaderRead lambda" );

            var data = csv.GetData();
            Assert.AreEqual( 0, data.Count, "No Data- should be zero length" );

            var cols = csv.GetColumnNames();
            Assert.IsNotNull( cols );
            Assert.AreEqual( 3, cols.Count, "Column Count" );
            Assert.AreEqual( "First_Name", cols[0], "0" );
            Assert.AreEqual( "Last__Name", cols[1], "1" );
            Assert.AreEqual( "Age", cols[2], "2" );
        }


        private class CTestPerson
        {
            public int Age = 0;
            public string LastName { get; set; }
            public string FirstName = null;
            public double Height = 0;

            public void TestPerson( string fname, string lname, int age, double height )
            {
                Assert.AreEqual( lname, LastName, "Last Name" );
                Assert.AreEqual( fname, FirstName, "First Name" );
                Assert.AreEqual( age, Age, "Age" );
                Assert.AreEqual( height, Height, "Height" );
            }
        }


        [TestMethod]
        [TestCategory( "CsvImporter" )]
        public void SimpleDataTest()
        {
            var sb = new StringBuilder( "FirstName, Last   Name, Age, Height" ).AppendLine();
            sb.Append( "Homer,   \"Simpson\"  , 41, 5.6" ).AppendLine();
            sb.Append( "\"Lisa Anne\", Simpson, 7, 3.9" ).AppendLine();
            sb.Append( "     " );

            var csv = new CsvImporter
            {
                ColumnNameSpaceReplacement = null
            };
            csv.ImportString( sb.ToString() );

            Assert.AreEqual( 2, csv.GetData().Count, "Should be 2 lines of data" );

            var list = csv.GetData<CTestPerson>().ToList();
            Assert.AreEqual( 2, list.Count, "List count should also be 2" );

            list[0].TestPerson( "Homer", "Simpson", 41, 5.6 );
            list[1].TestPerson( "Lisa Anne", "Simpson", 7, 3.9 );
        }

        [TestMethod]
        [TestCategory( "CsvImporter" )]
        public void BlankLinesTest()
        {
            var sb = new StringBuilder( "FirstName, Last   Name, Age, Height" ).AppendLine();
            sb.AppendLine();
            sb.Append( "Homer,   \"Simpson\"  , 41, 5.6" ).AppendLine();
            sb.Append( "          " ).AppendLine();
            sb.AppendLine();
            sb.Append( "\"Lisa Anne\", Simpson, 7, 3.9" ).AppendLine();
            sb.Append( "          " ).AppendLine();
            sb.Append( "          " ).AppendLine();
            sb.AppendLine();
            sb.Append( "          " ).AppendLine();
            sb.AppendLine();
            sb.Append( "     " );

            var csv = new CsvImporter
            {
                ColumnNameSpaceReplacement = null
            };
            csv.ImportString( sb.ToString() );

            Assert.AreEqual( 2, csv.GetData().Count, "Should be 2 lines of data" );

            var list = csv.GetData<CTestPerson>().ToList();
            Assert.AreEqual( 2, list.Count, "List count should also be 2" );

            list[0].TestPerson( "Homer", "Simpson", 41, 5.6 );
            list[1].TestPerson( "Lisa Anne", "Simpson", 7, 3.9 );
        }



        [TestMethod]
        [TestCategory( "CsvImporter" )]
        public void AliasTest1()
        {
            var sb = new StringBuilder( "Given Name, Family Name, Age, Height" ).AppendLine();
            sb.Append( "Homer,   \"Simpson\"  , 41, 5.6" ).AppendLine();
            sb.Append( "\"Lisa Anne\", Simpson, 7, 3.9" ).AppendLine();

            var aliases = new Dictionary<string, string>
            {
                ["Given Name"] = "FirstName",
                ["Family Name"] = "LastName"
            };

            var csv = new CsvImporter
            {
                ColumnNameSpaceReplacement = " "
            };
            csv.ImportString( sb.ToString() );

            Assert.AreEqual( 2, csv.GetData().Count, "Should be 2 lines of data" );

            var list = csv.GetData<CTestPerson>( aliases ).ToList();
            Assert.AreEqual( 2, list.Count, "List count should also be 2" );

            list[0].TestPerson( "Homer", "Simpson", 41, 5.6 );
            list[1].TestPerson( "Lisa Anne", "Simpson", 7, 3.9 );
        }

        [TestMethod]
        [TestCategory( "CsvImporter" )]
        public void UnknownColumnNameTest()
        {
            var invalidColumnFound = false;
            var invalidColumnCount = 0;

            var sb = new StringBuilder( "FirstName, Last_Name, Age, Height" ).AppendLine();
            sb.Append( "Homer,   \"Simpson\"  , 41, 5.6" ).AppendLine();
            sb.Append( "\"Lisa Anne\", Simpson, 7, 3.9" ).AppendLine();
            sb.Append( "     " );

            var csv = new CsvImporter
            {
                ColumnNameSpaceReplacement = null
            };
            csv.OnUnusableColumn += _colname =>
                {
                    invalidColumnCount++;
                    if (_colname == "Last_Name")
                        invalidColumnFound = true;
                };

            csv.ImportString( sb.ToString() );
            csv.GetData<CTestPerson>().ToList();

            Assert.AreEqual( 1, invalidColumnCount, "Invalid column count wrong" );
            Assert.IsTrue( invalidColumnFound, "Didn't find the invalid column" );
        }


        [TestMethod]
        [TestCategory( "CsvImporter" )]
        public void FileIoTest()
        {
            var csv = new CsvImporter();
            csv.ImportFile( "UnitTestCsvFileIo.csv" );
            var list = csv.GetData<CTestPerson>().ToList();

            Assert.AreEqual( 2, list.Count, "Number of data elements is incorrect" );
            list[0].TestPerson( "Homer", "Simpson", 41, 0 );
            list[1].TestPerson( "Bart", "Simpson", 6, 0 );
        }

        [TestMethod]
        [TestCategory( "CsvImporter" )]
        public void PreProcessLineTest()
        {
            var linesRead = 0;

            var csv = new CsvImporter();
            csv.OnLineRead += _sb =>
                {
                    linesRead++;
                    for (var i = 0; i < _sb.Length; i++)
                    {
                        _sb[i] = char.ToUpper( _sb[i] );
                    }
                };
            csv.ImportFile( "UnitTestCsvFileIo.csv" );
            var list = csv.GetData<CTestPerson>().ToList();

            Assert.AreEqual( 2, list.Count, "Number of data elements is incorrect" );
            list[0].TestPerson( "HOMER", "SIMPSON", 41, 0 );
            list[1].TestPerson( "BART", "SIMPSON", 6, 0 );
            Assert.AreEqual( 7, linesRead, "Number of lines preprocessed, including blank lines" );
        }




    }
}
