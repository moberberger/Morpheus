using System.Reflection;

namespace Morpheus;


/// <summary>
/// This class provides a mechanism for a member-by-member copy of one object to another.
/// This is a "shallow copy" of all public fields and properties from one object to another,
/// regardless of the Type of the source + destination objects.
/// </summary>
public class CObjectCopier
{
    private static readonly Dictionary<Type, Dictionary<string, MemberInfo>> sm_lookup =
        new Dictionary<Type, Dictionary<string, MemberInfo>>();

    /// <summary>
    /// Find the dictionary of members for a given type, including subtypes. Find all public
    /// fields and public properties.
    /// </summary>
    /// <param name="_type">The Type to find the data for</param>
    /// <returns>A dictionary of member-names to members for a given Type</returns>
    public static Dictionary<string, MemberInfo> GetMemberLookup( Type _type )
    {
        if (sm_lookup.TryGetValue( _type, out var retval ))
            return retval;

        retval = new Dictionary<string, MemberInfo>();

        if (_type.BaseType != typeof( object ) && _type.BaseType is not null) // this is NOT a direct inheritance of System.Object
        {
            var baseFields = GetMemberLookup( _type.BaseType );
            foreach (var fi in baseFields.Values)
            {
                retval[fi.Name] = fi;
            }
        }

        foreach (MemberInfo fi in
                _type.GetFields( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance ))
        {
            retval[fi.Name] = fi;
        }

        foreach (MemberInfo fi in
                _type.GetProperties( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance ))
        {
            retval[fi.Name] = fi;
        }

        sm_lookup[_type] = retval;
        return retval;
    }

    /// <summary>
    /// Copy, member-by-member, data from one object to another. Any missing members on
    /// either side are ignored.
    /// </summary>
    /// <param name="_destination">The object that will receive the data</param>
    /// <param name="_source">The object that will provide the data</param>
    public static void CopyValues( object _destination, object _source )
    {
        var sfields = GetMemberLookup( _source.GetType() );
        var dfields = GetMemberLookup( _destination.GetType() );

        // for all destination members
        foreach (var kv in dfields)
        {
            var dmi = kv.Value;

            if (!sfields.TryGetValue( kv.Key, out var smi ))
                continue; // Skip everything if the source doesn't have a corresponding member matching the name

            object? val;

            // Get the source value, if we can. If not, "continue" through the loop
            if (smi is FieldInfo fi)
            {
                val = fi.GetValue( _source );
            }
            else if (smi is PropertyInfo pi &&
                     pi.GetGetMethod( false ) != null)
            {
                val = pi.GetValue( _source, null );
            }
            else
            {
                continue; // There's no valid source value, so don't bother going any further
            }

            // Set the destination member, if possible.
            if (dmi is FieldInfo fi2)
            {
                fi2.SetValue( _destination, val );
            }
            else if (dmi is PropertyInfo pi2 &&
                     pi2.GetSetMethod( false ) != null)
            {
                pi2.SetValue( _destination, val, null );
            }
        }
    }

    public static T Copy<T>( T source ) where T : new()
    {
        var retval = new T();
        CopyValues( retval, source ?? throw new ArgumentNullException( "source" ) );
        return retval;
    }
}
