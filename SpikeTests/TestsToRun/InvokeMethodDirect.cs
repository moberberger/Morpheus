using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

#pragma warning disable 169
using Morpheus.PerformanceTests;

namespace Reflection.MethodInvocation
{
    internal class InvokeMethodDirect : InvokeMethodBase
    {
        public InvokeMethodDirect()
        {
            //OkToRun = true;
            TestName = "Invoking a method directly- Baseline";
        }

        public override void RunTest()
        {
            // The default test does nothing but give a baseline of how much the overhead costs.
            while (!StopRunning)
            {
                var x = SomeOperation( Iterations );
                if (Iterations != x) throw new InvalidOperationException();
                System.Threading.Interlocked.Increment( ref Iterations );
            }
        }
    }
}
