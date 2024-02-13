namespace Morpheus;


/// <summary>
/// This Rng implementation is not random at all. It simply returns the bytes
/// from a pre-defined byte array. This is useful for testing purposes or when
/// you have a large set of random information that doesn't need to be generated
/// on the fly.
/// </summary>
public class ByteArrayRng : Rng
{
    private readonly byte[] _buffer;
    private bool _wrapAround;

    public int Index { get; set; } = 0;
    public int Length => _buffer.Length;
    public int Remaining => _buffer.Length - Index;

    /// <summary>
    /// Construct using a byte array. Optionally, indicate how to decode
    /// <see cref="double"/> values.
    /// </summary>
    /// <param name="bytes">The byte array forming the basis for</param>
    /// If TRUE, then read
    /// <see cref="double"/> values as if they came from
    /// <see cref="BinaryWriter"/> </param>
    public ByteArrayRng( byte[] bytes, bool wrapAround ) => (_buffer, _wrapAround) = (bytes, wrapAround);


    private void CheckForWraparound( int requestLength )
    {
        if (Remaining < requestLength)
            if (_wrapAround)
                Index = 0;
            else
                throw new InvalidOperationException( "Not enough bytes in the buffer" );
    }

    /// <summary>
    /// Remove enough bytes from the underlying stream to fill the buffer passed
    /// in.
    /// </summary>
    /// <param name="_buffer">
    /// The buffer to fill with bytes from the stream
    /// </param>
    public override void NextBytes( Span<byte> destination )
    {
        CheckForWraparound( destination.Length );

        for (int i = 0; i < destination.Length; i++)
            destination[i] = _buffer[Index++];
    }

    /// <summary>
    /// Use <see cref="BitConverter.ToUInt32(byte[], int)"/> to convert the next 4 bytes"/>
    /// </summary>
    /// <returns></returns>
    public override uint Next32()
    {
        CheckForWraparound( 4 );
        var retval = BitConverter.ToUInt32( _buffer, Index );
        Index += 4;
        return retval;
    }

    /// <summary>
    /// Use <see cref="BitConverter.ToUInt64(byte[], int)"/> to convert the next 8 bytes"/>
    /// </summary>
    /// <returns></returns>
    public override ulong Next64()
    {
        CheckForWraparound( 8 );
        var retval = BitConverter.ToUInt64( _buffer, Index );
        Index += 8;
        return retval;
    }
}
