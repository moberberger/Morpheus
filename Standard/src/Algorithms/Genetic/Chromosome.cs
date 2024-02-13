#nullable disable

namespace Morpheus;


/// <summary>
/// A chromosome for use with the <see cref="GeneticAlgorithm"/> class.
/// </summary>
public class Chromosome : IComparable<Chromosome>, IComparable, IEvaluate
{
    /// <summary>
    /// The number of words in this chromosome
    /// </summary>
    public int WordCount { get; }

    /// <summary>
    /// The number of bits per word
    /// </summary>
    public int BitsPerWord { get; }

    /// <summary>
    /// Mark this chromosome as dirty, so it will surely call the evaluator the next time
    /// <see cref="GetValue"/> is called.
    /// </summary>
    public void MarkDirty() => m_dirty = true;

    /// <summary>
    /// The raw data, as 64-bit words which are taken apart.
    /// </summary>
    private readonly ulong[] m_longs;

    /// <summary>
    /// When not dirty, the chromosome will use a cached value.
    /// </summary>
    private bool m_dirty = true;

    /// <summary>
    /// The cached value of the chromosome- Works in conjunction with m_dirty
    /// </summary>
    private double m_cachedValue = double.MaxValue;

    /// <summary>
    /// Calculate the mask once- saves operations in GetWord
    /// </summary>
    private readonly ulong m_mask;

    /// <summary>
    /// The evaluator for the chromosome. Assume that this operation is not fast or trivial,
    /// thus "dirty" and a cached value.
    /// </summary>
    private readonly Func<Chromosome, double> m_evaluator;

    /// <summary>
    /// Construct a raw chromosome, giving it personality.
    /// </summary>
    /// <param name="_wordCount">The number of words in the chromosome</param>
    /// <param name="_bitsPerWord">The number of bits per word</param>
    /// <param name="_evaluator">
    /// The (presumably non-trivial) evaluator for the chromosome
    /// </param>
    public Chromosome( int _wordCount, int _bitsPerWord, Func<Chromosome, double> _evaluator )
    {
        if (_bitsPerWord < 1 || _bitsPerWord > 64)
            throw new ArgumentOutOfRangeException( "Bits Per Word", _bitsPerWord, "Bits Per Word must be between 1 and 64, inclusive" );
        if (_wordCount < 1)
            throw new ArgumentOutOfRangeException( "Word Count", _wordCount, "Word Count must be between positive" );

        m_evaluator = _evaluator ?? throw new ArgumentNullException( "Evaluator", "The Evaluator cannot be NULL" );
        WordCount = _wordCount;
        BitsPerWord = _bitsPerWord;

        var bits = _wordCount * _bitsPerWord;
        var wordCount = (bits + 63) / 64;

        wordCount = Math.Max( 2, wordCount ); // must have 2 words for crossover optimization

        m_longs = new ulong[wordCount];
        m_dirty = true;
        m_mask = 0xffff_ffff_ffff_ffff >> (64 - BitsPerWord);
    }

    /// <summary>
    /// Create using a template chromosome. This is NOT a clone or copy constructor. It does
    /// copy the personality of the chromosome- the word count, bit length of words, and
    /// evaluator.
    /// </summary>
    /// <param name="_other"></param>
    public Chromosome( Chromosome _other )
        : this( _other.WordCount, _other.BitsPerWord, _other.m_evaluator )
    {
    }

    /// <summary>
    /// This will populate all data in this chromosome from the data in another chromosome.
    /// This is a Deep Copy- All data is copied into this chromosome. This allows the GA to
    /// allocate chromosomes once and keep them around, thereby saving on garbage
    /// collections.
    /// </summary>
    /// <param name="_other"></param>
    public void FromOther( Chromosome _other )
    {
        if (m_longs.Length != _other.m_longs.Length)
            throw new InvalidOperationException( "Data Size Mismatch (m_longs.Length)" );

        Array.Copy( _other.m_longs, m_longs, m_longs.Length );
        m_dirty = _other.m_dirty;
        m_cachedValue = _other.m_cachedValue;
    }

    /// <summary>
    /// Get the chromosome's value, based on the evaluator passed in to the constructor.
    /// Will only call the evaluator if this chromosome is "dirty", meaning that we have
    /// reason to believe that the chromosome has changed since the last time the evaluator
    /// was called.
    /// </summary>
    /// <returns></returns>
    public double GetValue()
    {
        if (m_dirty)
        {
            m_cachedValue = m_evaluator( this );
            m_dirty = false;
        }
        return m_cachedValue;
    }



    /// <summary>
    /// Retrieve a word from this chromosome, using WordCount and BitsPerWord
    /// </summary>
    /// <param name="_index">The index into this single-dimensional chromosome</param>
    /// <returns>Up to 64 bits from the chromosome</returns>
    public ulong GetWord( int _index )
    {
        // Bit index of first (right-most) bit
        var bitIdx = _index * BitsPerWord;
        var longIdx1 = bitIdx / 64;
        var longIdx2 = (bitIdx + BitsPerWord - 1) / 64;

        // Doesn't span longs. So if the word is entirely contained within one ulong value,
        // then return the word.
        if (longIdx1 == longIdx2)
        {
            var rightShift = bitIdx - longIdx1 * 64;
            var retval = m_longs[longIdx1];
            retval >>= rightShift;
            retval &= m_mask;
            return retval;
        }
        else // spans longs
        {
            throw new NotImplementedException( "Spanning longs needs to be done" );
        }
    }

    /// <summary>
    /// Retrieve a word from this chromosome, using WordCount and BitsPerWord
    /// </summary>
    /// <param name="_index">The index into this single-dimensional chromosome</param>
    /// <returns>Up to 64 bits from the chromosome</returns>
    public ulong this[int _index] { get => GetWord( _index ); }



    /// <summary>
    /// Get a random 64 bits of data represented by an unsigned long integer
    /// </summary>
    /// <returns>An unsigned long random integer</returns>
    public ulong GetRandom64bits() => Rng.Default.Next64();

    /// <summary>
    /// Place random bits in all of this chromosome
    /// </summary>
    public void Randomize()
    {
        for (var idx = 0; idx < m_longs.Length; idx++)
            m_longs[idx] = GetRandom64bits();

        m_dirty = true;
    }

    /// <summary>
    /// Mutate this chromosome by flipping bits. Will always flip at least one bit. The
    /// Strength parameter represents the probability of flipping subsequent bits.
    /// </summary>
    /// <param name="_strength">
    /// Chance of another bit being flipped each time a bit is flipped- Theoretically could
    /// create an infinite loop. This method enforces a maximum value of 75% chance of
    /// subsequent bit flips.
    /// </param>
    public void Mutate( double _strength )
    {
        _strength = Math.Min( _strength, 0.75 ); // prevent infinite loops

        do
        {
            var idx = Rng.Default.Next( m_longs.Length );
            var bitIdx = Rng.Default.Next( 64 ); // bit index
            var bit = (ulong) 1 << bitIdx;

            m_longs[idx] ^= bit;

        } while (Rng.Default.NextDouble() < _strength);

        m_dirty = true;
    }


    /// <summary>
    /// Crossover by randomly choosing bits from one or the other chromosome.
    /// </summary>
    /// <param name="_1">The first chromosome to get bits from</param>
    /// <param name="_2">The second chromosome to get bits from</param>
    public void CrossoverBits( Chromosome _1, Chromosome _2 )
    {
        for (var i = 0; i < m_longs.Length; i++)
        {
            var mask = GetRandom64bits();
            m_longs[i] = _1.m_longs[i] & mask;

            mask = ~mask;
            m_longs[i] |= _2.m_longs[i] & mask;
        }
    }

    /// <summary>
    /// Crossover two chromosomes by splicing a random section of one chromosome into the
    /// other chromosome. This creates contiguous sequences of bits from one or the other
    /// chromosome.
    /// </summary>
    /// <param name="_1">The first chromosome to splice from</param>
    /// <param name="_2">The second chromosome to splice from</param>
    public void CrossoverSplice( Chromosome _1, Chromosome _2 )
    {
        int idx1 = 0, idx2 = 1;

        if (m_longs.Length > 2)
        {
            do
            {
                idx1 = Rng.Default.Next( m_longs.Length );
                idx2 = Rng.Default.Next( m_longs.Length );
                CMath.Order( ref idx1, ref idx2 );
            } while (idx1 == idx2);

            for (var i = 0; i < m_longs.Length; i++)
            {
                if (i < idx1 || i > idx2)
                    m_longs[i] = _1.m_longs[i];
                else
                    m_longs[i] = _2.m_longs[i];
            }
        }

        m_longs[idx1] = Merge( _1.m_longs[idx1], _2.m_longs[idx1] );
        m_longs[idx2] = Merge( _2.m_longs[idx2], _1.m_longs[idx2] );

        m_dirty = true;
    }

    /// <summary>
    /// Helper for CrossoverSplice- Takes part of a ulong from one chromosome and splices it
    /// with the "other" part of the ulong from the other chromosome
    /// </summary>
    /// <param name="_1">The first ulong to splice from</param>
    /// <param name="_2">The second ulong to splice from</param>
    /// <returns>
    /// A merged ulong containing the left bits from one ulong and the right bits from the
    /// other
    /// </returns>
    private static ulong Merge( ulong _1, ulong _2 )
    {
        var rightShift = Rng.Default.Next( 63 ) + 1;
        var leftShift = 64 - rightShift;

        var x = _1 >> rightShift; // remove low-order bits
        var y = _2 << leftShift; // remove high-order bits
        return (x << rightShift) | (y >> leftShift);
    }



    /// <summary>
    /// Allow chromosomes to be sorted based on their "Value" (from <see cref="GetValue"/> .
    /// </summary>
    /// <param name="_obj">Some other object, presumed to be a chromosome</param>
    /// <returns>
    /// -1 if this is less than obj, 0 if they're equal, or 1 if this is greater than the
    /// other chromosome
    /// </returns>
    public int CompareTo( object _obj ) => CompareTo( _obj as Chromosome );

    /// <summary>
    /// Allow chromosomes to be sorted based on their "Value" (from <see cref="GetValue"/> .
    /// </summary>
    /// <param name="_other">Some other object, presumed to be a chromosome</param>
    /// <returns>
    /// -1 if this is less than obj, 0 if they're equal, or 1 if this is greater than the
    /// other chromosome
    /// </returns>
    public int CompareTo( Chromosome _other )
    {
        if (_other == null)
            return 1;
        if (_other == this)
            return 0;

        var thisVal = GetValue();
        var otherVal = _other.GetValue();

        if (thisVal < otherVal)
            return -1;
        if (thisVal > otherVal)
            return 1;
        return 0;
    }

    /// <summary>
    /// Compare based on content of the bits
    /// </summary>
    /// <param name="_obj">Some other object to compare to this</param>
    /// <returns>TRUE if the bits are equal, FALSE if not</returns>
    public override bool Equals( object _obj )
    {
        if (!(_obj is Chromosome other))
            return false;

        if (m_longs.Length != other.m_longs.Length)
            return false;

        for (var i = 0; i < m_longs.Length; i++)
        {
            if (m_longs[i] != other.m_longs[i])
                return false;
        }
        return true;
    }

    /// <summary>
    /// HashCode can just be the hash code of the longs array
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode() => m_longs.GetHashCode();

    /// <summary>
    /// Turn this chromosome into a string of values.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var digits = (BitsPerWord + 3) / 4;
        var format = "{0:X" + digits + "} ";
        // format = "{0},"; // for decimal instead of Hex

        var str = new StringBuilder();
        for (var i = 0; i < WordCount; i++)
        {
            var x = GetWord( i );
            str.AppendFormat( format, x );
            // if (i % 3 == 2) { str.Length--; str.AppendLine(); }
        }

        return str.ToString();
    }

    /// <summary>
    /// Allow access to the words as an enumeration
    /// </summary>
    public IEnumerable<ulong> Words
    {
        get
        {
            for (var i = 0; i < WordCount; i++)
                yield return GetWord( i );
        }
    }
}
