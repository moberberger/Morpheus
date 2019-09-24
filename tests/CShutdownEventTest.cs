using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;




namespace Morpheus.Standard.UnitTests
{
    [TestClass]
    public class CShutdownEventTest
    {
        [TestMethod]
        public void ShutdownEventTest()
        {
            var e = new ShutdownEvent();
            Assert.IsFalse( e.IsShutdown, "Should not be shutting down on construction" );

            var wh = (WaitHandle) e;
            var signaled = wh.WaitOne( 0, true );
            Assert.IsFalse( signaled, "The wait handle should not have been signaled" );
        }

        [TestMethod]
        public void ShutdownEventSignalledTest()
        {
            var e = new ShutdownEvent();
            e.Shutdown();

            Assert.IsTrue( e.IsShutdown, "Should be shutting down after being instructed to" );

            var mre = (ManualResetEvent) e;
            var signaled = mre.WaitOne( 0, true );
            Assert.IsTrue( signaled, "The wait handle should have been signaled" );

            e.WaitForShutdown();
        }


    }
}
