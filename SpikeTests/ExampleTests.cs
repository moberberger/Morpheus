using System;
using System.Collections.Generic;
using System.Text;
using Morpheus.PerformanceTests;

public class TestRandom1 : TestBase
{
    readonly Random m_rng;

    public TestRandom1()
    {
        OkToRun = false;
        TestName = "Test Random x1";
        m_rng = new Random();
    }

    public override void RunTest()
    {
        while (!StopRunning)
        {
            var x = m_rng.Next();
            var y = m_rng.NextDouble();
            var z = x * y;

            System.Threading.Interlocked.Increment( ref Iterations );
        }
    }
}

public class TestRandom2 : TestBase
{
    readonly Random m_rng;

    public TestRandom2()
    {
        OkToRun = false;
        TestName = "Test Random x2";
        m_rng = new Random();
    }

    public override void RunTest()
    {
        while (!StopRunning)
        {
            var x = m_rng.Next();
            var y = m_rng.NextDouble();
            var z = x * y;

            var a = m_rng.Next();
            var b = m_rng.NextDouble();
            var c = a * b;

            System.Threading.Interlocked.Increment( ref Iterations );
        }
    }
}
