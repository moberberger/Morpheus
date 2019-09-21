using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

#pragma warning disable 169
using Morpheus.PerformanceTests;

namespace Reflection.MethodInvocation
{
    internal class InvokeMethodInfo : InvokeMethodBase
    {
        private readonly MethodInfo m_method;
        private readonly object[] m_args = new object[1];

        public InvokeMethodInfo()
        {
            //OkToRun = true;
            TestName = "Invoking a method using MethodInfo";

            m_method = GetType().GetMethod( "SomeOperation" );
            if (m_method == null) throw new InvalidOperationException( "Cannot get MethodInfo" );
        }

        public override void RunTest()
        {
            // The default test does nothing but give a baseline of how much the overhead costs.
            while (!StopRunning)
            {
                m_args[0] = Iterations;
                var x = (long) m_method.Invoke( this, m_args );
                if (Iterations != x) throw new InvalidOperationException();
                System.Threading.Interlocked.Increment( ref Iterations );
            }
        }
    }
}