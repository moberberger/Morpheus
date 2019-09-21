using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

#pragma warning disable 169
using Morpheus.PerformanceTests;

namespace Reflection.MethodInvocation
{
    internal class InvokeMethodLambda : InvokeMethodBase
    {
        private readonly Func<long, long> m_lambda;

        public InvokeMethodLambda()
        {
            //OkToRun = false;
            TestName = "Invoking a method using Lambda";
            m_lambda = SomeOperation;
        }

        public override void RunTest()
        {
            // The default test does nothing but give a baseline of how much the overhead costs.
            while (!StopRunning)
            {
                var x = m_lambda( Iterations );
                if (Iterations != x) throw new InvalidOperationException();
                System.Threading.Interlocked.Increment( ref Iterations );
            }
        }
    }
}