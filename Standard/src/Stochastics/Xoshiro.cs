namespace Morpheus;

/*  Written in 2018 by David Blackman and Sebastiano Vigna (vigna@acm.org)

To the extent possible under law, the author has dedicated all copyright
and related and neighboring rights to this software to the public domain
worldwide. This software is distributed without any warranty.

See <http://creativecommons.org/publicdomain/zero/1.0/>.

   This is xoshiro256+ 1.0, our best and fastest generator for floating-point
   numbers. We suggest to use its upper bits for floating-point
   generation, as it is slightly faster than xoshiro256++/xoshiro256**. It
   passes all tests we are aware of except for the lowest three bits,
   which might fail linearity tests (and just those), so if low linear
   complexity is not considered an issue (as it is usually the case) it
   can be used to generate 64-bit outputs, too.

   We suggest to use a sign test to extract a random Boolean value, and
   right shifts to extract subsets of bits.

   The state must be seeded so that it is not everywhere zero. If you have
   a 64-bit seed, we suggest to seed a splitmix64 generator and use its
   output to fill s. 

Translated to C# in 2024 by Michael Oberberger
*/

public class Xoshiro : Rng
{
    private ulong[] s { get; } = new ulong[4];

    public Xoshiro() : this( RandomSeed.FastULong() ) { }
    public Xoshiro( ulong seed )
    {
        var rng = new LCPRNG_MMIX( seed );
        for (int i = 0; i < 4; i++)
            s[i] = rng.Next64();
    }


    public override ulong Next64()
    {
        ulong result = s[0] + s[3];
        ulong t = s[1] << 17;

        s[2] ^= s[0];
        s[3] ^= s[1];
        s[1] ^= s[2];
        s[0] ^= s[3];

        s[2] ^= t;
        s[3] = rotl( s[3], 45 );

        return result;
    }
    private static ulong rotl( ulong x, int k ) => (x << k) | (x >> (64 - k));
}
