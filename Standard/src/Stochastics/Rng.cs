using System;

namespace Morpheus
{
    /// <summary>
    /// A better base class for enjoying a wider variety of random number sources.
    /// </summary>
    /// <remarks>
    /// Among the many reasons for not using <see cref="Random"/> 's default implementation
    /// include: <list type="bullet">
    /// <item> <see cref="Random.Next(int)"/> DOES NOT return unbiased numbers </item>
    /// <item> <see cref="Random"/> CANNOT return <see cref="int.MaxValue"/> </item>
    /// <item> <see cref="Random.Next()"/> has TWICE the chance of returning (
    /// <see cref="int.MaxValue"/> - 1) than any other value. </item>
    /// <item> <see cref="Random.NextDouble()"/> DOES NOT return unbiased numbers </item>
    /// <item> <see cref="Random.Sample()"/> introduces error by projecting (almost) 31 bits
    /// onto a 52 bit space, resulting in sporatic "holes" in the 52 bit space that cannot be
    /// filled. There's no study that shows how this affects randomness. </item>
    /// <item> <see cref="Random"/> DOES NOT provide cryptographically secure randomness </item>
    /// <item> Random's only provided (default) seeding algorithm only accounts for time, which
    /// narrows the domain of attack vectors DRAMATICALLY. It is also not re-entrant, therefore
    /// allowing objects created in tight succession to be seeded identically. </item>
    /// </list>
    /// 
    /// <para> This class introduces the <see cref="Next64"/> method, which is implemented by
    /// subclasses as the primary generator. Alternately, the subclass can re-implement
    /// <see cref="NextBytes(Span{byte})"/> , and this base class will handle converting to
    /// <see cref="Next64"/> . </para>
    /// 
    /// <para> If the subclass overrides any other generator method (e.g.
    /// <see cref="NextDouble"/> or <see cref="NextInt"/> , it must make sure
    /// <see cref="Next64"/> is correctly implemented to use said new method. If necessary, the
    /// subclass could implement <see cref="NextBytes(Span{byte})"/> instead, but this involves
    /// one extra step for all "Next*" methods because Next64 is the primary generator. </para>
    /// 
    /// <para> If <see cref="UseFastScale"/> is true, a tiny bias is introduced when scaling
    /// numbers using <see cref="Next()"/> , <see cref="Next(int)"/> and
    /// <see cref="Next(int, int)"/> , but speed is significantly faster. The bias is very small
    /// because the generator is 64 bits and these methods only result in 31 bits. </para>
    /// 
    /// </remarks>
    public abstract class Rng : Random, IDisposable
    {
        public static readonly Rng Default = new LCPRNG_MMIX();

        #region Sealing of Methods
        
        /// <summary>
        /// When set to TRUE, a biased scaled value will be returned for <see cref="Next"/> ,
        /// <see cref="Next(int)"/> and <see cref="Next(int, int)"/> .
        /// </summary>
        public bool UseFastScale = false;

        /// <summary>
        /// Return an integer in the range [0.. <see cref="int.MaxValue"/> ), just like
        /// <see cref="System.Random.Next()"/> .
        /// </summary>
        /// <returns>
        /// An integer in the range [0.. <see cref="int.MaxValue"/> ), just like
        /// <see cref="System.Random.Next()"/> .
        /// </returns>
        public sealed override int Next() => Next( int.MaxValue, UseFastScale );

        /// <summary>
        /// Return a <see cref="int"/> scaled in [0..maxValue).
        /// </summary>
        /// <param name="maxValue">One greater than the largest number to return</param>
        /// <returns></returns>
        public sealed override int Next( int maxValue ) => Next( maxValue, UseFastScale );

        /// <summary>
        /// Return an integer in the range [minValue..maxValue).
        /// </summary>
        /// <param name="minValue">Smallest possible value for the return value</param>
        /// <param name="maxValue">One greater than the largest possible return value</param>
        /// <returns>An integer in the range [minValue..maxValue).</returns>
        public sealed override int Next( int minValue, int maxValue ) => minValue + Next( maxValue - minValue, UseFastScale );


        /// <summary>
        /// Fill the provided byte array with random bytes
        /// </summary>
        /// <remarks>
        /// This method is sealed. To override, use <see cref="NextBytes(Span{byte})"/> instead.
        /// </remarks>
        /// <param name="buffer">The byte array to fill with random bytes</param>
        public sealed override void NextBytes( byte[] buffer ) => NextBytes( new Span<byte>( buffer ) );

        /// <summary>
        /// Return a <see cref="double"/> using all 52 bits of precision in [0..1)
        /// </summary>
        /// <returns>A <see cref="double"/> using all 52 bits of precision in [0..1)</returns>
        public sealed override double NextDouble() => Next64().AsDoubleZeroToOne();

        #endregion



        #region Scaling with Fast Option
        /// <summary>
        /// Like <see cref="Next"/> , but with the option to do a fast operation.
        /// </summary>
        /// <returns></returns>
        public int Next( bool useFast ) => Next( int.MaxValue, useFast );

        /// <summary>
        /// Return a <see cref="int"/> scaled in [0..maxValue).
        /// </summary>
        /// <remarks>This function lets a subclass override the scaling mechanism</remarks>
        /// <param name="maxValue">One greater than the largest number to return</param>
        /// <returns></returns>
        public virtual int Next( int maxValue, bool useFast = false )
        {
            if (useFast)
            {
                return (int)(Next64() % (uint)maxValue);
            }
            else
            {
                // 512 is arbitrary. Chance of loop completing is at worst asymptotic at 1 in
                // 2^512, also known as a virtual impossibility
                for (int i = 0; i < 512; i++)
                {
                    int retval = (int)(Next64().ScaleValue( (uint)maxValue, out var isUnbiased ));
                    if (isUnbiased) return retval;
                }
                throw new InvalidProgramException( "To prevent an infinite loop, no more numbers are being generated" +
                    " and its being assumed that the source of randomness is, in fact, nowhere near random." );
            }
        }

        /// <summary>
        /// Return an integer in the range [minValue..maxValue).
        /// </summary>
        /// <param name="minValue">Smallest possible value for the return value</param>
        /// <param name="maxValue">One greater than the largest possible return value</param>
        /// <returns>An integer in the range [minValue..maxValue).</returns>
        public int Next( int minValue, int maxValue, bool useFast ) => minValue + Next( maxValue - minValue, useFast );

        #endregion



        #region Primitive Datatype Operations
        /// <summary>
        /// Return an integer in the range [0.. <see cref="int.MaxValue"/> ]. Unlike
        /// <see cref="Next()"/> , this method returns the entire 31 bits of data, including a
        /// possible <see cref="int.MaxValue"/> .
        /// </summary>
        /// <returns>An integer in the range [0.. <see cref="int.MaxValue"/> ]</returns>
        public int NextInt() => (int)(Next64() & int.MaxValue);

        /// <summary>
        /// Return an integer in the range [0.. <see cref="long.MaxValue"/> ]. Unlike
        /// <see cref="Next()"/> , this method returns the entire 63 bits of data, including a
        /// possible <see cref="long.MaxValue"/> .
        /// </summary>
        /// <returns>
        /// An integer in the range [0.. <see cref="long.MaxValue"/> ), just like the base
        /// class.
        /// </returns>
        public long NextLong() => (long)(Next64() & long.MaxValue);

        /// <summary>
        /// Randomly return True or False
        /// </summary>
        /// <returns>True or False. Really.</returns>
        public bool NextBool() => (Next64() & 1) == 1;

        /// <summary>
        /// 32 bits of random data
        /// </summary>
        /// <remarks>
        /// If this is what's implemented in the subclass, you must also implement
        /// <see cref="Next64"/> in the subclass.
        /// </remarks>
        /// <returns>32 bits of random data</returns>
        public virtual uint Next32() => (uint)Next64();

        /// <summary>
        /// Return 64 bits of random data. This method drives all other non-byte methods in this
        /// class.
        /// </summary>
        /// <remarks>
        /// If not implemented by the subclass, the subclass MUST override
        /// <see cref="NextBytes(byte[])"/> .
        /// </remarks>
        /// <returns></returns>
        public virtual ulong Next64()
        {
            // This should only be executed when the sub-class must implement NextBytes for some
            // reason
            Span<byte> buffer = stackalloc byte[8];
            NextBytes( buffer );

            var asUlong = buffer.Cast<byte, ulong>();

            ulong retval = asUlong[0];
            return retval;
        }

        /// <summary>
        /// Fill the provided byte array with random bytes. This is optimized for use with
        /// <see cref="Next64"/> .
        /// </summary>
        /// <param name="buffer">The buffer to fill with data</param>
        public override void NextBytes( Span<byte> buffer )
        {
            int len = buffer.Length;
            int blockCount = (buffer.Length & ~0b111) >> 3;
            bool leftovers = (len & 0b111) != 0;

            if (blockCount > 0)
            {
                var as64 = buffer.Cast<byte, ulong>();
                for (int i = 0; i < blockCount; i++)
                    as64[i] = Next64();

                if (leftovers) // overlap some of the last bytes to facilitate a single write
                {
                    var last64 = buffer.Slice( len - 8 ).Cast<byte, ulong>();
                    last64[0] = Next64();
                }
            }
            else if (leftovers)
            {
                var x = Next64();
                for (int idx = 0; idx < len; idx++)
                {
                    buffer[idx] = (byte)(x & 0xff);
                    x >>= 8;
                }
            }
        }
        #endregion



        #region Miscellaneous and Administrative

        /// <summary>
        /// Just in case anyone wanted to use this relic from <see cref="System.Random"/>
        /// </summary>
        /// <returns>If you don't know, don't use this.</returns>
        protected override double Sample() => NextDouble();

        /// <summary>
        /// Typically, simply pulling a random number is suitable to "advance" the state of the
        /// algorithm. However, if an algorithm uses a different mechanism, it can implement an
        /// override.
        /// </summary>
        public virtual void Advance() => Next64();


        /// <summary>
        /// nothing by default
        /// </summary>
        public virtual void Dispose() { }

        #endregion
    }
}
