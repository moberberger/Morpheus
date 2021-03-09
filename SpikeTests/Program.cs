using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Morpheus.PerformanceTests;
using System.Linq;
using System.IO;

namespace Morpheus
{
    public class Program
    {
        public const double SECONDS_TO_RUN = 10;
        public const int MS_BETWEEN_UPDATES = 100;

        public class TestBaseline : TestBase
        {
            public TestBaseline()
            {
                // Use to turn on/off this test.
                OkToRun = true;
                TestName = "Baseline Test- No Operation within loop";
            }
        };


        /// <summary>
        /// Entry point for Console use
        /// </summary>
        /// <param name="args">Unused</param>
        public static void Main( string[] args )
        {
            //RunTests(); return;

            Console.WriteLine( $"hi" );
        }


        static void RunTests()
        {
            Console.WriteLine( $"Running tests in {CompileMode} mode." );

            // var tester = new TestRunner( new TestBase[] { new TestBaseline(), new
            // TestRandom1(), new TestRandom2() } );
            var tester = new TestRunner();
            var harness = new TextWriterTestHarness( tester, Console.Out );

            harness.RunTests( SECONDS_TO_RUN, MS_BETWEEN_UPDATES );

            Console.WriteLine( $"Done running tests in {CompileMode} mode." );
        }

#if DEBUG
        public const string CompileMode = "DEBUG";
#else
        public const string CompileMode = "RELEASE";
#endif

    }
}
