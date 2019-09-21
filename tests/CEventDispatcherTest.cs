using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;



namespace Morpheus.Standard.UnitTests
{
    public enum EShapes { Square, Circle, Shape, Nothing }

    [TestClass]
    public class CEventDispatcherTest
    {
        #region Events
        public class EvtShape
        {
            public string Name = "Shape";
        }

        public class EvtSquare : EvtShape
        {
            public EvtSquare()
            {
                Name = "Square";
            }
        }

        public class EvtCircle : EvtShape
        {
            public EvtCircle()
            {
                Name = "Circle";
            }
        }
        #endregion

        private bool m_square, m_circle, m_shape, m_anything;


        private void ClearFlags() => m_anything = m_shape = m_square = m_circle = false;

        public void HandleSquare( EvtSquare _square, MessageHandler _handler ) => m_square = true;

        public void HandleShape( EvtShape _square, Dispatcher _handler ) => m_shape = true;

        private Dispatcher GetDispatcher()
        {
            var dispatcher = new Dispatcher();

            dispatcher.RegisterHandler<EvtSquare>( HandleSquare );
            dispatcher.RegisterHandler<EvtCircle>( ( _d, _e ) => m_circle = true );
            dispatcher.RegisterHandler<EvtShape, Dispatcher>( HandleShape );
            dispatcher.RegisterHandler<object>( _d => m_anything = true );
            return dispatcher;
        }


        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        public void ExactValueTest()
        {
            var dispatcher = new Dispatcher();

            dispatcher.RegisterHandler( EShapes.Square, h => m_square = true );
            dispatcher.RegisterHandler( EShapes.Circle, h => m_circle = true );
            dispatcher.RegisterHandler( EShapes.Shape, h => m_shape = true );

            ClearFlags();
            dispatcher.Post( EShapes.Square );

            Assert.IsTrue( m_square, "m_square" );
            Assert.IsFalse( m_circle, "m_circle" );
            Assert.IsFalse( m_shape, "m_shape" );
            Assert.IsFalse( m_anything, "m_anything" );
            Assert.AreEqual( 1, dispatcher.EventExecutionCount, "Event Execution Count" );

            ClearFlags();
            dispatcher.ResetExecutionCount();
            dispatcher.Post( EShapes.Circle );
            dispatcher.Post( EShapes.Shape );

            Assert.IsFalse( m_square, "m_square" );
            Assert.IsTrue( m_circle, "m_circle" );
            Assert.IsTrue( m_shape, "m_shape" );
            Assert.IsFalse( m_anything, "m_anything" );
            Assert.AreEqual( 2, dispatcher.EventExecutionCount, "Event Execution Count" );
        }

        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        public void ExactValueTest2()
        {
            var dispatcher = new CStaticEventHandlersAndDispatcher();

            dispatcher.RegisterHandler<CStaticEventHandlersAndDispatcher>( EShapes.Square, _disp => _disp.m_square = true );

            dispatcher.ClearFlags();
            dispatcher.Post( EShapes.Circle );
            dispatcher.Post( EShapes.Square );

            Assert.IsTrue( dispatcher.m_square, "m_square" );
            Assert.IsFalse( dispatcher.m_circle, "m_circle" );
            Assert.IsFalse( dispatcher.m_shape, "m_shape" );
            Assert.IsFalse( dispatcher.m_anything, "m_anything" );
            Assert.AreEqual( 1, dispatcher.EventExecutionCount, "Event Execution Count" );
        }

        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        public void ExactValueReflectionTest()
        {
            var dispatcher = new CInstanceSpecificValueHandlers
            {
                Step = 2
            };

            var disc = new MessageHandlerDiscovery( dispatcher );
            disc.RegisterInstanceHandlers( dispatcher );

            dispatcher.Post( EShapes.Circle );
            dispatcher.Post( EShapes.Circle );

            Assert.AreEqual( 0, dispatcher.Squares, "Squares 1" );
            Assert.AreEqual( 4, dispatcher.Circles, "Circles 1" );
            Assert.AreEqual( 0, dispatcher.Shapes, "Shapes 1" );

            dispatcher.Step = 3;
            dispatcher.Post( EShapes.Shape );
            dispatcher.Post( EShapes.Square );
            dispatcher.Post( EShapes.Square );
            dispatcher.Post( EShapes.Square );

            Assert.AreEqual( 9, dispatcher.Squares, "Squares 1" );
            Assert.AreEqual( 4, dispatcher.Circles, "Circles 1" );
            Assert.AreEqual( 3, dispatcher.Shapes, "Shapes 1" );

        }

        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        public void ExactValueAndEnumTest()
        {
            var dispatcher = new Dispatcher();
            int cSquare = 0, cCircle = 0;

            dispatcher.RegisterHandler<EShapes>(
                ( _shape, _handler ) =>
                {
                    switch (_shape)
                    {
                    case EShapes.Square:
                        cSquare++;
                        break;
                    case EShapes.Circle:
                        cCircle++;
                        break;
                    case EShapes.Shape:
                        break;
                    case EShapes.Nothing:
                        break;
                    default:
                        break;
                    }
                } );
            dispatcher.RegisterHandler( EShapes.Square, _handler => cSquare++ );

            dispatcher.Post( EShapes.Shape );
            Assert.AreEqual( 0, cSquare, "1st cSquare" );
            Assert.AreEqual( 0, cCircle, "1st cCircle" );

            dispatcher.Post( EShapes.Circle );
            dispatcher.Post( EShapes.Square );

            Assert.AreEqual( 2, cSquare, "2nd cSquare" );
            Assert.AreEqual( 1, cCircle, "2nd cCircle" );

        }

        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        public void ExactStringValueTest()
        {
            var dispatcher = new Dispatcher();
            int cMiss = 0, cHit = 0;

            dispatcher.RegisterHandler( "hit", _x => cHit++ );
            dispatcher.RegisterHandler( "miss", _x => cMiss++ );

            var shit = new StringBuilder( "h" );
            shit.Append( 'i' );
            shit.Append( "t" );

            dispatcher.Post( "hit" );
            dispatcher.Post( "miss" );
            dispatcher.Post( shit.ToString() ); // This should count, even though its put together piecemeal and can't be considered a constant like the string passed to the RegisterHandler function
            dispatcher.Post( "Miss" ); // This should NOT trigger the handler, as "Miss" != "miss"

            Assert.AreEqual( 1, cMiss, "Miss Count" );
            Assert.AreEqual( 2, cHit, "Hit Count" );
        }







        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        public void SquareTest()
        {
            var dispatcher = GetDispatcher();

            ClearFlags();
            dispatcher.Post( new EvtSquare() );
            Assert.IsTrue( m_square, "m_square" );
            Assert.IsFalse( m_circle, "m_circle" );
            Assert.IsTrue( m_shape, "m_shape" );
            Assert.IsTrue( m_anything, "m_anything" );
            Assert.AreEqual( 3, dispatcher.EventExecutionCount, "Event Execution Count" );
        }

        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        public void CircleTest()
        {
            var dispatcher = GetDispatcher();

            ClearFlags();
            dispatcher.Post( new EvtCircle() );
            Assert.IsTrue( m_anything, "m_anything" );
            Assert.IsTrue( m_shape, "m_shape" );
            Assert.IsFalse( m_square, "m_square" );
            Assert.IsTrue( m_circle, "m_circle" );
            Assert.AreEqual( 3, dispatcher.EventExecutionCount, "Event Execution Count" );
        }

        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        public void ShapeTest()
        {
            var dispatcher = GetDispatcher();

            ClearFlags();
            dispatcher.Post( new EvtShape() );
            Assert.IsTrue( m_anything, "m_anything" );
            Assert.IsTrue( m_shape, "m_shape" );
            Assert.IsFalse( m_square, "m_square" );
            Assert.IsFalse( m_circle, "m_circle" );
            Assert.AreEqual( 2, dispatcher.EventExecutionCount, "Event Execution Count" );
        }

        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        public void NoShapeTest()
        {
            var dispatcher = GetDispatcher();

            ClearFlags();
            dispatcher.Post( "what?" );
            Assert.IsTrue( m_anything, "m_anything" );
            Assert.IsFalse( m_shape, "m_shape" );
            Assert.IsFalse( m_square, "m_square" );
            Assert.IsFalse( m_circle, "m_circle" );
            Assert.AreEqual( 1, dispatcher.EventExecutionCount, "Event Execution Count" );
        }





        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        public void SquareBatchTest()
        {
            var dispatcher = GetDispatcher();
            dispatcher.DefaultDispatchMode = EDispatchMode.Batched;

            ClearFlags();
            dispatcher.Post( new EvtSquare() );

            Assert.IsTrue( dispatcher.HasPendingEvents(), "Has pending events" );

            Assert.IsFalse( m_square, "m_square" );
            Assert.IsFalse( m_circle, "m_circle" );
            Assert.IsFalse( m_shape, "m_shape" );
            Assert.IsFalse( m_anything, "m_anything" );
            Assert.AreEqual( 0, dispatcher.EventExecutionCount, "Event Execution Count 0" );

            dispatcher.ExecuteBatch( 0 ); // execute one of them- The most specific event handler (i.e. not Shape or Object)
            Assert.IsTrue( m_square, "m_square" );
            Assert.IsFalse( m_circle, "m_circle" );
            Assert.IsFalse( m_shape, "m_shape" );
            Assert.IsFalse( m_anything, "m_anything" );
            Assert.AreEqual( 1, dispatcher.EventExecutionCount, "Event Execution Count 1" );

            dispatcher.ResetExecutionCount();
            dispatcher.ExecuteBatch();
            Assert.IsTrue( m_square, "m_square" );
            Assert.IsFalse( m_circle, "m_circle" );
            Assert.IsTrue( m_shape, "m_shape" );
            Assert.IsTrue( m_anything, "m_anything" );
            Assert.AreEqual( 2, dispatcher.EventExecutionCount, "Event Execution Count 2" );
            Assert.IsFalse( dispatcher.HasPendingEvents(), "No more events pending" );
        }

        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        public void CircleBatchTest()
        {
            var dispatcher = GetDispatcher();

            ClearFlags();
            dispatcher.Post( new EvtCircle(), EDispatchMode.Batched );

            Assert.IsTrue( dispatcher.HasPendingEvents(), "Has pending events" );

            Assert.IsFalse( m_square, "m_square" );
            Assert.IsFalse( m_circle, "m_circle" );
            Assert.IsFalse( m_shape, "m_shape" );
            Assert.IsFalse( m_anything, "m_anything" );
            Assert.AreEqual( 0, dispatcher.EventExecutionCount, "Event Execution Count 0" );

            dispatcher.ExecuteBatch( 0 ); // execute one of them
            Assert.IsFalse( m_square, "m_square" );
            Assert.IsTrue( m_circle, "m_circle" );
            Assert.IsFalse( m_shape, "m_shape" );
            Assert.IsFalse( m_anything, "m_anything" );
            Assert.AreEqual( 1, dispatcher.EventExecutionCount, "Event Execution Count 1" );

            dispatcher.ExecuteBatch();
            Assert.IsFalse( m_square, "m_square" );
            Assert.IsTrue( m_circle, "m_circle" );
            Assert.IsTrue( m_shape, "m_shape" );
            Assert.IsTrue( m_anything, "m_anything" );
            Assert.AreEqual( 3, dispatcher.EventExecutionCount, "Event Execution Count 2" );
            Assert.IsFalse( dispatcher.HasPendingEvents(), "No more events pending" );

        }


        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        public void SquareBatchTimeoutTest()
        {
            var dispatcher = GetDispatcher();
            dispatcher.DefaultDispatchMode = EDispatchMode.Batched;

            var slept = false;
            dispatcher.RegisterHandler<EvtShape>( ( _d, _e ) => { Thread.Sleep( 20 ); slept = true; } );

            var wrongThreadId = false;
            var notThreadpoolThread = false;
            var threadId = Thread.CurrentThread.ManagedThreadId;
            dispatcher.RegisterHandler<object>( ( _d, _e ) =>
            {
                if (threadId == Thread.CurrentThread.ManagedThreadId)
                    wrongThreadId = true;
                if (!Thread.CurrentThread.IsThreadPoolThread)
                    notThreadpoolThread = true;
            } );


            ClearFlags();
            dispatcher.Post( new EvtSquare() );

            Assert.IsTrue( dispatcher.HasPendingEvents(), "Has pending events" );

            Assert.IsFalse( m_square, "m_square" );
            Assert.IsFalse( m_circle, "m_circle" );
            Assert.IsFalse( m_shape, "m_shape" );
            Assert.IsFalse( m_anything, "m_anything" );
            Assert.AreEqual( 0, dispatcher.EventExecutionCount, "Event Execution Count 0" );
            Assert.IsFalse( slept, "Did sleep" );

            dispatcher.ExecuteBatch( 1 ); // execute for one millisecond- should never be able to execute ALL event handlers
            Assert.IsTrue( dispatcher.EventExecutionCount < 4, "Count should be less than 4" );

            dispatcher.ExecuteBatch();
            Assert.IsTrue( m_square, "m_square" );
            Assert.IsFalse( m_circle, "m_circle" );
            Assert.IsTrue( m_shape, "m_shape" );
            Assert.IsTrue( m_anything, "m_anything" );
            Assert.AreEqual( 5, dispatcher.EventExecutionCount, "Event Execution Count 1" );
            Assert.IsTrue( slept, "Didn't sleep" );

            Assert.IsFalse( dispatcher.HasPendingEvents(), "No more events pending" );
            Assert.IsTrue( wrongThreadId, "Wrong thread id" );
            Assert.IsTrue( notThreadpoolThread, "Not threadpool thread" );
        }

        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        public void SquareThreadpoolTest()
        {
            var dispatcher = GetDispatcher();

            var slept = false;
            dispatcher.RegisterHandler<EvtShape>( ( _d, _e ) => { Thread.Sleep( 10 ); slept = true; } );

            var wrongThreadId = false;
            var notThreadpoolThread = false;
            var threadId = Thread.CurrentThread.ManagedThreadId;
            dispatcher.RegisterHandler<object>( ( _d, _e ) =>
                {
                    if (threadId == Thread.CurrentThread.ManagedThreadId)
                        wrongThreadId = true;
                    if (!Thread.CurrentThread.IsThreadPoolThread)
                        notThreadpoolThread = true;
                } );

            ClearFlags();
            dispatcher.Post( new EvtSquare(), EDispatchMode.Threadpool );

            while (dispatcher.HasPendingEvents())
                Thread.Sleep( 10 );

            var count = dispatcher.ExecuteBatch();
            Assert.AreEqual( 0, count, "Should have no batched handlers" );

            Assert.IsTrue( m_square, "m_square" );
            Assert.IsFalse( m_circle, "m_circle" );
            Assert.IsTrue( m_shape, "m_shape" );
            Assert.IsTrue( m_anything, "m_anything" );
            Assert.AreEqual( 5, dispatcher.EventExecutionCount, "Event Execution Count 1" );
            Assert.IsTrue( slept, "Didn't sleep" );

            Assert.IsFalse( dispatcher.HasPendingEvents(), "No more events pending" );
            Assert.IsFalse( wrongThreadId, "Wrong thread id" );
            Assert.IsFalse( notThreadpoolThread, "Not threadpool thread" );
        }


        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        public void EventExceptionTest()
        {
            var exceptionHandled = false;
            var rethrowException = false;

            var dispatcher = new Dispatcher();
            dispatcher.RegisterHandler<EvtSquare>( ( _e, _h ) =>
                {
                    if (_e.Name == "X")
                        throw new InvalidProgramException();
                    m_square = true;
                } );
            dispatcher.RegisterHandler<EvtShape>( ( _e, _h ) =>
                {
                    m_shape = true;
                } );
            dispatcher.RegisterHandler<DispatcherException>( ( _e, _h ) =>
                {
                    if (rethrowException)
                        throw _e.Exception; // allows for re-throw and "loss" of exception

                    exceptionHandled = true;
                } );


            ClearFlags();
            dispatcher.Post( new EvtSquare() );

            Assert.IsTrue( m_square, "m_square" );
            Assert.IsFalse( m_circle, "m_circle" );
            Assert.IsTrue( m_shape, "m_shape" );
            Assert.IsFalse( m_anything, "m_anything" );
            Assert.AreEqual( 2, dispatcher.EventExecutionCount, "Event Execution Count 1" );
            Assert.IsFalse( exceptionHandled, "No Exception to be Handled" );

            ClearFlags();
            dispatcher.Post( new EvtSquare() { Name = "X" } );
            Assert.IsFalse( m_square, "m_square" );
            Assert.IsFalse( m_circle, "m_circle" );
            Assert.IsTrue( m_shape, "m_shape" );
            Assert.IsFalse( m_anything, "m_anything" );
            Assert.AreEqual( 5, dispatcher.EventExecutionCount, "Event Execution Count 1" );
            Assert.IsTrue( exceptionHandled, "Exception NOT Handled" );

            ClearFlags();
            exceptionHandled = false;
            rethrowException = true; // Make it so that "exceptionHandled" isn't set this time
            dispatcher.Post( new EvtSquare() { Name = "X" } );
            Assert.IsFalse( m_square, "m_square" );
            Assert.IsFalse( m_circle, "m_circle" );
            Assert.IsTrue( m_shape, "m_shape" );
            Assert.IsFalse( m_anything, "m_anything" );
            Assert.AreEqual( 8, dispatcher.EventExecutionCount, "Event Execution Count 1" );
            Assert.IsFalse( exceptionHandled, "Exception Handled" );

        }

        private const int RECURSIVE_START_VAL = 1 << 29; // 29 is highest exp because of size of "int"

        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        public void RecursiveInlineTest()
        {
            var sum = 0;
            var x = RECURSIVE_START_VAL;

            var dispatcher = new Dispatcher();
            dispatcher.RegisterHandler<int>( ( _h, _i ) =>
            {
                if (x == 0)
                    return;

                sum += x;
                x >>= 1;
                if (x > 0)
                {
                    dispatcher.Post( x );

                    sum += x;
                    x >>= 1;
                    if (x > 0)
                        dispatcher.Post( x );
                }
            } );

            dispatcher.Post( x );

            Assert.AreEqual( (RECURSIVE_START_VAL * 2 - 1), sum, "Sum Incorrect" );
            Assert.AreEqual( (RECURSIVE_START_VAL.Log2Int() + 1), dispatcher.EventExecutionCount, "Event Count" );
        }

        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        public void RecursiveBatchTest()
        {
            var sum = 0;
            var x = RECURSIVE_START_VAL;

            var dispatcher = new Dispatcher();
            dispatcher.RegisterHandler<int>( ( _h, _i ) =>
            {
                if (x == 0)
                    return;

                sum += x;
                x >>= 1;
                if (x > 0)
                {
                    dispatcher.Post( x );

                    sum += x;
                    x >>= 1;
                    if (x > 0)
                        dispatcher.Post( x );
                }
            } );

            dispatcher.Post( x, EDispatchMode.Batched );
            Assert.AreEqual( 0, sum, "Sum should still be zero" );
            Assert.AreEqual( 0, dispatcher.EventExecutionCount, "Count should still be zero" );

            dispatcher.ExecuteBatch();

            Assert.AreEqual( (RECURSIVE_START_VAL * 2 - 1), sum, "Sum Incorrect" );
            Assert.AreEqual( (RECURSIVE_START_VAL.Log2Int() + 1), dispatcher.EventExecutionCount, "Event Count" );
        }

        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        public void RecursiveThreadpoolTest()
        {
            var sum = 0;
            var x = RECURSIVE_START_VAL;
            var sync = new object();

            var dispatcher = new Dispatcher
            {
                DefaultDispatchMode = EDispatchMode.Threadpool
            };

            dispatcher.RegisterHandler<int>( ( _h, _i ) =>
            {
                if (x == 0)
                    return;

                Assert.IsTrue( Thread.CurrentThread.IsThreadPoolThread );

                lock (sync) // heavy-handed thread safety... this Test is mainly testing the Threadpool aspect. Proving that sometimes one of the accumulation operators can be interrupted by a context switch is not the issue.
                {
                    sum += x;

                    x >>= 1;
                    if (x > 0)
                    {
                        dispatcher.Post( x );

                        sum += x;
                        x >>= 1;
                        if (x > 0)
                            dispatcher.Post( x );
                    }
                }
            } );

            dispatcher.Post( x );

            while (dispatcher.HasPendingEvents())
                Thread.Sleep( 1 );

            Assert.AreEqual( (RECURSIVE_START_VAL * 2 - 1), sum, "Sum Incorrect" );
            Assert.AreEqual( (RECURSIVE_START_VAL.Log2Int() + 1), dispatcher.EventExecutionCount, "Event Count" );
        }



        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        public void DiscoveryMainTest()
        {
            var disp = new Dispatcher();
            var disc = new MessageHandlerDiscovery( disp );
            disc.RegisterHandlers();

            CStaticEventHandlersForTest.ClearFlags();
            disp.Post( new EvtSquare() );

            Assert.AreEqual( true, CStaticEventHandlersForTest.m_square, "Square" );
            Assert.AreEqual( false, CStaticEventHandlersForTest.m_circle, "Circle" );
            Assert.AreEqual( false, CStaticEventHandlersForTest.m_shape, "Shape" );
            Assert.AreEqual( false, CStaticEventHandlersForTest.m_anything, "Anything" );

            CStaticEventHandlersForTest.ClearFlags();
            disp.Post( new EvtCircle() );

            Assert.AreEqual( false, CStaticEventHandlersForTest.m_square, "Square" );
            Assert.AreEqual( true, CStaticEventHandlersForTest.m_circle, "Circle" );
            Assert.AreEqual( true, CStaticEventHandlersForTest.m_shape, "Shape" );
            Assert.AreEqual( false, CStaticEventHandlersForTest.m_anything, "Anything" );

        }



        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        public void DiscoveryHandlersNeedingDispatcherTest()
        {
            var disp = new CStaticEventHandlersAndDispatcher();
            var disc = new MessageHandlerDiscovery( disp );
            disc.RegisterHandlers( typeof( CStaticEventHandlersAndDispatcher ) );

            disp.ClearFlags();
            disp.Post( new EvtSquare() );

            Assert.AreEqual( true, disp.m_square, "Square" );
            Assert.AreEqual( false, disp.m_circle, "Circle" );
            Assert.AreEqual( false, disp.m_shape, "Shape" );
            Assert.AreEqual( false, disp.m_anything, "Anything" );
            Assert.AreEqual( 1, disp.Count, "Count" );

            disp.ClearFlags();
            disp.Post( new EvtCircle() );

            Assert.AreEqual( false, disp.m_square, "Square2" );
            Assert.AreEqual( true, disp.m_circle, "Circle2" );
            Assert.AreEqual( true, disp.m_shape, "Shape2" );
            Assert.AreEqual( false, disp.m_anything, "Anything2" );
            Assert.AreEqual( 2, disp.Count, "Coutn2" );
        }


        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        public void InstanceMethodHandlersTest()
        {
            var disp = new Dispatcher();
            var disc = new MessageHandlerDiscovery( disp );

            var x1 = new CInstanceEventHandlers() { Step = 1 };
            var x2 = new CInstanceEventHandlers() { Step = 7 };

            disc.RegisterInstanceHandlers( x1 );
            disc.RegisterInstanceHandlers( x2 );

            disp.Post( new EvtShape() );
            disp.Post( new EvtCircle() );
            disp.Post( new EvtSquare() );
            disp.Post( new EvtShape() );
            disp.Post( new EvtCircle() );
            disp.Post( new EvtSquare() );

            Assert.AreEqual( 8, disp.EventExecutionCount, "Execution Count" );
            Assert.AreEqual( 2, x1.Circles, "x1 circles" );
            Assert.AreEqual( 2, x1.Squares, "x1 squares" );
            Assert.AreEqual( 14, x2.Circles, "x2 circles" );
            Assert.AreEqual( 14, x2.Squares, "x2 squares" );
        }

        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        public void DeregisterInstanceMethodsTest()
        {
            var disp = new Dispatcher();
            var disc = new MessageHandlerDiscovery( disp );

            var x1 = new CInstanceEventHandlers() { Step = 1 };
            var x2 = new CInstanceEventHandlers() { Step = 7 };

            disc.RegisterInstanceHandlers( x1 );
            disc.RegisterInstanceHandlers( x2 );

            disp.RegisterHandler<object>( x => m_anything = true );

            var h1 = disp.GetAllHandlers();
            Assert.AreEqual( 5, h1.Count, "Count after registration" );

            disp.DeregisterHandlerObject( x2 );
            var h2 = disp.GetAllHandlers();
            Assert.AreEqual( 3, h2.Count, "Count after Deregistration" );
        }

        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        public void DeregisterHandlerTest()
        {
            var disp = new Dispatcher();

            var hsq = disp.RegisterHandler<EvtSquare>( x => m_square = true );
            var hcr = disp.RegisterHandler<EvtCircle>( x => m_circle = true );

            ClearFlags();
            disp.Post( new EvtSquare() );
            Assert.IsTrue( m_square, "square" );
            Assert.IsFalse( m_circle, "circle" );

            var success = disp.DeregisterHandler( hsq );
            Assert.IsTrue( success, "Deregister Square" );

            ClearFlags();
            disp.Post( new EvtSquare() );
            disp.Post( new EvtCircle() );
            Assert.IsFalse( m_square, "square 2" );
            Assert.IsTrue( m_circle, "circle 2" );

            success = disp.DeregisterHandler( hsq );
            Assert.IsFalse( success, "Deregister Second Square" );

            success = disp.DeregisterHandler( hcr );
            Assert.IsTrue( success, "Deregister Circle" );

            ClearFlags();
            disp.Post( new EvtSquare() );
            disp.Post( new EvtCircle() );

            Assert.IsFalse( m_circle, "circle 3" );
            Assert.IsFalse( m_square, "square 3" );
        }





        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        [ExpectedException( typeof( XReflectionArgumentException ) )]
        public void X_TooManyParamsTest()
        {
            var disp = new Dispatcher();
            var disc = new MessageHandlerDiscovery( disp );
            disc.RegisterHandlers( typeof( CBadEventHandler_TooManyParams ) );
        }

        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        [ExpectedException( typeof( XReflectionArgumentException ) )]
        public void X_ZeroParamsTest()
        {
            var disp = new Dispatcher();
            var disc = new MessageHandlerDiscovery( disp );
            disc.RegisterHandlers( typeof( CBadEventHandler_ZeroParams ) );
        }

        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        [ExpectedException( typeof( XReflectionArgumentException ) )]
        public void X_InvalidSecondParamTest()
        {
            var disp = new Dispatcher();
            var disc = new MessageHandlerDiscovery( disp );
            disc.RegisterHandlers( typeof( CBadEventHandler_InvalidSecondParam ) );
        }

        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        [ExpectedException( typeof( XReflectionArgumentException ) )]
        public void X_AttributeMethodEventMismatchTest()
        {
            var disp = new Dispatcher();
            var disc = new MessageHandlerDiscovery( disp );
            disc.RegisterHandlers( typeof( CBadEventHandler_AttributeMethodEventMismatch ) );
        }

        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        [ExpectedException( typeof( XReflectionArgumentException ) )]
        public void X_SecondParamBadDispatcherTest()
        {
            var disp = new CBadEventHandler_SecondParamBadDispatcher();
            var disc = new MessageHandlerDiscovery( disp );
            disc.RegisterHandlers( typeof( CBadEventHandler_SecondParamBadDispatcher ) );

            disp.Post( 3 );
        }

        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        [ExpectedException( typeof( InvalidOperationException ) )]
        public void X_InvalidDefaultDispatchModeTest()
        {
            var disp = new Dispatcher
            {
                DefaultDispatchMode = EDispatchMode.NotAssigned
            };
        }

        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        [ExpectedException( typeof( InvalidProgramException ) )]
        public void X_InvalidDefaultDispatchModeTest2()
        {
            var disp = new Dispatcher
            {
                DefaultDispatchMode = (EDispatchMode) 55 // This should break things good!
            };

            disp.RegisterHandler<object>( e => m_anything = true );

            disp.Post( 8 );
        }

        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void X_DeregisterNullArgTest1()
        {
            var disp = new Dispatcher();
            disp.DeregisterHandlerObject( null );
        }

        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void X_DeregisterNullArgTest2()
        {
            var disp = new Dispatcher();
            disp.DeregisterHandler( null );
        }


        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        [ExpectedException( typeof( ArgumentException ) )]
        public void X_LambdaRegisrationDispatcherTypeMismatchTest()
        {
            var disp = new CStaticEventHandlersAndDispatcher();
            disp.RegisterHandler<int, CBadEventHandler_SecondParamBadDispatcher>( ( x, y ) => { } );
        }

        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        [ExpectedException( typeof( ArgumentException ) )]
        public void X_LambdaRegisrationDispatcherTypeMismatchTest2()
        {
            var disp = new CStaticEventHandlersAndDispatcher();
            disp.RegisterHandler<CBadEventHandler_SecondParamBadDispatcher>( AttributeTargets.Assembly, x => { } );
        }


        [TestMethod]
        [TestCategory( "EventDispatcher" )]
        [ExpectedException( typeof( XReflectionArgumentException ) )]
        public void X_InvalidTypeOnAttributeTest()
        {
            var disp = new Dispatcher();
            var disc = new MessageHandlerDiscovery( disp );
            disc.RegisterHandlers( typeof( CBadEventHandler_InvalidEventTypeOnAttribute ) );
        }


    }


    [AEventHandler] // allow assembly-level discovery to search this class
    public class CStaticEventHandlersForTest
    {
        public static bool m_square, m_circle, m_shape, m_anything;

        public static void ClearFlags() => m_anything = m_shape = m_square = m_circle = false;

        [AEventHandler]
        public static void HandleSquare( CEventDispatcherTest.EvtSquare _square ) => m_square = true;

        [AEventHandler( EventType = typeof( CEventDispatcherTest.EvtCircle ) )]
        public static void HandleCircleViaShape( CEventDispatcherTest.EvtShape _shape ) => m_shape = true;

        [AEventHandler( EventType = typeof( CEventDispatcherTest.EvtCircle ) )]
        public static void HandleCircle() => m_circle = true;
    }



    // no attribute means that assembly-level discovery will not catch this class
    public class CStaticEventHandlersAndDispatcher : Dispatcher
    {
        public int Count;

        public bool m_square, m_circle, m_shape, m_anything;

        public void ClearFlags()
        {
            m_anything = m_shape = m_square = m_circle = false;
            Count = 0;
        }

        [AEventHandler]
        public static void HandleSquare( CEventDispatcherTest.EvtSquare _square, CStaticEventHandlersAndDispatcher _dispatcher )
        {
            _dispatcher.Count++;
            _dispatcher.m_square = true;
        }

        [AEventHandler( EventType = typeof( CEventDispatcherTest.EvtCircle ) )]
        public static void HandleCircleViaShape( CEventDispatcherTest.EvtShape _shape, MessageHandler _handler )
        {
            var disp = _handler.Dispatcher as CStaticEventHandlersAndDispatcher;
            disp.Count++;
            disp.m_shape = true;
        }

        [AEventHandler( EventType = typeof( CEventDispatcherTest.EvtCircle ) )]
        public static void HandleCircle( object _event, Dispatcher _dispatcher )
        {
            Assert.IsInstanceOfType( _dispatcher, typeof( CStaticEventHandlersAndDispatcher ) );
            ((CStaticEventHandlersAndDispatcher) _dispatcher).Count++;
            ((CStaticEventHandlersAndDispatcher) _dispatcher).m_circle = true;
        }

        [AEventHandler( EventType = typeof( int ) )]
        public static void HandleInt()
        {
        }

        [AEventHandler]
        public static void HandleFloat( float x )
        {
        }
    }

    public class CInstanceEventHandlers
    {
        public int Step = 1;
        public int Squares = 0;
        public int Circles = 0;

        [AEventHandler( EventType = typeof( CEventDispatcherTest.EvtSquare ) )]
        public void HandleSquare() => Squares += Step;

        [AEventHandler]
        public void HandleCircle( CEventDispatcherTest.EvtCircle _event ) => Circles += Step;
    }

    public class CInstanceSpecificValueHandlers : Dispatcher
    {
        public int Step = 1;
        public int Squares = 0;
        public int Circles = 0;
        public int Shapes = 0;

        [AEventHandler( Value = EShapes.Square )]
        public void ASquare() => Squares += Step;

        [AEventHandler( Value = EShapes.Circle )]
        public void ACircle( Dispatcher _dispatcher, EShapes _event )
        {
            var disp = _dispatcher as CInstanceSpecificValueHandlers;
            disp.Circles += disp.Step;
        }

        [AEventHandler( Value = EShapes.Shape, EventType = typeof( EShapes ) )]
        public void AShape( MessageHandler _eh, object _event, CInstanceSpecificValueHandlers _dispatcher )
        {
            _dispatcher.Shapes += _dispatcher.Step;
            Assert.AreEqual( this, _dispatcher, "Dispatcher Instance Mismatch" );
        }
    }

    /// <summary>
    /// An event handler can have zero, one, or two parameters.
    /// </summary>
    public class CBadEventHandler_TooManyParams
    {
        [AEventHandler]
        public static void TooManyParams( object _event, MessageHandler _handler, int _what, Dispatcher _dispatcher )
        {
        }
    }

    public class CBadEventHandler_ZeroParams
    {
        [AEventHandler]
        public static void ZeroParams()
        {
        }
    }

    public class CBadEventHandler_InvalidSecondParam
    {
        [AEventHandler]
        public static void BadSecondParameter( object _event, object _shouldBeSomethingElse )
        {
        }
    }

    public class CBadEventHandler_SecondParamBadDispatcher : Dispatcher
    {
        [AEventHandler]
        public static void InvalidDispatcherParam( object _event, CStaticEventHandlersAndDispatcher _shouldBeThisClassOrCEventDispatcher )
        {
        }
    }

    public class CBadEventHandler_AttributeMethodEventMismatch
    {
        [AEventHandler( EventType = typeof( CEventDispatcherTest.EvtShape ) )]
        public static void EventAttributeMismatch( CEventDispatcherTest.EvtCircle _circle )
        {
        }
    }


    public class CBadEventHandler_InvalidEventTypeOnAttribute
    {
        [AEventHandler( EventType = typeof( object ), Value = EShapes.Square )]
        public static void Homer()
        {
        }
    }

}
