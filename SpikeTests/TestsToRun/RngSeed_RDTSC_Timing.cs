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
    internal class RDTSCTiming : TestBase
    {
        public RDTSCTiming()
        {
            OkToRun = false;
            TestName = "RngSeed.RDTSC Timing Test";
            RandomSeed.RDTSC8();
        }

        public override void RunTest()
        {
            // The default test does nothing but give a baseline of how much the overhead costs.
            while (!StopRunning)
            {
                var x = RandomSeed.RDTSC;
                System.Threading.Interlocked.Increment( ref Iterations );
            }
        }
    }
}
