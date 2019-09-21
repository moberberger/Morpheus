using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;


namespace Morpheus.Core
{
    /// <summary>
    /// A wrapper around a dynamically created method responsible for doing a shallow-copy of
    /// fields from one object to another object of the same Type. This can be somewhat compared
    /// to the <see cref="object.MemberwiseClone"/> method, except that the application
    /// needs to present both objects (This does NOT create the destination object) and this
    /// class will allow a subset of fields to be copied. Cursory tests have placed this
    /// operation at roughly 6 times faster than MemberwiseClone.
    /// 
    /// TODO: Cache the copier for each Type that is created.
    /// </summary>
    public class CFieldTransfer
    {
        /// <summary>
        /// The dynamically created method that handles the copy operation for this class
        /// </summary>
        private Action<object, object> m_copier;

        /// <summary>
        /// Only allowed to create from the static factory methods, which may cache objects
        /// </summary>
        private CFieldTransfer() { }


        /// <summary>
        /// Create a FieldTransfer object for a particular type
        /// </summary>
        /// <param name="_type">
        /// The Type to create a transfor object for. Will include all instance fields in this
        /// Type and any superclasses, both public and private.
        /// </param>
        /// <returns>A FieldTransfer object for the given Type</returns>
        public static CFieldTransfer CreateFromType( Type _type )
        {
            var ft = new CFieldTransfer();
            var fields = _type.GetAllFields();

            ft.CreateCopier( _type, fields );

            return ft;
        }

        /// <summary>
        /// Create a FieldTransfer object for a particular type
        /// </summary>
        /// <typeparam name="T">
        /// The Type to create a transfor object for. Will include all instance fields in this
        /// Type and any superclasses, both public and private.
        /// </typeparam>
        /// <returns>A FieldTransfer object for the given Type</returns>
        public static CFieldTransfer CreateFromType<T>()
        {
            var ft = new CFieldTransfer();
            var typ = typeof( T );
            var fields = typ.GetAllFields();

            ft.CreateCopier( typ, fields );

            return ft;
        }

        /// <summary>
        /// Create a FieldTransfer for a specific set of fields. The fields can be from
        /// different Types, so long as all Types declaring the given fields are in the same
        /// inheritance chain. The most-derived Type from all presented fields will be used as
        /// the requirement for objects passed to the Transfer method.
        /// 
        /// If the fields are from two separate inheritance chains, an exception will be thrown.
        /// </summary>
        /// <param name="_fields">The fields to transfer between objects</param>
        /// <returns>A FieldTransfer object for the given fields</returns>
        public static CFieldTransfer CreateFromFields( IEnumerable<FieldInfo> _fields )
        {
            var ft = new CFieldTransfer();
            var typ = GetCommonType( _fields );

            ft.CreateCopier( typ, _fields );

            return ft;
        }


        /// <summary>
        /// Create a FieldTransfer for a specific set of Fields all found on one given Type (the
        /// generic parameter is this Type). This allows a type-safe specification of fields if
        /// they are known at compile-time. While the expressions are assuming that the
        /// application will only specify fields on the Type specified, the application could
        /// theoretically specify other fields. Therefore, the same checks for the inheritance
        /// chain that <see cref="CreateFromFields"/> performs will be performed here.
        /// </summary>
        /// <remarks>
        /// Beware that there may be visibility issues for the fields required by the
        /// application when using this form of the factory method. All required fields will
        /// have to be accessible (i.e. likely NOT private) to be used in this method.
        /// </remarks>
        /// <typeparam name="T">
        /// A Type that should include all fields necessary somewhere in the inheritance chain.
        /// </typeparam>
        /// <param name="_expressions">The field descriptions</param>
        /// <returns>A FieldTransfer object for the given fields</returns>
        public static CFieldTransfer CreateFromExpressions<T>( params Expression<Func<T, object>>[] _expressions )
        {
            var fields = new List<FieldInfo>();

            for (var i = 0; i < _expressions.Length; i++)
            {
                var fi = _expressions[i].Body.GetFieldInfo();
                fields.Add( fi );
            }

            return CreateFromFields( fields );
        }


        /// <summary>
        /// Given a Type and an enumeration of fields, create a field transfer method that will
        /// rapidly transfer those given fields between two objects of the given Type
        /// </summary>
        /// <param name="_type">
        /// The Type which includes all the presented Fields somewhere in its hierarchy
        /// </param>
        /// <param name="_fields">
        /// The Fields to transfer between two objects of the given Type
        /// </param>
        private void CreateCopier( Type _type, IEnumerable<FieldInfo> _fields )
        {
            var copier = new DynamicMethod(
                "_DynamicCopier", // Method Name- inconsequential
                typeof( void ), // Return Type
                new Type[] { typeof( object ), typeof( object ) }, // Param Types
                true );   // Dont worry about member visibility rules

            var il = copier.GetILGenerator();

            il.DeclareLocal( _type );
            il.DeclareLocal( _type );

            il.Emit( OpCodes.Ldarg_0 );
            il.Emit( OpCodes.Castclass, _type );
            il.Emit( OpCodes.Stloc_0 );

            il.Emit( OpCodes.Ldarg_1 );
            il.Emit( OpCodes.Castclass, _type );
            il.Emit( OpCodes.Stloc_1 );

            foreach (var fi in _fields)
            {
                il.Emit( OpCodes.Ldloc_0 );
                il.Emit( OpCodes.Ldloc_1 );

                il.Emit( OpCodes.Ldfld, fi );
                il.Emit( OpCodes.Stfld, fi );
            }

            il.Emit( OpCodes.Ret );

            m_copier = (Action<object, object>)
                    copier.CreateDelegate( typeof( Action<object, object> ) );
        }

        /// <summary>
        /// Return the most derived Type found in any of the FieldInfo.DeclaringType values.
        /// Also make sure that each DeclaringType belongs to the SAME INHERITANCE CHAIN. The
        /// Transfer object will be created such that it requires the objects passed to it to be
        /// of (at least) the most derived Type found in the FieldInfo's. It is OK for the
        /// copied objects to be of a more derived Type.
        /// </summary>
        /// <param name="_fields">The fields from which a common Type will be inferred</param>
        /// <returns>The Type that contains all the fields specified</returns>
#pragma warning disable IDE0011 // Add braces
        internal static Type GetCommonType( IEnumerable<FieldInfo> _fields )
        {
            Type mostDerived = null;

            foreach (var fi in _fields)
            {
                var dt = fi.DeclaringType;

                // Get the faster checks done first before enumerating
                if (mostDerived == null)
                    mostDerived = dt; // Most derived not set yet- set it.
                else if (mostDerived == dt)
                    continue; // Most derived IS this declaring type, so nothing
                else if (mostDerived.GetTypesInInheritanceChain( false, false ).Contains( dt ))
                    continue; // Declaring Type is already in most defined's chain, so nothing
                else if (dt.GetTypesInInheritanceChain( false, false ).Contains( mostDerived ))
                    mostDerived = dt; // New most derived Type
                else // There has to be two different inheritance chains
                {
                    throw new InvalidOperationException(
                        string.Format(
                        "For Field '{1}.{0}', the Declaring Type {1} is in a different inheritance chain than Type {2} found earlier in the Field enumeration",
                        fi.Name,
                        fi.DeclaringType.Name,
                        mostDerived.Name )
                    );
                }
            }

            return mostDerived;
        }
#pragma warning restore IDE0011 // Add braces


        /// <summary>
        /// Transfer the configured fields between two objects of the appropriate Type. An
        /// exception will be thrown if the Types are not compatible with the Types specified or
        /// inferred when this transfer object was created.
        /// </summary>
        /// <param name="_destination">
        /// The object to receive Field information. This may be a more-derived Type than the
        /// source.
        /// </param>
        /// <param name="_source">
        /// The object providing the Field information. This object may be more-derived than the
        /// Type specified when this transfer object was created.
        /// </param>
        public void TransferFields( object _destination, object _source ) => m_copier( _destination, _source );


        private static readonly Dictionary<Type, CFieldTransfer> sm_cache = new Dictionary<Type, CFieldTransfer>();


        /// <summary>
        /// Copy the source object to the destination object, using the fields from the Source
        /// object.
        /// </summary>
        /// <param name="_destination">Will receive field information from Source</param>
        /// <param name="_source">Provides field data to Destination</param>
        public static void Copy( object _destination, object _source )
        {
            if (_source.GetType() != _destination.GetType())
                throw new InvalidOperationException( "Use Copy<T> if the Types of the two objects are not identical." );

            Copy( _destination, _source, _source.GetType() );
        }

        /// <summary>
        /// Copy from source to destination using the fields of the specified Type, which must
        /// be compatible with both object types.
        /// </summary>
        /// <typeparam name="T">The Type to use to generate the field list</typeparam>
        /// <param name="_destination">Object to receive data</param>
        /// <param name="_source">Object to provide data</param>
        public static void Copy<T>( object _destination, object _source )
            where T : class => Copy( _destination, _source, typeof( T ) );

        /// <summary>
        /// Copy fields from one object to another object using the field list from the
        /// specified Type. Will cache the field list if its new, and use the cached field list
        /// if it exists already.
        /// </summary>
        /// <param name="_destination">Object to receive data</param>
        /// <param name="_source">Object to provide data</param>
        /// <param name="_type">The Type to use to generate the field list</param>
        public static void Copy( object _destination, object _source, Type _type )
        {

            if (!sm_cache.TryGetValue( _type, out var ft ))
            {
                ft = CreateFromType( _type );
                sm_cache[_type] = ft;
            }

            ft.TransferFields( _destination, _source );
        }

        /// <summary>
        /// Clone an object using data for a specific Type, allowing a partial clone of a
        /// sub-class
        /// </summary>
        /// <typeparam name="T">The Type to use to determine which fields are copied</typeparam>
        /// <param name="_source">
        /// The object containing the source data. Must be of type T or inherited from type T
        /// </param>
        /// <returns>A new object with the fields from type T copied from the source</returns>
        public static T Clone<T>( object _source )
            where T : class, new()
        {
            var retval = new T();
            Copy<T>( retval, _source );
            return retval;
        }
    }
}
