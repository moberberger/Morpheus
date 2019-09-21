using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

#pragma warning disable 169
using Morpheus.PerformanceTests;
using Morpheus;

namespace Reflection.MethodInvocation
{
    internal class InvokeMethodInvoker : InvokeMethodBase
    {
        private Func<object, object[], object> m_invoker;

        public InvokeMethodInvoker()
        {
            //OkToRun = true;
            TestName = "Invoking a method using a MethodProxy' Invoker directly";

            var mi = GetType().GetMethod( "SomeOperation" );

            m_invoker = MethodProxy.CreateInvoker( mi );
        }

        public override void RunTest()
        {
            var parms = new object[1];
            // The default test does nothing but give a baseline of how much the overhead costs.
            while (!StopRunning)
            {
                parms[0] = Iterations;
                var x = (long) m_invoker( this, parms );

                if (Iterations != x) throw new InvalidOperationException();
                System.Threading.Interlocked.Increment( ref Iterations );
            }
        }
    }
    internal class InvokeMethodProxy : InvokeMethodBase
    {
        private MethodProxy m_proxy;

        public InvokeMethodProxy()
        {
            //OkToRun = true;
            TestName = "Invoking a method using a MethodProxy";

            var mi = GetType().GetMethod( "SomeOperation" );

            m_proxy = new MethodProxy( mi );
        }

        public override void RunTest()
        {
            var parms = new object[1];
            // The default test does nothing but give a baseline of how much the overhead costs.
            while (!StopRunning)
            {
                parms[0] = Iterations;
                var x = (long) m_proxy.Invoke( this, parms );

                if (Iterations != x) throw new InvalidOperationException();
                System.Threading.Interlocked.Increment( ref Iterations );
            }
        }
    }
}