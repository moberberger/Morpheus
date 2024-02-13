#nullable disable

namespace Morpheus.PerformanceTests;


/// <summary>
/// This class holds the statistics for the performance tests.
/// </summary>
/// <remarks>Please see the class <see cref="TestRunner"/> for examples.</remarks>
public class TestStatus
{
    /// <summary>
    /// The <see cref="TestBase"/> used for this performance data
    /// </summary>
    public readonly TestBase Test;

    /// <summary>
    /// The time that these statistics were created.
    /// </summary>
    public readonly DateTime Now;

    /// <summary>
    /// When the test started
    /// </summary>
    public readonly DateTime StartTime;

    /// <summary>
    /// An approximation of when the test will end. Calculated at the beginning of the test
    /// with the expected duration of the test.
    /// </summary>
    public readonly DateTime ExpectedEndTime;

    /// <summary>
    /// When the test actually ended
    /// </summary>
    public readonly DateTime EndTime;

    /// <summary>
    /// The number of loop iterations which occurred for the test.
    /// </summary>
    public readonly long Iterations;

    /// <summary>
    /// Set when the loop exits and the test is done.
    /// </summary>
    public readonly bool IsDone;

    /// <summary>
    /// The amount of time that the test ran for
    /// </summary>
    public readonly TimeSpan RunTime;

    /// <summary>
    /// The amount of time that the test ran for, in seconds
    /// </summary>
    public readonly double RunTimeInSeconds;

    /// <summary>
    /// The number of seconds for each iteration of the loop.
    /// </summary>
    public readonly double SecondsPerIteration;

    /// <summary>
    /// 
    /// </summary>
    public readonly double CompletionRatio;

    /// <summary>
    /// Construct from a <see cref="TestBase"/> object
    /// </summary>
    /// <param name="_test"></param>
    public TestStatus( TestBase _test )
    {
        Now = DateTime.Now;
        StartTime = _test.StartTime;
        ExpectedEndTime = _test.ExpectedEndTime;
        EndTime = _test.EndTime;
        Iterations = _test.Iterations;

        if (EndTime > StartTime)
        {
            IsDone = true;
            if (Now > EndTime)
                Now = EndTime;
        }

        RunTime = Now - StartTime;
        RunTimeInSeconds = RunTime.TotalSeconds;
        SecondsPerIteration = RunTimeInSeconds / Iterations;
        CompletionRatio = (RunTimeInSeconds / (ExpectedEndTime - StartTime).TotalSeconds).Clamp( 0.0, 1.0 );
    }


    /// <summary>
    /// Conver to a string
    /// </summary>
    /// <returns>String version of these data</returns>
    public override string ToString() => $"Count: {Iterations:N0}   Each: {SecondsPerIteration.MakeMetricString( "s" )}  Complete: {CompletionRatio * 100:N2}%";
};
