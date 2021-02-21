using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Morpheus
{
    /// <summary>
    /// A better base class for enjoying a wider variety of random number sources.
    /// </summary>
    /// <remarks>
    /// Among the many reasons for not using <see cref="Random"/> 's default implementation
    /// include: <list type="bullet">
    /// <item> Random.Next(int) DOES NOT return unbiased numbers </item>
    /// <item> Random.Next() CANNOT return int.MaxValue </item>
    /// <item> Random.Next() has TWICE the chance of returning int.MaxValue - 1 than any other
    /// value. </item>
    /// <item> Random.NextDouble() DOES NOT return unbiased numbers </item>
    /// <item> Random.Sample() provides good but not great stochasticity </item>
    /// <item> Random.Sample() introduces error by projecting (almost) 31 bits onto a 52 bit
    /// space, resulting in sporatic "holes" in the 52 bit space that cannot be filled. There's
    /// no study that shows how this affects randomness. </item>
    /// <item> Random.Sample() DOES NOT provide cryptographically secure randomness </item>
    /// <item> Random's only provided seeding algorithm only accounts for time, which narrows
    /// the domain of attack vectors DRAMATICALLY and allows for objects created in succession
    /// to be seeded identically. </item>
    /// </list>
    /// <para> This class is meant to view the <see cref="Random.NextBytes(byte[])"/> method as
    /// the purest source of random data. It views any potential performance compromise from
    /// converting </para>
    /// </remarks>
    public abstract class RandomAspect : Random, IDisposable
    {
        private readonly ThreadLocal<byte[]> m_scaleBuffer4 = new ThreadLocal<byte[]>( () => new byte[4] );
        private readonly ThreadLocal<byte[]> m_scaleBuffer8 = new ThreadLocal<byte[]>( () => new byte[8] );

        /// <summary>
        /// The subclass must implement this based on how it naturally generates numbers. A
        /// 32-bit algorithm and a 64-bit algorithm should use different means for filling this
        /// buffer, let along an algorithm that generates <see cref="double"/> values.
        /// </summary>
        /// <param name="_buffer">The buffer to fill with bytes</param>
        /// <remarks>
        /// <para> The following is a potential override for an algorithm that naturally
        /// generates 32-bit values. </para>
        /// <code>
        /// public override void NextBytes( byte[] buffer ) => buffer.FromIntegers( () => Next32() );
        /// </code>
        /// <para> The following is a potential override for an algorithm that naturally
        /// generates 64-bit values. </para>
        /// <code>
        /// public override void NextBytes( byte[] buffer ) => buffer.FromIntegers( () => Next64() );
        /// </code>
        /// <para> The <see cref="RandomExtensions.FromIntegers(byte[], Func{uint})"/> and
        /// <see cref="RandomExtensions.FromIntegers(byte[], Func{ulong})"/> are part of
        /// Morpheus' RNG library </para>
        /// </remarks>
        public abstract override void NextBytes( byte[] _buffer );

        /// <summary>
        /// Generate 32 bits of random data. This data must not be biased- That means that the
        /// implementor must remove the bias and error from <see cref="Random"/> if that's
        /// really the algorithm you want to use.
        /// </summary>
        /// <returns>32 bits of random data</returns>
        public virtual uint Next32()
        {
            var buf = m_scaleBuffer4.Value;
            NextBytes( buf );
            return BitConverter.ToUInt32( buf, 0 );
        }

        /// <summary>
        /// Generate 64 bits of random data
        /// </summary>
        /// <returns>64 bits of random data</returns>
        public virtual ulong Next64()
        {
            var buf = m_scaleBuffer8.Value;
            NextBytes( buf );
            return BitConverter.ToUInt64( buf, 0 );
        }

        /// <summary>
        /// Default implementation takes 52 bits to determine the floating point value as a
        /// <see cref="double"/> .
        /// </summary>
        /// <returns>A <see cref="double"/> in the range [0..1)</returns>
        /// <remarks>
        /// Not sealed to make sure a subclass that natively generates floating point values can
        /// override this one.
        /// </remarks>
        public override double NextDouble() => this.Next64().LerpZeroToOne();

        /// <summary>
        /// Return TRUE or FALSE randomly.
        /// </summary>
        /// <returns>TRUE or FALSE- You don't know which until you call this method!</returns>
        public virtual bool NextBool() => (this.Next32() & 1) == 1;



        /// <summary>
        /// Return an integer in the range [0.. <see cref="int.MaxValue"/> ), just like the base
        /// class.
        /// </summary>
        /// <returns>
        /// An integer in the range [0.. <see cref="int.MaxValue"/> ), just like the base class.
        /// </returns>
        public override int Next() => (int)this.ScaleValue( int.MaxValue );


        /// <summary>
        /// Return an integer in the range [0..maxValue).
        /// </summary>
        /// <param name="maxValue">One greater than the largest possible return value</param>
        /// <returns>An integer in the range [0..maxValue).</returns>
        public override int Next( int maxValue ) => (maxValue > 0)
                                                    ? (int)this.ScaleValue( (uint)maxValue )
                                                    : throw new ArgumentException( "maxValue" );

        /// <summary>
        /// Return an integer in the range [0..maxValue).
        /// </summary>
        /// <param name="minValue">Smallest possible value for the return value</param>
        /// <param name="maxValue">One greater than the largest possible return value</param>
        /// <returns>An integer in the range [minValue..maxValue).</returns>
        public override int Next( int minValue, int maxValue ) => minValue + (int)this.ScaleValue( (uint)(maxValue - minValue) );



        /// <summary>
        /// Return an integer in the range [0..maxValue).
        /// </summary>
        /// <param name="maxValue">One greater than the largest possible return value</param>
        /// <returns>An integer in the range [0..maxValue).</returns>
        public virtual long NextLong( long maxValue ) => (maxValue > 0)
                                                    ? (long)this.ScaleValue( (ulong)maxValue )
                                                    : throw new ArgumentException( "maxValue" );

        /// <summary>
        /// Return an integer in the range [minValue..maxValue).
        /// </summary>
        /// <param name="minValue">Smallest possible value for the return value</param>
        /// <param name="maxValue">One greater than the largest possible return value</param>
        /// <returns>An integer in the range [minValue..maxValue).</returns>
        public virtual long NextLong( long minValue, long maxValue ) => minValue + (long)this.ScaleValue( (ulong)(maxValue - minValue) );



        /// <summary>
        /// Default implementation does nothing, and allows any subclass to implement as needed.
        /// </summary>
        public virtual void Dispose() { }
    }
}
