using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morpheus
{
    /// <summary>
    /// A class that determines how to make one object intercept a second moving object
    /// </summary>
    /// <remarks>
    /// To use this class, set the four input properties ChaserPosition, ChaserSpeed,
    /// RunnerPosition and RunnerVelocity.
    /// 
    /// Then, simply "get" the properties InterceptionPoint, ChaserVelocity, TimeToInterception
    /// and InterceptionPossible.
    /// </remarks>
    public class InterceptCalculator2d
    {
        private SVector2d m_chaserPosition = SVector2d.NotAVector;
        private double m_chaserSpeed = double.NaN;
        private SVector2d m_runnerPosition = SVector2d.NotAVector;
        private SVector2d m_runnerVelocity = SVector2d.NotAVector;

        private bool m_interceptionPossible = false;
        private SVector2d m_chaserVelocity = SVector2d.NotAVector;
        private SVector2d m_interceptionPoint = SVector2d.NotAVector;
        private double m_timeToInterception = double.NaN;


        private bool m_calculationPerformed = false;


        /// <summary>
        /// SET: The location of the chaser
        /// </summary>
        public SVector2d ChaserPosition
        {
            get => m_chaserPosition;
            set
            {
                ClearResults();
                m_chaserPosition = value;
            }
        }

        /// <summary>
        /// SET: How fast the chaser can move in "some" direction, tbd by this class
        /// </summary>
        public double ChaserSpeed
        {
            get => m_chaserSpeed;
            set
            {
                ClearResults();
                m_chaserSpeed = value;
            }
        }

        /// <summary>
        /// SET: The postion of the Runner
        /// </summary>
        public SVector2d RunnerPosition
        {
            get => m_runnerPosition;
            set
            {
                ClearResults();
                m_runnerPosition = value;
            }
        }

        /// <summary>
        /// SET: The velocity vector (Speed and Direction) of the Runner
        /// </summary>
        public SVector2d RunnerVelocity
        {
            get => m_runnerVelocity;
            set
            {
                ClearResults();
                m_runnerVelocity = value;
            }
        }



        /// <summary>
        /// GET: The point where interception will occur, if it is possible
        /// </summary>
        public SVector2d InterceptionPoint
        {
            get
            {
                SetResults();
                return m_interceptionPoint;
            }
        }

        /// <summary>
        /// The Velocity of the chaser
        /// </summary>
        public SVector2d ChaserVelocity
        {
            get
            {
                SetResults();
                return m_chaserVelocity;
            }
        }

        /// <summary>
        /// The time when interception will occur
        /// </summary>
        public double TimeToInterception
        {
            get
            {
                SetResults();
                return m_timeToInterception;
            }
        }

        /// <summary>
        /// TRUE if interception is possible, FALSE if not
        /// </summary>
        public bool InterceptionPossible
        {
            get
            {
                SetResults();
                return m_interceptionPossible;
            }
        }


        /// <summary>
        /// Force re-compute of the interception when any output variables are queried
        /// </summary>
        public void ClearResults()
        {
            m_calculationPerformed = false;
            m_interceptionPossible = false;
            m_chaserVelocity = SVector2d.NotAVector;
            m_interceptionPoint = SVector2d.NotAVector;
            m_timeToInterception = double.NaN;
        }

        /// <summary>
        /// TRUE if all inputs are valid, FALSE if not. All inputs must be valid in order for
        /// any of the outputs to be valid.
        /// </summary>
        public bool HasValidInputs => m_chaserPosition.IsAVector &&
                                      m_runnerPosition.IsAVector &&
                                      m_runnerVelocity.IsAVector &&
                                      !double.IsNaN( m_chaserSpeed ) &&
                                      !double.IsInfinity( m_chaserSpeed );


        /// <summary>
        /// Internal- Calculate the interception
        /// </summary>
        private void SetResults()
        {
            if (m_calculationPerformed)
                return;

            // Make sure all results look like "no interception possible".
            ClearResults();

            // Set this to TRUE regardless of the success or failure of interception
            m_calculationPerformed = true;

            // If the inputs are invalid, then everything is already set for a "no interception"
            // scenario.
            if (!HasValidInputs)
                return;


            // First check- Are we already on top of the target? If so, its valid and we're done
            if (ChaserPosition.AreSame( RunnerPosition ))
            {
                m_interceptionPossible = true;
                m_interceptionPoint = ChaserPosition;
                m_timeToInterception = 0;
                m_chaserVelocity = new SVector2d();
                return;
            }

            // Check- Am I moving? Be gracious about exception throwing even though negative
            // speed is undefined.
            if (ChaserSpeed <= 0)
                return; // No interception


            var vectorFromRunner = ChaserPosition - RunnerPosition;
            var distanceToRunner = vectorFromRunner.Length;
            var runnerSpeed = RunnerVelocity.Length;

            // Check- Is the other thing not moving? If it isn't, the calcs don't work because
            // we can't use the Law of Cosines, so use this shortcut
            if (runnerSpeed.IsClose( 0 ))
            {
                m_timeToInterception = distanceToRunner / ChaserSpeed;
                m_interceptionPoint = RunnerPosition;
            }
            else // Everything looks OK for the Law of Cosines approach
            {
                var cosTheta = vectorFromRunner.CosineOfAngleBetween( RunnerVelocity );

                // Now set up the quadratic formula coefficients
                var a = ChaserSpeed * ChaserSpeed - runnerSpeed * runnerSpeed;
                var b = 2 * distanceToRunner * runnerSpeed * cosTheta;
                var c = -distanceToRunner * distanceToRunner;

                if (!CMath.QuadraticSolver( a, b, c, out var t1, out var t2 ))
                {
                    // No real-valued solution, so no interception possible
                    return;
                }

                if (t1 < 0 && t2 < 0)
                {
                    // Both values for t are negative, so the interception would have to have
                    // occured in the past
                    return;
                }

                if (t1 > 0 && t2 > 0) // Both are positive, take the smaller one
                    m_timeToInterception = Math.Min( t1, t2 );
                else // One has to be negative, so take the larger one
                    m_timeToInterception = Math.Max( t1, t2 );

                m_interceptionPoint = RunnerPosition + RunnerVelocity * m_timeToInterception;
            }

            // Calculate the resulting velocity based on the time and intercept position
            m_chaserVelocity = (m_interceptionPoint - ChaserPosition) / m_timeToInterception;

            // Finally, signal that the interception was possible.
            m_interceptionPossible = true;
        }
    }
}
