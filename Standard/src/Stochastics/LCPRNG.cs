namespace Morpheus;


/// <summary>
/// Abstract base, needing only "a" and "c" a/c (multiplier and increment)
/// 
/// Maybe refactor to make non-abstract with a constructor- but bad a/c means really bad
/// mismatch to expectations. So for now I'm forcing a level of purpose when setting a/c
/// </summary>
/// <remarks>
/// Technically should be called "ACPRNG". there's the acknowledgement.
/// </remarks>
public abstract class LCPRNG : Rng
{
    /// <summary>
    /// The "a" coefficient
    /// </summary>
    private readonly ulong _multiplier;

    /// <summary>
    /// The "c" coefficient
    /// </summary>
    private readonly ulong _increment;

    /// <summary>
    /// The current state of the LCPRNG- also the last value returned by
    /// <see cref="Next64"/>
    /// </summary>
    private ulong _state;

    /// <summary>
    /// Maybe consider removing the "abstract" and making this public
    /// </summary>
    /// <param name="multiplier"></param>
    /// <param name="increment"></param>
    protected LCPRNG( ulong multiplier, ulong increment )
    {
        _multiplier = multiplier;
        _increment = increment;
        _state = RandomSeed.FastULong();
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

        var x = _state;
        x *= _multiplier;
        x += _increment;
        return _state = x;

        // End Race Condition
    }
}
