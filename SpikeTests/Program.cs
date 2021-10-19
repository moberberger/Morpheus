using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Morpheus.PerformanceTests;
using System.Linq;
using System.IO;
using System.Runtime.CompilerServices;

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


        public class TestInlining
        {
            public const ulong Multiplier = 6364136223846793005UL;
            public const ulong Increment = 1442695040888963407UL;
            public ulong State;
            public double Sum;


            [MethodImpl( MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization )]
            public static ulong Next( ulong state ) => state * Multiplier + Increment;

            [MethodImpl( MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization )]
            public static long Next( long state ) => (long)((ulong)state * Multiplier + Increment);

            [MethodImpl( MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization )]
            public ulong Next() => State = Next( State );

            [MethodImpl( MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization )]
            public virtual double NextDouble() => Next() / (1.0 + uint.MaxValue);

            [MethodImpl( MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization )]
            public double Add( double x ) => Sum += Next() * x;

        }

        /// <summary>
        /// Entry point for Console use
        /// </summary>
        /// <param name="args">Unused</param>
        public static void Main( string[] args )
        {
            //RunTests(); return;

            double sum = 0;
            long state = DateTime.Now.Ticks;

            var rng = new TestInlining();

            for (int i = 0; i < 1000000; i++)
            {
                var smp = rng.NextDouble();
                sum += rng.Add( smp );
            }


            Console.WriteLine( $"hi {sum - rng.Sum}" );
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
