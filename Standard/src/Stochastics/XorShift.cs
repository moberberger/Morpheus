namespace Morpheus;


/***
 *    ___  ___                           _ _       
 *    |  \/  |                          | (_)      
 *    | .  . | __ _ _ __ ___  __ _  __ _| |_  __ _ 
 *    | |\/| |/ _` | '__/ __|/ _` |/ _` | | |/ _` |
 *    | |  | | (_| | |  \__ \ (_| | (_| | | | (_| |
 *    \_|  |_/\__,_|_|  |___/\__,_|\__, |_|_|\__,_|
 *                                  __/ |          
 *                                 |___/           
 */


/// <summary>
/// The Marsaglia XorShift PRNG for both 32 and 64 bit values.
/// </summary>
public class XorShift : Rng
{
    protected uint _state32;
    protected ulong _state64;

    /// <summary>
    /// Construct using default seed generator
    /// </summary>
    public XorShift() => _state32 = (uint)(_state64 = RandomSeed.FastULong());

    /// <summary>
    /// Construct using specific seed
    /// </summary>
    public XorShift( ulong initialState ) => _state32 = (uint)(_state64 = initialState);

    /// <summary>
    /// No bias- This is the core generation function for this <see cref="Random"/>
    /// implementation
    /// </summary>
    /// <returns>An unbiased PRNG value</returns>
    public override uint Next32()
    {
        // Race Condition- Don't use this class in a re-entrant manner if you need stable
        // (repeatable) results

        uint rngState = _state32;

        rngState ^= rngState << 13;
        rngState ^= rngState >> 17;
        rngState ^= rngState << 5;

        return _state32 = rngState;

        // End Race Condition
    }

    /// <summary>
    /// No bias- This is the core generation function for this <see cref="Random"/>
    /// implementation
    /// </summary>
    /// <returns>An unbiased PRNG value</returns>
    public override ulong Next64()
    {
        // Race Condition- Don't use this class in a re-entrant manner if you need stable
        // (repeatable) results

        ulong rngState = _state64;

        rngState ^= rngState << 13;
        rngState ^= rngState >> 7;
        rngState ^= rngState << 17;

        return _state64 = rngState;

        // End Race Condition
    }

    /// <summary>
    /// Implements this feature, but only as part of this specific class. i.e. this would not be
    /// available via the base class <see cref="rng"/> . For <see cref="Rng"/> to be able to use
    /// the "star" version, use the class <see cref="XorShiftStar"/> .
    /// </summary>
    /// <returns></returns>
    public ulong Next64Star() => Next64() * 0x2545f4914f6cdd1dUL;
}


public class XorShiftStar : XorShift
{
    /// <summary>
    /// Construct using default seeding
    /// </summary>
    public XorShiftStar() : base() { }

    /// <summary>
    /// Construct using specific seed
    /// </summary>
    public XorShiftStar( ulong initialState ) : base( initialState ) { }

    /// <summary>
    /// No bias- This is the core generation function for this <see cref="Random"/>
    /// implementation
    /// </summary>
    /// <returns>An unbiased PRNG value</returns>
    public override uint Next32() => (uint)Next64();

    /// <summary>
    /// No bias- This is the core generation function for this <see cref="Random"/>
    /// implementation
    /// </summary>
    /// <returns>An unbiased PRNG value</returns>
    public override ulong Next64() => Next64Star();
}
