using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;


#pragma warning disable 169
using Morpheus.PerformanceTests;
using Morpheus;

namespace Reflection.RDTSCTiming
{
    internal class RngSeed_Robust_Timing : TestBase
    {
        public RngSeed_Robust_Timing()
        {
            OkToRun = false;
            TestName = "RngSeed.Robust() Timing Test";
            RandomSeed.RDTSC8();
        }

        public override void RunTest()
        {
            // The default test does nothing but give a baseline of how much the overhead costs.
            while (!StopRunning)
            {
                var x = RandomSeed.Robust();
                System.Threading.Interlocked.Increment( ref Iterations );
            }
        }
    }
}
