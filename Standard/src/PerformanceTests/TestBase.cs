using System;

namespace Morpheus.PerformanceTests
{
    /// <summary>
    /// Harness for the tests.
    /// </summary>
    /// <remarks>
    /// <para> Tests are initialized on the main thread, but run on their own thread. </para>
    /// 
    /// <para> All immutable test setup should be done in the constructor. This should be data
    /// stored in this specific (presumably) base-class object (e.g. TestName, Order, etc). No
    /// test-specific data should be set in the constructor. </para>
    /// 
    /// <para> All setup to configure the object for the test should be done in "Initialize()".
    /// This should include any setting of test-specific properties/fields. </para>
    /// 
    /// <para> Any cleanup should be done in the CleanUp() override. </para>
    /// 
    /// <para> Please see the class <see cref="TestRunner"/> for examples. </para>
    /// </remarks>
    public class TestBase
    {
        #region Configuration

        /// <summary>
        /// Should be set by the constructor to match the name of this test, for output and
        /// descriptive purposes
        /// </summary>
        public string TestName { get; protected set; } = "base class";

        /// <summary>
        /// Is the test OK to run. Derived classes set this to FALSE when the test shouldn't
        /// run. This tells the framework whether or not to run a test, not whether or not the
        /// test is completed or should stop running.
        /// </summary>
        public bool OkToRun { get; set; } = false;

        /// <summary>
        /// The RunOrder is used to sort all tests found in a DLL.
        /// </summary>
        public int RunOrder { get; protected set; } = int.MinValue;

        /// <summary>
        /// App defined context that may be assigned at will to this test
        /// </summary>
        public object Context { get; set; } = null;

        /// <summary>
        /// Used to keep old Spike Tests around while new spikes are developed.
        /// </summary>
        public string Frame { get; set; } = null;

        #endregion


        #region Run Status

        /// <summary>
        /// The number of iterations that the test has run. Should be modified by using
        /// Interlocked.Increment(ref Iterations).
        /// </summary>
        public long Iterations = 0;

        /// <summary>
        /// The framework will set this to TRUE when the test is supposed to terminate.
        /// </summary>
        public bool StopRunning = false;

        /// <summary>
        /// If an exception is thrown while accessing this harness, it will be stored here as
        /// well as passed into the OnException event
        /// </summary>
        public Exception Exception;

        /// <summary>
        /// The time that the test started running
        /// </summary>
        public DateTime StartTime { get; internal set; }

        /// <summary>
        /// When the test was started, this is when the test was expected to end
        /// </summary>
        public DateTime ExpectedEndTime { get; internal set; }

        /// <summary>
        /// Due to overheads and imprecision, this is when the test actually ended.
        /// </summary>
        public DateTime EndTime { get; internal set; }

        #endregion


        #region Functionality

        /// <summary>
        /// This is called by the framework before the test starts. The test should establish
        /// any test-specific data here. Doing so helps with Exception processing, as exceptions
        /// thrown in constructors when Activator.CreateInstance is used is harder to debug.
        /// </summary>
        /// <remarks>
        /// Constructor- Stuff that really should be impervious to exception throwing.
        /// Initialize- All Test-specific initialization, including anything that may throw
        /// exceptions. Cleanup- Any cleanup that should happen outside the timing of the tests.
        /// Do NOT clean up in RunTests()!
        /// </remarks>
        public virtual void Initialize() { }

        /// <summary>
        /// Called by the framework.
        /// </summary>
        /// <remarks>
        /// This routine should contain loops or other constructs. Each iteration of the loop
        /// should increment the "Iterations" field by calling Interlocked.Increment(ref
        /// Iterations). This is the lowest overhead oepration I can think of to record an
        /// iteration of the test.
        /// 
        /// The test should stop looping when it notices the "StopRunning" bool is TRUE
        /// 
        /// The contents of this method should be copy-pasted to form the basis of application
        /// tests in order to make sure those tests are as close to "baseline" as possible
        /// </remarks>
        public virtual void RunTest()
        {
            // The default test does nothing but give a baseline of how much the overhead costs.
            while (!StopRunning)
            {
                System.Threading.Interlocked.Increment( ref Iterations );
            }
        }

        /// <summary>
        /// Is this test in the specified Frame? Used to tightly filter tests.
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public bool InFrame( string frame )
        {
            if (frame == null)
                return true;
            if (frame == "") // only tests WITHOUT a frame
                return string.IsNullOrEmpty( Frame );
            if (frame == Frame)
                return true;
            return false;
        }

        /// <summary>
        /// Called by the framework to make sure all parts of the test that are not to be
        /// included in timing are cleaned up or finalized. This routine's run-time IS NOT
        /// MEASURED.
        /// </summary>
        public virtual void CleanUp() { }

        #endregion
    }


}
