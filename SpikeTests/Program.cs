#nullable enable

using Morpheus.PerformanceTests;

namespace Morpheus;


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


    public static void TestShuffle()
    {
        int[] buf = new int[4];
        var counts = new Dictionary<int, int>();

        for (int i = 0; i < 10000000; i++)
        {
            buf[0] = 1;
            buf[1] = 2;
            buf[2] = 3;
            buf[3] = 4;

            buf.Shuffle();

            int x = buf[0] + buf[1] * 10 + buf[2] * 100 + buf[3] * 1000;
            counts.TryGetValue( x, out int count );
            count++;
            counts[x] = count;
        }
        Console.WriteLine( counts.OrderBy( x => x.Value ).Select( kv => $"{kv.Key} = {kv.Value}" ).JoinAsString( "\n" ) );
    }

    /// <summary>
    /// Entry point for Console use
    /// </summary>
    /// <param name="args">Unused</param>
    public static void Main( string[] args )
    {
        //RunTests(); return;
        //TestShuffle();
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
