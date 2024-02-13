﻿using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Morpheus;


/// <summary>
/// This class contains a variety of seed generators for the RandomAspect library. The
/// generators typically trade performance for "goodness", giving the caller a choice
/// depending on their needs.
/// </summary>
/// <remarks>
/// <para> This is the statistical analysis of bytes generated by successive calls to
/// <see cref="RandomSeed.RDTSC"/> taking the low-order 8 bits at each iteration </para>
/// <code>
///  SUMMARY
///  -------
///  monobit_test                             0.8681446538335948    PASS
///  frequency_within_block_test              0.9999357428902714    PASS
///  runs_test                                0.10626576644609134   PASS
///  longest_run_ones_in_a_block_test         4.491865045016535e-07 FAIL
///  binary_matrix_rank_test                  4.438018943301276e-49 FAIL
///  dft_test                                 2.21766509461179e-182 FAIL
///  non_overlapping_template_matching_test   1.0000022434613756    PASS
///  overlapping_template_matching_test       1.683301591456356e-16 FAIL
///  maurers_universal_test                   0.997760242333619     PASS
///  linear_complexity_test                   0.5619100494450724    PASS
///  serial_test                              0.191681946146207     PASS
///  approximate_entropy_test                 0.19122350061307122   PASS
///  cumulative_sums_test                     0.7256891633163818    PASS
///  random_excursion_test                    0.07472173045065679   PASS
///  random_excursion_variant_test            0.016125950440928533  PASS
/// </code>
/// </remarks>
public static class RandomSeed
{
    /// <summary>
    /// Simply calls the RDTSC assembly language instruction
    /// </summary>
    /// <returns>RDTSC assembly language instruction</returns>
    [DllImport( "RDTSC.dll", CallingConvention = CallingConvention.StdCall )]
    private static extern ulong RDTSC_Wrapper();
    [DllImport( "RDTSC.dll", CallingConvention = CallingConvention.StdCall )]
    private static extern uint RDSEED32_Wrapper();
    [DllImport( "RDTSC.dll", CallingConvention = CallingConvention.StdCall )]
    private static extern ulong RDSEED64_Wrapper();
    [DllImport( "RDTSC.dll", CallingConvention = CallingConvention.StdCall )]
    private static extern uint RDRAND32_Wrapper();
    [DllImport( "RDTSC.dll", CallingConvention = CallingConvention.StdCall )]
    private static extern ulong RDRAND64_Wrapper();


    private static bool sm_rdtscExists = false;

    private static readonly RandomNumberGenerator sm_masterRng = RandomNumberGenerator.Create();
    private static readonly object sm_lock = new object();
    private static readonly ulong sm_rdtscAtStaticInitialization = 0;
    private static ulong sm_rdtscAtFirstCall = 0;
    private static ulong sm_instantiationSeed = 0;


    /// <summary>
    /// Figure out if the RDTSC dll is available.
    /// </summary>
    static RandomSeed()
    {
        try
        {
            RDTSC_Wrapper(); // force the call
            sm_rdtscExists = true; // set the flag

            // NOW this routine will work
            sm_rdtscAtStaticInitialization = RDTSC8();
        }
        catch (Exception e)
        {
            sm_rdtscExists = false;
            Console.WriteLine( $"RDTSC_Wrapper not found: {e.Message}" );
            Console.WriteLine( Directory.GetCurrentDirectory() );
        }

        Initialize();
    }

    /// <summary>
    /// Simply calls the RDTSC assembly language instruction. If the RDTSC DLL cannot be
    /// found, RDTSC always returns zero.
    /// </summary>
    /// <remarks>
    /// <para> The invocation into the DLL takes roughly ~11.2ns, while simply returning 0
    /// because there was no DLL found takes roughly ~2.7ns. This was on an i7, but what's
    /// germaine is the relative timing costs associated with this call. </para>
    /// </remarks>
    public static ulong RDTSC() => sm_rdtscExists ? RDTSC_Wrapper() : 0;
    public static uint RDSEED32() => sm_rdtscExists ? RDSEED32_Wrapper() : 0;
    public static uint RDRAND32() => sm_rdtscExists ? RDRAND32_Wrapper() : 0;
    public static ulong RDSEED64() => sm_rdtscExists ? RDSEED64_Wrapper() : 0;
    public static ulong RDRAND64() => sm_rdtscExists ? RDRAND64_Wrapper() : 0;

    /// <summary>
    /// Return a <see cref="long"/> based on making 8 calls to <see cref="RDTSC"/> , taking
    /// the low-order 8 bits from each value, and contatenating those bytes into an integer.
    /// </summary>
    /// <returns>
    /// 64 bits formed by taking the low-order byte from 8 successive calls to
    /// <see cref="RDTSC"/> .
    /// </returns>
    /// <remarks>
    /// <para> This is the statistical analysis of bytes generated by successive calls to
    /// <see cref="RandomSeed.RDTSC"/> in a tight loop taking the low-order 8 bits at each
    /// iteration </para>
    /// <code>
    ///  SUMMARY
    ///  -------
    ///  monobit_test                             0.9610562683926701    PASS
    ///  frequency_within_block_test              0.9920294541502036    PASS
    ///  runs_test                                0.4042884756461461    PASS
    ///  longest_run_ones_in_a_block_test         7.670617444517102e-05 FAIL
    ///  binary_matrix_rank_test                  6.950380905432474e-38 FAIL
    ///  dft_test                                 2.71546984117062e-305 FAIL
    ///  non_overlapping_template_matching_test   0.999984121230964     PASS
    ///  overlapping_template_matching_test       8.792081781545951e-08 FAIL
    ///  maurers_universal_test                   0.9864574605063752    PASS
    ///  linear_complexity_test                   0.24587090671700623   PASS
    ///  serial_test                              0.9746528818603666    PASS
    ///  approximate_entropy_test                 0.9947113128247125    PASS
    ///  cumulative_sums_test                     0.985656443158553     PASS
    ///  random_excursion_test                    0.014093394157334602  PASS
    ///  random_excursion_variant_test            0.10218040549352575   PASS
    /// </code>
    /// </remarks>
    public static ulong RDTSC8()
    {
        ulong x = RDTSC(); // All the extra bits [b8..b63] will be shifted off
        x <<= 8; x |= RDTSC() & 0xff;
        x <<= 8; x |= RDTSC() & 0xff;
        x <<= 8; x |= RDTSC() & 0xff;
        Thread.Sleep( 0 );
        x <<= 8; x |= RDTSC() & 0xff;
        x <<= 8; x |= RDTSC() & 0xff;
        x <<= 8; x |= RDTSC() & 0xff;
        x <<= 8; x |= RDTSC() & 0xff;

        return x;
    }

    /// <summary>
    /// Set up the seeding algorithms
    /// </summary>
    private static void Initialize()
    {
        sm_rdtscAtFirstCall = LCPRNG_MMIX.Next( RDTSC8() );

        // Temporal: Related to when in time this initialization occurred. Not super
        // accurate ( around 100ns i believe).
        ulong medium = (ulong)DateTime.Now.Ticks;

        // Temporal: Related to when(-ish) this object was loaded by .NET. This is
        // affected by what other objects in the program have already been instantiated.
        // Since this is Framework, we assume a varied instantiation order, yet
        // admittedly (potentially?) constant for each specific program. Please review
        // GetHashCode for more info on how this value is determined.
        ulong course1 = LCPRNG_MMIX.Next( (ulong)sm_instantiationSeed.GetHashCode() );
        ulong course2 = LCPRNG_MMIX.Next( (ulong)sm_lock.GetHashCode() );

        // These bytes are generated in part based on the crypto seeding algorithm,
        // thereby adding significant randomness
        var buf = new byte[8];
        sm_masterRng.GetBytes( buf );
        ulong crypto = BitConverter.ToUInt64( buf, 0 );

        // So merge these with lossless XOR
        sm_instantiationSeed = LCPRNG_MMIX.Next( medium ^ course1 ^ course2 ^ crypto );
    }


    public static int Fast() => (int)FastULong();
    /// <summary>
    /// Except for one-time initialization, this consists of one compare, one interlocked
    /// increment, a multiply and add, and a cast. Provides a value affected by
    /// instantiation count run through Knuth's values for a 64-bit linear congruential
    /// generator. Initialization uses three different time values merged together
    /// non-destructively.
    /// </summary>
    /// <returns>
    /// A seed that will be statistically different each time this is called.
    /// </returns>
    /// <remarks>
    /// This is not suitable for cryptography. Knowing when the program was started can
    /// significantly compromise the secret of this algorithm.
    /// </remarks>
    public static ulong FastULong()
    {
        // Very quickly attribute the fact that this is a "+1" operation (i.e. allow
        // multiple rapid invocations yield different results)
        Interlocked.Increment( ref sm_instantiationSeed );
        sm_instantiationSeed = LCPRNG_MMIX.Next( sm_instantiationSeed );

        return sm_instantiationSeed ^ sm_rdtscAtFirstCall ^ sm_rdtscAtStaticInitialization;
    }

    /// <summary>
    /// Uses <see cref="Fast"/> merged with the timestamp of when this was called. This adds
    /// the overhead of getting the system time to Fast(), but adds unpredictability as this
    /// changes the seed not only with each instantiation count, but also with the system
    /// time, both of which advance monotonically but at very different rates.
    /// </summary>
    /// <returns></returns>
    public static int Medium() => (int)((long)Fast() ^ Stopwatch.GetTimestamp());

    /// <summary>
    /// This is a very robust algorithm. It is crypto-secure. It is relatively slow.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// <para> This method takes roughly 200 times as long as <see cref="Medium()"/> . On my
    /// i7, this was roughly 9.17µs for Robust, 47.17ns for Medium(). </para>
    /// <code>
    ///  SUMMARY
    ///  -------
    ///  monobit_test                             0.19266546205306553   PASS
    ///  frequency_within_block_test              0.056454336286505466  PASS
    ///  runs_test                                0.2372301747212983    PASS
    ///  longest_run_ones_in_a_block_test         0.08671490897112756   PASS
    ///  binary_matrix_rank_test                  0.758467899173356     PASS
    ///  dft_test                                 0.8339015763555688    PASS
    ///  non_overlapping_template_matching_test   1.0000001068095663    PASS
    ///  overlapping_template_matching_test       0.2757838770554818    PASS
    ///  maurers_universal_test                   0.9996589852559914    PASS
    ///  linear_complexity_test                   0.8248302677938896    PASS
    ///  serial_test                              0.4765342543039493    PASS
    ///  approximate_entropy_test                 0.4769349373048006    PASS
    ///  cumulative_sums_test                     0.22862119514107992   PASS
    ///  random_excursion_test                    0.3951480349886672    PASS
    ///  random_excursion_variant_test            0.055131784641997125  PASS
    ///  </code>
    /// </remarks>
    public static int Robust( byte[]? _userSeedData = null )
    {
        var buffer = new MemoryStream();
        using (var writer = new BinaryWriter( buffer ))
        {
            writer.Write( RDTSC8() );

            var mediumVal = Medium();
            writer.Write( mediumVal );

            var timerBased = CreateTimerBasedSeedData();
            writer.Write( timerBased );

            var fastVal = Fast();
            writer.Write( fastVal );

            writer.Write( sm_rdtscAtStaticInitialization );
            writer.Write( sm_rdtscAtFirstCall );
            writer.Write( sm_rdtscExists );

            var guid = new Guid();
            writer.Write( guid.ToByteArray() );

            if (_userSeedData != null)
                writer.Write( _userSeedData );

            writer.Write( RDTSC8() );

            using (var hasher = SHA512.Create())
            {
                var hash = hasher.ComputeHash( buffer.ToArray() );

                var index = (int)(RDTSC() & 0x1f); // 0-31
                var retval = BitConverter.ToInt32( hash, index ); // take 4 bytes from somewhere in the hash
                return retval;
            }
        }
    }

    /// <summary>
    /// Generate a <see cref="long"/> value based on taking either the RDTSC (if it exists)
    /// or the Ticks from <see cref="Stopwatch.GetTimestamp"/>
    /// </summary>
    /// <remarks>
    /// This routine takes 8 of these values and contatenates each of their low-order 8
    /// bits.
    /// </remarks>
    /// <returns>
    /// 64 bits of data coming from the low-order 8 bits of 8 consecutive calls to the
    /// timestamp routine.
    /// </returns>
    public static long CreateTimerBasedSeedData()
    {
        long last = 0L, seed = 0L;

        for (var i = -1; i < 8; i++)
        {
            var counter = sm_rdtscExists ? (long)RDTSC() : Stopwatch.GetTimestamp();

            if (i >= 0)
            {
                var delta = counter - last;
                seed <<= 8;
                seed |= (byte)(delta & 0xff);
            }

            last = counter;
            Thread.Sleep( 0 );
        }

        return seed;
    }

}
