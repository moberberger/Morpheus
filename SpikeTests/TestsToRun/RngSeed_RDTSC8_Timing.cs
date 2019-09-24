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
    internal class RngSeed_RDTSC8_Timing : TestBase
    {
        public RngSeed_RDTSC8_Timing()
        {
            OkToRun = false;
            TestName = "RngSeed.RDTSC8() Timing Test";
            RandomSeed.RDTSC8();
        }

        public override void RunTest()
        {
            // The default test does nothing but give a baseline of how much the overhead costs.
            while (!StopRunning)
            {
                var x = RandomSeed.RDTSC8();
                System.Threading.Interlocked.Increment( ref Iterations );
            }
        }
    }
}
