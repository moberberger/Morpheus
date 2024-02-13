// <copyright file="MersenneTwister.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// 
// Copyright (c) 2009-2017 Math.NET
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software
// and associated documentation files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
// BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

/*
   Original code's copyright and license:
   Copyright (C) 1997 - 2002, Makoto Matsumoto and Takuji Nishimura,
   All rights reserved.

   Redistribution and use in source and binary forms, with or without
   modification, are permitted provided that the following conditions
   are met:

     1. Redistributions of source code must retain the above copyright
        notice, this list of conditions and the following disclaimer.

     2. Redistributions in binary form must reproduce the above copyright
        notice, this list of conditions and the following disclaimer in the
        documentation and/or other materials provided with the distribution.

     3. The names of its contributors may not be used to endorse or promote
        products derived from this software without specific prior written
        permission.

   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
   "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
   LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
   A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT OWNER OR
   CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
   EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
   PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
   PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
   LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
   NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
   SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.


   Any feedback is very welcome.
   http://www.math.sci.hiroshima-u.ac.jp/~m-mat/MT/emt.html
   email: m-mat @ math.sci.hiroshima-u.ac.jp (remove space)
*/


namespace Morpheus
{
    /// <summary>
    /// Random number generator using Mersenne Twister 19937 algorithm.
    /// </summary>
    public class MersenneTwister : Rng
    {
        /// <summary>
        /// The number of integers required to store the state space. Actually this is 1 bit too
        /// large.
        /// </summary>
        public const int STATE_SIZE_IN_INTS = 624;

        /// <summary>
        /// Mersenne twister constant.
        /// </summary>
        public const int M = 397;

        /// <summary>
        /// Mask for b31
        /// </summary>
        private const uint sm_upperMask = 0x80000000;

        /// <summary>
        /// Mask for bits b0..b30
        /// </summary>
        private const uint sm_lowerMask = 0x7fffffff;


        /// <summary>
        /// Mersenne twister constant.
        /// </summary>
        private static readonly uint[] sm_mag01 = { 0x0U, 0x9908b0df };

        /// <summary>
        /// This is the state space for the RNG.
        /// </summary>
        private readonly uint[] m_stateSpace = new uint[STATE_SIZE_IN_INTS];

        /// <summary>
        /// The index into the state space where the next value will come from.
        /// </summary>
        private int m_ssIdx = STATE_SIZE_IN_INTS + 1;



        /// <summary>
        /// Initializes a new instance of the <see cref="MersenneTwister"/> class TODO: based on
        /// what seed?
        /// </summary>
        public MersenneTwister()
        {
            Initialize( (uint)RandomSeed.Robust() );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MersenneTwister"/> class.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        public MersenneTwister( uint seed )
        {
            Initialize( seed );
        }


        /// <summary>
        /// Initialize the state space using a Knuth algorithm.
        /// </summary>
        /// <param name="seed">A seed used to initialize the state space</param>
        private void Initialize( uint seed )
        {
            m_stateSpace[0] = seed & 0xffff_ffff;
            for (m_ssIdx = 1; m_ssIdx < STATE_SIZE_IN_INTS; m_ssIdx++)
            {
                m_stateSpace[m_ssIdx] = 1812433253 * (m_stateSpace[m_ssIdx - 1] ^ (m_stateSpace[m_ssIdx - 1] >> 30)) + (uint)m_ssIdx;
                /* See Knuth TAOCP Vol2. 3rd Ed. P.106 for multiplier. */
                /* In the previous versions, MSBs of the seed affect   */
                /* only MSBs of the array _mt[].                        */
                /* 2002/01/09 modified by Makoto Matsumoto             */
                m_stateSpace[m_ssIdx] &= 0xffffffff;
                /* for >32 bit machines */
            }
        }

        /// <summary>
        /// Return an unsigned integer with all bits randomized.
        /// </summary>
        /// <returns>An unsigned integer with all bits randomized</returns>
        public override uint Next32()
        {
            uint y;

            /* mag01[x] = x * MATRIX_A  for x=0,1 */

            if (m_ssIdx >= STATE_SIZE_IN_INTS)
            {
                /* generate _n words at one time */
                int kk;

                for (kk = 0; kk < STATE_SIZE_IN_INTS - M; kk++)
                {
                    y = (m_stateSpace[kk] & sm_upperMask) | (m_stateSpace[kk + 1] & sm_lowerMask);
                    m_stateSpace[kk] = m_stateSpace[kk + M] ^ (y >> 1) ^ sm_mag01[y & 0x1];
                }

                for (; kk < STATE_SIZE_IN_INTS - 1; kk++)
                {
                    y = (m_stateSpace[kk] & sm_upperMask) | (m_stateSpace[kk + 1] & sm_lowerMask);
                    m_stateSpace[kk] = m_stateSpace[kk + (M - STATE_SIZE_IN_INTS)] ^ (y >> 1) ^ sm_mag01[y & 0x1];
                }

                y = (m_stateSpace[STATE_SIZE_IN_INTS - 1] & sm_upperMask) | (m_stateSpace[0] & sm_lowerMask);
                m_stateSpace[STATE_SIZE_IN_INTS - 1] = m_stateSpace[M - 1] ^ (y >> 1) ^ sm_mag01[y & 0x1];

                m_ssIdx = 0;
            }

            y = m_stateSpace[m_ssIdx++];

            /* Tempering */
            y ^= y >> 11;
            y ^= (y << 7) & 0x9d2c5680;
            y ^= (y << 15) & 0xefc60000;
            y ^= y >> 18;

            return y;
        }

        /// <summary>
        /// Return an unsigned long integer with all bits randomized.
        /// </summary>
        /// <returns>An unsigned long integer with all bits randomized</returns>
        public override ulong Next64() => (ulong)Next32() << 32 | Next32();
    }
}
