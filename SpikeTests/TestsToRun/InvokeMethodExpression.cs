using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;


#pragma warning disable 169
using TestHarness = Morpheus.PerformanceTests.TestBase;

namespace Reflection.MethodInvocation
{
    internal class InvokeMethodExpression : InvokeMethodBase
    {
        private delegate long TFunc( InvokeMethodExpression obj, long val );

        private readonly TFunc m_caller;
        private readonly MethodInfo m_memberInfo;



        public InvokeMethodExpression()
        {
            //OkToRun = true;
            TestName = "Invoking a method using Expression";

            m_memberInfo = GetType().GetMethod( "SomeOperation", BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public );
            if (m_memberInfo == null) throw new InvalidProgramException( "Cannot find SomeOperation method" );

            try
            {
                var exObjParam = Expression.Parameter( typeof( InvokeMethodBase ), "ThisObject" );
                var exValueParam = Expression.Parameter( typeof( long ), "ValueToAssign" );

                var exCall = Expression.Call( exObjParam, m_memberInfo, exValueParam );

                var ex = Expression.Lambda<TFunc>( exCall, exObjParam, exValueParam );

                m_caller = ex.Compile();
                m_caller( this, 0 );
            }
            catch (Exception ex)
            {
                Console.WriteLine( $"{ex}" );
                throw;
            }
        }

        public override void RunTest()
        {
            // The default test does nothing but give a baseline of how much the overhead costs.
            while (!StopRunning)
            {
                var x = m_caller( this, Iterations );
                if (Iterations != x) throw new InvalidOperationException();
                System.Threading.Interlocked.Increment( ref Iterations );
            }
        }

    }
}