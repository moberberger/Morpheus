using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#pragma warning disable 169

namespace Morpheus.PerformanceTests
{
    /// <summary>
    /// The TestRunner class is a framework for running performance tests over very fast
    /// operations. It is focused on operations whose time is measured in microseconds or
    /// nanoseconds.
    /// </summary>
    /// <remarks>
    /// <para> The bare minimal baseline test looks like this: </para>
    /// <code>
    /// public class TestBaseline : TestBase
    /// {
    ///     public TestBaseline()
    ///     {
    ///         OkToRun = true;
    ///         TestName = "Baseline Test- No Operation within loop"; // not required, but a good idea
    ///     }
    /// 
    ///     public override void RunTest()
    ///     {
    ///         while (!StopRunning)
    ///         {
    ///             System.Threading.Interlocked.Increment( ref Iterations );
    ///         }
    ///     }
    /// };
    /// </code>
    /// <para> This is the format of a user-defined test. </para>
    /// <code>
    /// public class TestRandom : TestBase
    /// {
    ///     Random rng;
    /// 
    ///     public TestRandom()
    ///     {
    ///         OkToRun = false;
    ///         TestName = "Test Random x1";
    ///         rng = new Random();
    ///     }
    /// 
    ///     public override void RunTest()
    ///     {
    ///         while (!StopRunning)
    ///         {
    ///             var x = rng.Next();
    ///             var y = rng.NextDouble();
    ///             var z = x * y;
    /// 
    ///             System.Threading.Interlocked.Increment( ref Iterations );
    ///         }
    ///     }
    /// }
    /// </code>
    /// <para> Instantiating the runner and running it using the Console.Out test harness is as
    /// simple as this: </para>
    /// <code>
    /// var tester = new TestRunner( new TestBase[] { new TestBaseline(), new TestRandom() } );
    /// var harness = new TextWriterTestHarness( tester, Console.Out ); // Console.Out is optional
    ///
    /// harness.RunTests( SECONDS_TO_RUN, MS_BETWEEN_UPDATES );
    /// </code>
    /// <para> The following shortcut will run ALL tests found in the calling
    /// <see cref="Assembly"/> : </para>
    /// <code>
    /// var tester = new TestRunner(); // no params => All subclasses of TestBase in calling assembly
    /// var harness = new TextWriterTestHarness( tester ); // Auto-Console-Out
    ///
    /// harness.RunTests( SECONDS_TO_RUN, MS_BETWEEN_UPDATES );
    /// </code>
    /// </remarks>
    public class TestRunner
    {
        /// <summary>
        /// Fired when a test is about to start
        /// </summary>
        public event Action<TestBase> OnTestStart;

        /// <summary>
        /// Fired after a test has been completed.
        /// </summary>
        public event Action<TestBase> OnTestComplete;

        /// <summary>
        /// Fired when an exception has occurred in any of the app-defined event handlers
        /// </summary>
        public event Action<TestBase, Exception> OnException;

        /// <summary>
        /// The tests that will be or were run.
        /// </summary>
        public IEnumerable<TestBase> Tests { get; }

        /// <summary>
        /// Run all tests found in the specified Assembly, or optionally in the calling Assembly
        /// (pass NULL)
        /// </summary>
        /// <param name="_assembly">
        /// The assembly to search for classes deriving from TestBase, or NULL to search the
        /// calling assembly
        /// </param>
        public TestRunner( Assembly _assembly = null )
        {
            // Must check for null here as well- If this check is relegated to the DiscoverTests
            // method (which also does this check), it will only find the BaseTest found in
            // Morpheus.
            Tests = DiscoverTests( _assembly ?? Assembly.GetCallingAssembly() ).ToList();
        }

        /// <summary>
        /// Use a pre-set enumeration of tests to run
        /// </summary>
        /// <param name="_tests">The tests to run</param>
        public TestRunner( IEnumerable<TestBase> _tests )
        {
            Tests = _tests.ToList();
        }






        /// <summary>
        /// Run all of the tests configured for this runner in the constructor
        /// </summary>
        /// <param name="secondsToRun">How long the tests should be run for</param>
        /// <param name="frame">
        /// If specified (non-null), ONLY run tests in the specified frame
        /// </param>
        public void RunTests( double secondsToRun, string frame = null )
        {
            foreach (var test in Tests)
            {
                if (test.OkToRun && test.InFrame( frame ))
                    RunTest( test, secondsToRun );
            }
        }

        /// <summary>
        /// Run a specific test, regardless of its OkToRun flag
        /// </summary>
        /// <param name="_test">
        /// The test to run, regardless of its <see cref="TestBase.OkToRun"/> flag
        /// </param>
        /// <param name="_secondsToRun">How long the tests should be run for</param>
        public void RunTest( TestBase _test, double _secondsToRun )
        {
            using (var timer = new Timer( _x => _test.StopRunning = true, null, (int)(1000 * _secondsToRun), 0 ))
            {
                InitTest( _test );

                // System.Diagnostics.StopWatch doesn't seem to be a part of .NET Standard, so
                // I'm using DateTime because this time measurement is not required to be
                // precise, as its a summary timing of hopefully millions or billions of tiny
                // operations (each of which, were it the interesting part, would be a precise
                // operation requiring StopWatch)
                _test.StartTime = DateTime.Now;
                _test.ExpectedEndTime = _test.StartTime + TimeSpan.FromSeconds( _secondsToRun );
                _test.EndTime = DateTime.MinValue;

                RunTestMethod( _test );
                _test.EndTime = DateTime.Now;

                CleanUpTest( _test );
            }
        }

        private void RunTestMethod( TestBase _test )
        {
            try
            {
                _test.RunTest();
            }
            catch (Exception ex)
            {
                _test.Exception = ex;
                OnException?.Invoke( _test, ex );
                throw ex;
            }
        }

        private void InitTest( TestBase _test )
        {
            OnTestStart?.Invoke( _test );

            try
            {
                _test.Initialize();
                _test.Iterations = 0;
            }
            catch (Exception ex)
            {
                _test.Exception = ex;
                OnException?.Invoke( _test, ex );
                throw ex;
            }
        }

        private void CleanUpTest( TestBase _test )
        {
            try
            {
                _test.CleanUp();
            }
            catch (Exception ex)
            {
                _test.Exception = ex;
                OnException?.Invoke( _test, ex );
                throw ex;
            }

            OnTestComplete?.Invoke( _test );
        }






        /// <summary>
        /// Helper to discover all tests found in the specified <see cref="Assembly"/> .
        /// </summary>
        /// <param name="_assembly">
        /// The <see cref="Assembly"/> to search for classes that are subclasses of
        /// <see cref="TestBase"/> . If NULL, then this will search the calling Assembly.
        /// </param>
        /// <returns>
        /// An enumeration of all <see cref="Type"/> s that are subclasses of
        /// <see cref="TestBase"/> .
        /// </returns>
        public static IEnumerable<TestBase> DiscoverTests( Assembly _assembly = null )
        {
            _assembly = _assembly ?? Assembly.GetCallingAssembly();
            return _assembly
                .GetTypes()
                .Where( _t => !_t.IsAbstract )
                .Where( _t => typeof( TestBase ).IsAssignableFrom( _t ) )
                .Select( _t => Activator.CreateInstance( _t ) as TestBase )
                .OrderBy( _test => _test.OkToRun )
                .OrderBy( _test => _test.RunOrder );
        }
    }
}
