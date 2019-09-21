using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;


#pragma warning disable 169
using Morpheus.PerformanceTests;

namespace Reflection.MethodInvocation
{
    internal class InvokeMethodDynamic : InvokeMethodBase
    {
        private readonly Func<long, long> m_caller;
        private readonly MethodInfo m_memberInfo;

        public InvokeMethodDynamic()
        {
            //OkToRun = true;
            TestName = "Invoking a method using a Dynamically Created (Emit) Method";

            m_memberInfo = GetType().GetMethod( "SomeOperation", BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public );
            if (m_memberInfo == null) throw new InvalidProgramException( "Cannot find SomeOperation method" );

            var caller = new DynamicMethod(
                "_DynamicCaller", // Method Name- inconsequential
                typeof( long ), // Return Type
                new Type[] { typeof( InvokeMethodBase ), typeof( long ) }, // Param Types
                true );   // Dont worry about member visibility rules

            var il = caller.GetILGenerator();

            il.Emit( OpCodes.Ldarg_0 );
            il.Emit( OpCodes.Ldarg_1 );
            il.Emit( OpCodes.Call, m_memberInfo );

            il.Emit( OpCodes.Ret );

            m_caller = (Func<long, long>) caller.CreateDelegate( typeof( Func<long, long> ) );
            m_caller( 0 ); // JIT maybe?
        }

        public override void RunTest()
        {
            // The default test does nothing but give a baseline of how much the overhead costs.
            while (!StopRunning)
            {
                var x = m_caller( Iterations );
                if (Iterations != x) throw new InvalidOperationException();
                System.Threading.Interlocked.Increment( ref Iterations );
            }
        }
    }
}
