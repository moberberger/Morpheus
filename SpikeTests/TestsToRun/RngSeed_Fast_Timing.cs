﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;


#pragma warning disable 169
using Morpheus.PerformanceTests;
using Morpheus;

namespace Reflection.RDTSCTiming
{
    internal class RngSeed_Fast_Timing : TestBase
    {
        public RngSeed_Fast_Timing()
        {
            OkToRun = false;
            TestName = "RandomSeed.Fast() Timing Test";
            RandomSeed.RDTSC8();
        }

        public override void RunTest()
        {
            // The default test does nothing but give a baseline of how much the overhead costs.
            while (!StopRunning)
            {
                var x = RandomSeed.Fast();
                System.Threading.Interlocked.Increment( ref Iterations );
            }
        }
    }

    internal class RngSeed_Medium_Timing : TestBase
    {
        public RngSeed_Medium_Timing()
        {
            OkToRun = false;
            TestName = "RandomSeed.Medium() Timing Test";
            RandomSeed.RDTSC8();
        }

        public override void RunTest()
        {
            // The default test does nothing but give a baseline of how much the overhead costs.
            while (!StopRunning)
            {
                var x = RandomSeed.Medium();
                System.Threading.Interlocked.Increment( ref Iterations );
            }
        }
    }
}
