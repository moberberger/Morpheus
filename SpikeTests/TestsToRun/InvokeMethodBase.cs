using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

#pragma warning disable 169
using TestBase = Morpheus.PerformanceTests.TestBase;

namespace Reflection.MethodInvocation
{
    internal abstract class InvokeMethodBase : TestBase
    {
        public long SomeOperation( long val )
        {
            var x = val;
            long accum = 0;
            for (var i = 0; i < 64; i++)
            {
                accum ^= (x & 1);
                accum <<= 1;
                x >>= 1;
            }
            accum ^= val;
            val ^= accum;
            accum ^= val;

            return accum;
        }
    }
}