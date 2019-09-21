using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;


#pragma warning disable 169
using Morpheus.PerformanceTests;

namespace Reflection.FieldSetter
{
    internal class FieldSetterDynamic : TestBase
    {
        private readonly FieldInfo m_memberInfo;
        private readonly long m_value;
        private readonly Action<FieldSetterDynamic, long> m_copier;


        public FieldSetterDynamic()
        {
            OkToRun = false;
            TestName = "Assignment with Dynamically Generated Method";

            m_memberInfo = typeof( FieldSetterDynamic ).GetField( "m_value", BindingFlags.Instance | BindingFlags.NonPublic );
            if (m_memberInfo == null) throw new InvalidProgramException( "Cannot find FieldInfo object" );

            var method = new DynamicMethod(
                "_DynamicCopier", // Method Name- inconsequential
                typeof( void ), // Return Type
                new Type[] { typeof( FieldSetterDynamic ), typeof( long ) }, // Param Types
                true );   // Dont worry about member visibility rules

            var il = method.GetILGenerator();

            il.Emit( OpCodes.Ldarg_0 );
            il.Emit( OpCodes.Ldarg_1 );
            il.Emit( OpCodes.Stfld, m_memberInfo );

            il.Emit( OpCodes.Ret );

            m_copier = (Action<FieldSetterDynamic, long>) method.CreateDelegate( typeof( Action<FieldSetterDynamic, long> ) );
        }

        public override void RunTest()
        {
            // The default test does nothing but give a baseline of how much the overhead costs.
            while (!StopRunning)
            {
                m_copier( this, Iterations );
                System.Threading.Interlocked.Increment( ref Iterations );
            }
        }
    }
}