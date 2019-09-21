using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

#pragma warning disable 169
using Morpheus.PerformanceTests;

namespace Reflection.PropertySetter
{
    internal class PropertySetterPropertyInfo : TestBase
    {
        private long MyValue { get; set; }

        private readonly PropertyInfo m_memberInfo;

        public PropertySetterPropertyInfo()
        {
            OkToRun = false;
            TestName = "Setting Values using PropertyInfo";

            m_memberInfo = GetType().GetProperty( "MyValue", BindingFlags.NonPublic | BindingFlags.Instance );
            if (m_memberInfo == null) throw new InvalidProgramException( "Cannot find PropertyInfo object" );
        }

        public override void RunTest()
        {
            // The default test does nothing but give a baseline of how much the overhead costs.
            while (!StopRunning)
            {
                m_memberInfo.SetValue( this, Iterations ); // arbitrary what we set it to
                System.Threading.Interlocked.Increment( ref Iterations );
            }
        }
    }
}
