using System;
using System.Collections.Generic;
using System.Reflection;

namespace Morpheus
{
    /// <summary>
    /// This class provides a mechanism for a field-by-field copy from one object to another. This class will attempt to copy all
    /// fields in an object, regardless of public/private status. Check out CObjectCopier for a copier that obeys public/private
    /// conventions AND handles fields + properties.
    /// </summary>
    public class CFieldCopier
    {
        private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> sm_lookup =
            new Dictionary<Type, Dictionary<string, FieldInfo>>();

        /// <summary>
        /// Find the dictionary of fields for a given type, including subtypes. Find all public and private instance fields, but no
        /// static fields
        /// </summary>
        /// <param name="_type">The Type to find the data for</param>
        /// <returns>A dictionary of field-names to fields for a given Type</returns>
        public static Dictionary<string, FieldInfo> GetFieldLookup( Type _type )
        {
            if (sm_lookup.TryGetValue( _type, out var retval ))
                return retval;

            retval = new Dictionary<string, FieldInfo>();

            if (_type.BaseType != typeof( object ))
            {
                var baseFields = GetFieldLookup( _type.BaseType );
                foreach (var fi in baseFields.Values)
                {
                    retval[fi.Name] = fi;
                }
            }

            foreach (
                var fi in
                    _type.GetFields( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic |
                                      BindingFlags.Instance ))
            {
                retval[fi.Name] = fi;
            }

            sm_lookup[_type] = retval;
            return retval;
        }

        /// <summary>
        /// Copy, field-for-field, data from one object to another. Any missing fields on either side are ignored. 
        /// </summary>
        /// <param name="_destination">The object that will receive the data</param>
        /// <param name="_source">The object that will provide the data</param>
        public static void CopyFields( object _destination, object _source )
        {
            var sfields = GetFieldLookup( _source.GetType() );
            var dfields = GetFieldLookup( _destination.GetType() );

            foreach (var kv in dfields)
            {
                var dfi = kv.Value;
                if (sfields.TryGetValue( dfi.Name, out var sfi ) && sfi.FieldType == dfi.FieldType)
                    dfi.SetValue( _destination, sfi.GetValue( _source ) );
            }
        }

        /// <summary>
        /// A generic Clone operation that will do a field-by-field copy of the source object. This will create an
        /// object of the specified type, even if that type is different (a base class of) the source object. 
        /// This is a SHALLOW COPY.
        /// </summary>
        /// <typeparam name="TObject">The type of the object to clone</typeparam>
        /// <param name="_source">The object containing the "source" data.</param>
        /// <returns>an object of the type specified by the generic parameter</returns>
        public static TObject Clone<TObject>( object _source )
        {
            var retval = Activator.CreateInstance<TObject>();
            CopyFields( retval, _source );
            return retval;
        }

        /// <summary>
        /// Clone an object into a new object. This is a SHALLOW COPY.
        /// </summary>
        /// <param name="_source">the object that is to be cloned</param>
        /// <returns>A clone of the specified object.</returns>
        public static object Clone( object _source )
        {
            var retval = Activator.CreateInstance( _source.GetType() );
            CopyFields( retval, _source );
            return retval;
        }
    }
}