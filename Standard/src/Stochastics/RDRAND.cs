namespace Morpheus;

/// <summary> RDRAND uses the intrinsic methods for both seeding and generating numbers. It is
/// not possible to "pre-seed" thereby regenerating an identical sequence.
/// 
/// <code>
/// Refer to Intel® 64 and IA-32 Architectures, 
///     Software Developer’s Manual
///     Volume 1, Section 7.3.17,
///     “Random Number Generator Instructions”).
/// </summary>
public class RDRAND : Rng
{
    public RDRAND() => RandomSeed.RDSEED64();

    public override ulong Next64() => RandomSeed.RDRAND64();
    public override uint Next32() => RandomSeed.RDRAND32();
}
