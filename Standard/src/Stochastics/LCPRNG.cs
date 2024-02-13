namespace Morpheus;


/// <summary>
/// Abstract base requiring multiplier and increment values along with optional
/// seed.
/// </summary>
public abstract class LCPRNG : Rng
{
    /// <summary>
    /// The "a" coefficient in state = a * state + c
    /// </summary>
    private readonly ulong _multiplier;

    /// <summary>
    /// The "c" coefficient in state = a * state + c
    /// </summary>
    private readonly ulong _increment;

    /// <summary>
    /// The current state of the LCPRNG- also the last value returned by
    /// <see cref="Next64"/>
    /// </summary>
    public ulong State { get; private set; }


    /// <summary>
    /// Called by inheriting class to set multiplier and increment using an
    /// internally generated really good, fast, but not crypto-secure random
    /// seed
    /// </summary>
    protected LCPRNG( ulong multiplier, ulong increment )
        : this( multiplier, increment, RandomSeed.FastULong() ) { }

    /// <summary>
    /// Called by inheriting class to set multiplier and increment and initial
    /// seed value
    /// </summary>
    protected LCPRNG( ulong multiplier, ulong increment, ulong seed )
    {
        _multiplier = multiplier;
        _increment = increment;
        State = seed;
    }

    /// <summary>
    /// No bias- This is the core generation function for any Linear
    /// Congruential PRNG implementation. This is not re-entrant- wrap your
    /// <see cref="Rng"/> in a <see cref="SynchronizedRng"/> or call
    /// rng.AsSynchronized()
    /// </summary>
    /// <returns>An unbiased PRNG value</returns>
    public override ulong Next64()
    {
        var x = State;
        x *= _multiplier;
        x += _increment;
        return State = x;
    }
}
