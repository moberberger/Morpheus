namespace Morpheus;


/// <summary>
/// Knuth's MMIX full 64-bit LCPRNG.
/// 
/// No representation to its spectral characteristics.
/// </summary>
public class LCPRNG_MMIX : LCPRNG
{
    public const ulong Multiplier = 6364136223846793005UL;
    public const ulong Increment = 1442695040888963407UL;

    public LCPRNG_MMIX() : base( Multiplier, Increment ) { }
    public LCPRNG_MMIX( ulong seed ) : base( Multiplier, Increment, seed ) { }
    public static ulong Next( ulong state ) => (Multiplier * state) + Increment;
}
