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


            var buf = new byte[1 << 17];
            buf.FromIntegers( () => (uint) RandomSeed.Robust() );

            // var mem = new MemoryStream(); var writer = new BinaryWriter( mem );

            // for (int i = 0; i 1024 * 256; i++) {

            // }


            using (var ofp = File.Create( @"D:\Temp\Robust.bin" ))
                ofp.Write( buf, 0, buf.Length );
            return;
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

        static void Test()
        {
            using (var ifp = File.OpenRead( @"D:\\Temp\\TimerSeed.bin" ))
            {
                var array = new byte[ifp.Length];
                ifp.Read( array, 0, array.Length );

                var rngArray = new byte[array.Length];
                using (var rng = new Morpheus.MersenneTwister())
                    rng.NextBytes( rngArray );

                for (int i = 0; i < array.Length; i++)
                {
                    array[i] ^= rngArray[i];
                }

                using (var ofp = File.OpenWrite( @"D:\\Temp\\TimerRng.bin" ))
                    ofp.Write( array, 0, array.Length );
            }
        }

        static void FoldWithRandomNumberGenerator()
        {
            using (var ifp = File.OpenRead( @"D:\\Temp\\TimerSeed.bin" ))
            {
                var array = new byte[ifp.Length];
                ifp.Read( array, 0, array.Length );

                var rngArray = new byte[array.Length];
                using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
                    rng.GetBytes( rngArray );

                for (int i = 0; i < array.Length; i++)
                {
                    array[i] ^= rngArray[i];
                }

                using (var ofp = File.OpenWrite( @"D:\\Temp\\TimerRng.bin" ))
                    ofp.Write( array, 0, array.Length );
            }
        }


        static void CreateTimerSeedData()
        {
            const int SIZE = 200_000;

            var last = Stopwatch.GetTimestamp();
            var array = new byte[SIZE];

            for (int i = -1; i < array.Length; i++)
            {
                var counter = Stopwatch.GetTimestamp();

                if (i >= 0)
                {
                    var delta = counter - last;
                    array[i] = (byte) (delta & 0xff);
                }

                last = counter;
                Thread.Sleep( 1 );
                if (i % 1000 == 0) Console.WriteLine( i );
            }

            using (var ofp = File.Create( @"D:\\Temp\\TimerSeed.bin" ))
                ofp.Write( array, 0, array.Length );
        }
    }
}
