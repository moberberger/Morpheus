using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;


#pragma warning disable 169
using Morpheus.PerformanceTests;
using Morpheus;

namespace Reflection.PrimitiveExtensions
{
    internal class PrimitiveExtensions_NoExtention : TestBase
    {
        public PrimitiveExtensions_NoExtention()
        {
            OkToRun = true;
            TestName = "PrimitiveExtensions - No Extension";
        }

        public override void RunTest()
        {
            // The default test does nothing but give a baseline of how much the overhead costs.
            double x = 1, y = 2;
            while (!StopRunning)
            {
                Order( ref x, ref y );
                x += 1.0;
                System.Threading.Interlocked.Increment( ref Iterations );
            }
        }

        public static void Order( ref double x, ref double y )
        {
            if (x > y)
            {
                var tmp = x;
                x = y;
                y = tmp;
            }
        }
    }

    internal static class OrderStatic
    {
        public static double Order( this double x, ref double y )
        {
            if (x > y)
            {
                var tmp = y;
                y = x;
                return tmp;
            }
            return x;
        }
    }

    internal class PrimitiveExtensions_ExtensionNotUsed : TestBase
    {
        public PrimitiveExtensions_ExtensionNotUsed()
        {
            OkToRun = true;
            TestName = "PrimitiveExtensions - Extension Not Used";
        }

        public override void RunTest()
        {
            // The default test does nothing but give a baseline of how much the overhead costs.
            double x = 1, y = 2;
            while (!StopRunning)
            {
                x = OrderStatic.Order( x, ref y );
                x += 1.0;
                System.Threading.Interlocked.Increment( ref Iterations );
            }
        }
    }


    internal class PrimitiveExtensions_WithExtension : TestBase
    {
        public PrimitiveExtensions_WithExtension()
        {
            OkToRun = true;
            TestName = "PrimitiveExtensions - With Extension";
        }

        public override void RunTest()
        {
            // The default test does nothing but give a baseline of how much the overhead costs.
            double x = 1, y = 2;
            while (!StopRunning)
            {
                x = x.Order( ref y );
                x += 1.0;
                System.Threading.Interlocked.Increment( ref Iterations );
            }
        }
    }

}
