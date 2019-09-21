using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

#pragma warning disable 169
using Morpheus.PerformanceTests;

namespace Reflection.FieldSetter
{
    internal class FieldSetterFieldInfo : TestBase
    {
        private readonly long m_value;
        private readonly FieldInfo m_memberInfo;

        public FieldSetterFieldInfo()
        {
            OkToRun = false;
            TestName = "Setting a Field using FieldInfo";

            m_memberInfo = GetType().GetField( "m_value", BindingFlags.NonPublic | BindingFlags.Instance );
            if (m_memberInfo == null) throw new InvalidProgramException( "Cannot find FieldInfo object" );
        }

        public override void RunTest()
        {
            // The default test does nothing but give a baseline of how much the overhead costs.
            while (!StopRunning)
            {
                m_memberInfo.SetValue( this, Iterations );
                System.Threading.Interlocked.Increment( ref Iterations );
            }
        }
    }
}