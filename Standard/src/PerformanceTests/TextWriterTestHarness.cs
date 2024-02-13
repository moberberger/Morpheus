#nullable disable

namespace Morpheus.PerformanceTests;


/// <summary>
/// A harness for running performance tests in a basically single-threaded environment.
/// </summary>
/// <remarks>Please see the class <see cref="TestRunner"/> for examples.</remarks>
public class TextWriterTestHarness
{
    private readonly TextWriter m_output;
    private readonly TestRunner m_testRunner;
    private int m_msBetweenNotifications;

    /// <summary>
    /// Construct this harness with a <see cref="TestRunner"/> and something to route the
    /// output to.
    /// </summary>
    /// <param name="_testRunner">The <see cref="TestRunner"/> to run</param>
    /// <param name="_output">
    /// The output stream. If null, <see cref="Console.Out"/> will be used.
    /// </param>
    public TextWriterTestHarness( TestRunner _testRunner, TextWriter _output )
    {
        m_output = _output ?? Console.Out;
        m_testRunner = _testRunner;
    }

    /// <summary>
    /// Run the tests in the configured <see cref="TestRunner"/> .
    /// </summary>
    /// <param name="_secondsToRun">How long to run tests for</param>
    /// <param name="_msBetweenNotifications">
    /// The number of milliseconds between notifications to the application
    /// </param>
    public void RunTests( double _secondsToRun, int _msBetweenNotifications = 200 )
    {
        m_testRunner.OnTestStart += OnTestStart;
        m_testRunner.OnTestComplete += OnTestComplete;
        m_testRunner.OnException += OnTestException;
        m_msBetweenNotifications = _msBetweenNotifications;

        m_testRunner.RunTests( _secondsToRun );

        var runnableTests = m_testRunner.Tests.Where( _t => _t.OkToRun );
        foreach (var test in runnableTests)
        {
            var status = new TestStatus( test );
            m_output.WriteLine( $"{status} - {test.TestName}" );
        }
        foreach (var test in runnableTests)
            m_output.WriteLine( $"This Test Not Run - {test.TestName}" );
    }

    private void OnTestStart( TestBase _test )
    {
        m_output.WriteLine( $"Test '{_test.TestName}' Starting" );

        var timer = new Timer( ReportProgress, _test, m_msBetweenNotifications, m_msBetweenNotifications );
        _test.Context = timer;
    }

    private void OnTestComplete( TestBase _test )
    {
        ReportProgress( _test );
        m_output.WriteLine( $"\nTest '{_test.TestName}' Complete.\n" );

        var timer = _test.Context as Timer;
        timer.Dispose();
    }

    private void OnTestException( TestBase _test, Exception _ex ) => throw _ex;

    /// <summary>
    /// Helper function to report the progress to the output stream.
    /// </summary>
    /// <param name="_param"></param>
    public void ReportProgress( object _param )
    {
        var test = _param as TestBase;
        var status = new TestStatus( test );
        m_output.Write( $"\t{status}               \r" );
    }
}
