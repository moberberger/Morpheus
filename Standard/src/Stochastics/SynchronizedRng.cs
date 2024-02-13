namespace Morpheus;

public class SynchronizedRng : Rng
{
    private readonly Rng m_rng;
    private object m_next64Lock = new object();
    private object m_nextBytesLock = new object();

    public SynchronizedRng( Rng rng ) => m_rng = rng;

    public override ulong Next64()
    {
        lock (m_next64Lock)
        {
            return m_rng.Next64();
        }
    }

    public override void NextBytes( Span<byte> buffer )
    {
        lock (m_nextBytesLock)
        {
            m_rng.NextBytes( buffer );
        }
    }
}
