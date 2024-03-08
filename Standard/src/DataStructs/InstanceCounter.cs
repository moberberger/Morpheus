namespace Morpheus;

public class InstanceCounter<T> : Dictionary<T, long>
    where T : notnull
{
    public void Add( T item )
    {
        TryGetValue( item, out var count );
        this[item] = count + 1;
    }

    public string ToString( bool orderByKey )
    {
        var kvs = orderByKey ? this.OrderBy( kvp => kvp.Key ) : this.OrderByDescending( kvp => kvp.Value );

        var sb = new StringBuilder();
        foreach (var kv in kvs)
            sb.AppendLine( $"{kv.Key} : {kv.Value:N0}" );
        return sb.ToString();
    }

    public override string ToString() => ToString( false );
}
