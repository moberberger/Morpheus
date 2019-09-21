using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;


namespace Morpheus
{
    /// <summary>
    /// Helper class with <see cref="System.Reflection"/> based operations
    /// </summary>
    public static class ReflectionExtenstions
    {
        private readonly static Dictionary<string, Type> sm_typeCrossReference = new Dictionary<string, Type>();

        /// <summary>
        /// Helper method to try to resolve a type name.
        /// </summary>
        /// <remarks>
        /// Will try to overcome the inherent limitation of Type.GetType that only looks in
        /// mscorlib and in the current assembly. This will be addressed by iterating through
        /// all loaded assemblies if Type.GetType does not return anything.
        /// </remarks>
        /// <param name="_name">The name to resolve</param>
        /// <returns>
        /// The type associated with the name if it exists, or NULL if no type can be found.
        /// </returns>
        public static Type BetterGetType( string _name )
        {
            var t = Type.GetType( _name );
            if (t != null)
                return t;

            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                t = a.GetType( _name );
                if (t != null)
                    return t;
            }

            var idx = _name.IndexOf( ", Version=" );
            if (idx > 0)
                return BetterGetType( _name.Substring( 0, idx ) );

            return null;
        }

        /// <summary>
        /// Helper method to try to resolve a type name using a combination of a "cache" of
        /// already-found names and the search through all assemblies for a matching type name
        /// </summary>
        /// <remarks>
        /// Because of the order in which assemblies get loaded, the "foreach" loop in
        /// <see cref="BetterGetType(string)"/> usually loops through all the system (microsoft)
        /// assemblies before it gets to your assemblies. As such, this cache was implemented to
        /// "remember" if you've passed in any given type name and to look that name up
        /// immediately before trying to loop through all of the loaded assemblies. This is not
        /// the default behavior because its possible, when doing a lot of "custom" assembly
        /// loading, the application may rely on a new search every time (this argument may be
        /// fallacious).
        /// </remarks>
        /// <param name="_name">
        /// The "full name" of the class, WITHOUT ASSEMBLY QUALIFICATION
        /// </param>
        /// <param name="_useCacheOfTypes">
        /// When TRUE, use the cache before looping through all assemblies to find the type
        /// </param>
        /// <returns>
        /// The type associated with the name if it exists, or NULL if no type can be found.
        /// </returns>
        public static Type BetterGetType( string _name, bool _useCacheOfTypes )
        {
            // Below, check the parameter first, and if its false, then force the looping
            // anyways.
            if (!_useCacheOfTypes || !sm_typeCrossReference.TryGetValue( _name, out var retval ))
            {
                retval = BetterGetType( _name );
                sm_typeCrossReference[_name] = retval;
                // regardless of the flag, calling this method WILL add the info to the cache
            }

            return retval;
        }


        /// <summary>
        /// Get all loaded types that have a specified attribute and that attribute matches a
        /// filter condition
        /// </summary>
        /// <typeparam name="T">The Type of the attribute that is required</typeparam>
        /// <param name="_filter">
        /// The filter to use on the attributes before a Type is returned. If null, then no
        /// filter is applied and all found attributes are returned.
        /// </param>
        /// <returns>
        /// All Types in all loaded assemblies that have the specified attribute that conform to
        /// the spefified filter
        /// </returns>
        public static IEnumerable<Type> GetTypesWithAttribute<T>( Func<T, bool> _filter = null )
        {
            var retval = new List<Type>();

            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var t in a.GetTypes())
                    {
                        foreach (var attr in t.GetCustomAttributes( typeof( T ), false ))
                        {
                            if (_filter == null || _filter( (T) attr ))
                            {
                                retval.Add( t );
                                break;
                            }

                        }
                    }
                }
                catch { }
            }

            return retval;
        }


        /// <summary>
        /// Search all-or-current assembly for all public static functions that are decorated
        /// with a given attribute.
        /// </summary>
        /// <typeparam name="T">The Type of the Attribute to search for</typeparam>
        /// <param name="_allLoadedAssemblies">
        /// TRUE- Look in all loaded assemblies, FALSE- Look only in the caller's assembly
        /// </param>
        /// <returns>
        /// A Dictionary with <see cref="MethodInfo"/> objects as keys and Attributes (of type
        /// T) as values.
        /// </returns>
        public static Dictionary<MethodInfo, T> GetStaticFunctionsWithAttribute<T>( bool _allLoadedAssemblies )
            where T : Attribute
        {
            var assemblies = new List<Assembly>();
            if (_allLoadedAssemblies)
                assemblies.AddRange( AppDomain.CurrentDomain.GetAssemblies() );
            else
                assemblies.Add( Assembly.GetCallingAssembly() );

            var retval = new Dictionary<MethodInfo, T>();

            foreach (var a in assemblies)
            {
                try
                {
                    foreach (var t in a.GetTypes())
                    {
                        foreach (var m in t.GetMethods( BindingFlags.Static | BindingFlags.Public ))
                        {
                            var ca = m.GetCustomAttributes( typeof( T ), false );
                            if (ca.Length > 0)
                                retval[m] = ca[0] as T;
                        }

                    }
                }
                catch { }
            }

            return retval;
        }


        /// <summary>
        /// Find out if a particular member has a specific attribute. Don't use this method if
        /// you need the actual attribute objects- use the GetCustomAttributes method of the
        /// member, as usual.
        /// </summary>
        /// <param name="_member">The member to check</param>
        /// <param name="_attribute">The Attribute that you're looking for</param>
        /// <returns>TRUE if the attribute exists on the member, FALSE if not.</returns>
        public static bool HasAttribute( this MemberInfo _member, Type _attribute )
        {
            var attrs = _member.GetCustomAttributes( _attribute, false );
            return attrs.Length > 0;
        }

        /// <summary>
        /// Find out if a particular member has a specific attribute. Don't use this method if
        /// you need the actual attribute objects- use the GetCustomAttributes method of the
        /// member, as usual.
        /// </summary>
        /// <typeparam name="TAttrType">The Attribute that you're looking for</typeparam>
        /// <param name="_member">The member to check</param>
        /// <returns>TRUE if the attribute exists on the member, FALSE if not</returns>
        public static bool HasAttribute<TAttrType>( this MemberInfo _member )
            where TAttrType : Attribute
        {
            var attrs = _member.GetCustomAttributes( typeof( TAttrType ), false );
            return attrs.Length > 0;
        }

        /// <summary>
        /// Return an attribute associated with a member, or null if no such attribute exists on
        /// the member. Returns the "first" attribute if multiple attributes exist.
        /// </summary>
        /// <typeparam name="TAttrType">The Type of the attribute to find</typeparam>
        /// <param name="_member">The member to look for attributes on</param>
        /// <returns>
        /// NULL if the attribute isn't associated with the member, or the first attribute found
        /// on the member if one is.
        /// </returns>
        public static TAttrType GetSingleAttribute<TAttrType>( this MemberInfo _member )
            where TAttrType : Attribute
        {
            var attrs = _member.GetCustomAttributes( typeof( TAttrType ), false );
            if (attrs.Length == 0)
                return null;
            return attrs[0] as TAttrType;
        }


        /// <summary>
        /// Create a new instance of an object using the constructor parameters specified.
        /// </summary>
        /// <param name="_type">The Type of the object to create</param>
        /// <param name="_params">
        /// The parameters to pass to the constructor. NULL implies the default constructor
        /// </param>
        /// <returns>
        /// An object of the type specified constructed using the parameters specified
        /// </returns>
        public static object CreateWithConstructor( this Type _type, params object[] _params )
        {
            var paramTypes = _params.Select( _o => _o.GetType() ).ToArray();
            var constructor = _type.GetConstructor( paramTypes );

            return constructor.Invoke( _params );
        }



















        /// <summary>
        /// A Helper function that will not only allocate an array of type T, but it will also
        /// construct objects for each element in the array using whatever parameters for the
        /// constructor were specified by the caller. The same rules for constructor parameters
        /// apply here as do for <see cref="CreateWithConstructor"/>
        /// </summary>
        /// <typeparam name="T">
        /// The Type of objects in the array to create- All objects are of this type, so this
        /// can't be an interface or abstract type
        /// </typeparam>
        /// <param name="_size">
        /// The number of elements in the array- All of these elements will be constructed
        /// </param>
        /// <param name="_constructorParams">
        /// A list of parameters which are to be passed to the constructors for all objects in
        /// the array
        /// </param>
        /// <returns>
        /// A new array of type T containing objects of type T constructed using the specified
        /// constructor parameters
        /// </returns>
        public static T[] CreatePopulatedArray<T>( int _size, params object[] _constructorParams )
        {
            var arr = new T[_size];
            var typ = typeof( T );

            for (var i = 0; i < _size; i++)
                arr[i] = (T) CreateWithConstructor( typ, _constructorParams );

            return arr;
        }

        /// <summary>
        /// A Helper function that will not only allocate an array of type T, but it will also
        /// construct objects for each element in the array using whatever constructor function
        /// provided by the caller
        /// </summary>
        /// <typeparam name="T">
        /// The Type of objects in the array to create- All objects are of this type, so this
        /// can't be an interface or abstract type
        /// </typeparam>
        /// <param name="_size">
        /// The number of elements in the array- All of these elements will be constructed
        /// </param>
        /// <param name="_creator">The function used to create new objects of type T</param>
        /// <returns>
        /// A new array of type T containing objects of type T constructed using the specified
        /// creation function
        /// </returns>
        public static T[] CreatePopulatedArray<T>( int _size, Func<T> _creator )
        {
            var arr = new T[_size];

            for (var i = 0; i < _size; i++)
                arr[i] = _creator();

            return arr;
        }

        /// <summary>
        /// A Helper function that will not only allocate an array of type T, but it will also
        /// construct objects for each element in the array using whatever constructor function
        /// provided by the caller
        /// </summary>
        /// <typeparam name="T">
        /// The Type of objects in the array to create- All objects are of this type, so this
        /// can't be an interface or abstract type
        /// </typeparam>
        /// <param name="_size">
        /// The number of elements in the array- All of these elements will be constructed
        /// </param>
        /// <param name="_creator">
        /// The function used to create new objects of type T. The index of the element to be
        /// created is passed to this function.
        /// </param>
        /// <returns>
        /// A new array of type T containing objects of type T constructed using the specified
        /// creation function
        /// </returns>
        public static T[] CreatePopulatedArray<T>( int _size, Func<int, T> _creator )
        {
            var arr = new T[_size];

            for (var i = 0; i < _size; i++)
                arr[i] = _creator( i );

            return arr;
        }





        /// <summary>
        /// Determines if a Type inherits a specific Interface. This handles generic interfaces
        /// correctly, in that passing in typeof( <see cref="IEnumerable{T}"/> ) will check the
        /// Type against any specific versions of <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <param name="_type">The Type to check for the interface.</param>
        /// <param name="_interfaceType">The Type of the Interface to look for</param>
        /// <returns>TRUE if the Type has the specified interface in its heirarchy.</returns>
        public static bool ImplementsInterface( this Type _type, Type _interfaceType )
        {
            if (!_interfaceType.IsInterface)
                throw new ArgumentException( $"Type '{_interfaceType}' is not an Interface." );

            if (_type.IsInterface)
            {
                if (IsInterfaceMatch( _type, _interfaceType ))
                    return true;
            }

            var interfaces = _type.GetInterfaces();
            for (var i = 0; i < interfaces.Length; i++) // much faster than foreach on Arrays
            {
                var _interface = interfaces[i];

                if (IsInterfaceMatch( _interface, _interfaceType ))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Determine if the two types represent matching interfaces. Interfaces match always
        /// when they're the same <see cref="Type"/> . Also, interfaces also match if one is an
        /// un-bound generic and the other is a specific generic.
        /// </summary>
        /// <param name="_interfaceToTest"></param>
        /// <param name="_interfaceType"></param>
        /// <returns></returns>
        public static bool IsInterfaceMatch( Type _interfaceToTest, Type _interfaceType )
        {
            if (_interfaceToTest == _interfaceType)
                return true;

            if (_interfaceType.IsGenericTypeDefinition)
            {
                if (_interfaceToTest.IsGenericType && _interfaceToTest.GetGenericTypeDefinition() == _interfaceType)
                    return true;
            }
            return false;
        }


        /// <summary>
        /// This method will retrieve all fields in the Type's hierarchy, including private
        /// fields found in a superclass that would otherwise be hidden when using the
        /// GetFields() method (even with BindingFlags.FlattenHierarchy). This will return
        /// public and non-public instance fields, but not any static fields.
        /// </summary>
        /// <param name="_type">The Type to return all fields for</param>
        /// <returns>
        /// An enumeration of all fields of a Type, including private fields on superclasses
        /// </returns>
        public static IEnumerable<FieldInfo> GetAllFields( this Type _type )
        {
            var query = from typ in _type.GetTypesInInheritanceChain( true, false )
                        from fi in typ.GetFields( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly )
                        select fi;
            return query;
        }

        /// <summary>
        /// Return an enumeration of the provided Type plus all types considered "BaseTypes"
        /// (SuperClasses) of that Type. The returned values will be most-derived to
        /// least-derived, with "this type" being the first returned value (if requested) and
        /// typeof(object) being the last returned value (if requested)
        /// </summary>
        /// <param name="_type">
        /// The Type that this function will return the inheritance chain for
        /// </param>
        /// <param name="_includeThisType">
        /// If TRUE, then _type will be returned first. If FALSE, then _type.BaseType will be
        /// return first
        /// </param>
        /// <param name="_includeSystemObject">
        /// If TRUE, then typeof(System.Object) will be returned as the last element. If FALSE,
        /// then the Type derived directly from System.Object is returned last.
        /// </param>
        /// <returns>
        /// An enumeration of Types in a Type's inheritance chain, in order of the most-derived
        /// to the least-derived Types.
        /// </returns>
        public static IEnumerable<Type> GetTypesInInheritanceChain( this Type _type, bool _includeThisType = true, bool _includeSystemObject = false )
        {
            if (_type != typeof( object ))
            {
                if (_includeThisType)
                    yield return _type;

                for (var typ = _type.BaseType; typ != typeof( object ); typ = typ.BaseType)
                    yield return typ;
            }

            if (_includeSystemObject)
                yield return typeof( object );
        }







        /// <summary>
        /// Gets MemberInfo even if the referenced member is a primitive value nested inside a
        /// UnaryExpression which is a Convert operation.
        /// </summary>
        /// <param name="_body">
        /// The Expression Body which is evaluated as a Member expression
        /// </param>
        /// <returns>
        /// A FieldInfo or PropertyInfo representing the member described in the expression.
        /// </returns>
        public static MemberInfo GetMemberInfo( this Expression _body )
        {
            var memberExpr = _body as MemberExpression;

            if (_body is UnaryExpression)
            {
                var unaryBody = _body as UnaryExpression;
                if (unaryBody.NodeType != ExpressionType.Convert)
                    throw new ArgumentException( "A Non-Convert Unary Expression was found." );

                memberExpr = unaryBody.Operand as MemberExpression;
                if (memberExpr == null)
                    throw new ArgumentException( "The target of the Convert operation was not a MemberExpression." );
            }
            else if (memberExpr == null)
            {
                throw new ArgumentException( "The Expression must identify a single member." );
            }

            var member = memberExpr.Member;
            if (!(member is FieldInfo || member is PropertyInfo))
                throw new ArgumentException( "The member specified was not a Field or Property: " + member.GetType() );

            return memberExpr.Member;
        }

        /// <summary>
        /// Wrapper around GetMemberInfo that assures a Field is returned
        /// </summary>
        /// <param name="_body">The expression identifying a field</param>
        /// <returns>A FieldInfo object for the identified field</returns>
        public static FieldInfo GetFieldInfo( this Expression _body )
        {
            var member = _body.GetMemberInfo();
            if (!(member is FieldInfo))
                throw new ArgumentException( "The specified member is not a Field: " + member.GetType() );

            return member as FieldInfo;
        }

        /// <summary>
        /// Wrapper around GetMemberInfo that assures a Property is returned
        /// </summary>
        /// <param name="_body">The expression identifying a property</param>
        /// <returns>A PropertyInfo object for the identified property</returns>
        public static PropertyInfo GetPropertyInfo( this Expression _body )
        {
            var member = _body.GetMemberInfo();
            if (!(member is PropertyInfo))
                throw new ArgumentException( "The specified member is not a Property: " + member.GetType() );

            return member as PropertyInfo;
        }

        /// <summary>
        /// Get a MemberInfo object for an expression. Allows the expression to be constructed
        /// as a parameter to this method.
        /// </summary>
        /// <typeparam name="T">
        /// The Type of the object declaring the interesting field or property
        /// </typeparam>
        /// <param name="_expr">An expression identifying a member on type T</param>
        /// <returns>A MemberInfo object for the identified member</returns>
        public static MemberInfo GetMemberInfo<T>( Expression<Func<T, object>> _expr ) => _expr.Body.GetMemberInfo();

        /// <summary>
        /// Get a FieldInfo object for an expression. Allows the expression to be constructed as
        /// a parameter to this method.
        /// </summary>
        /// <typeparam name="T">
        /// The Type of the object declaring the interesting field
        /// </typeparam>
        /// <param name="_expr">An expression identifying a field on type T</param>
        /// <returns>A FieldInfo object for the identified field</returns>
        public static FieldInfo GetFieldInfo<T>( Expression<Func<T, object>> _expr ) => _expr.Body.GetFieldInfo();

        /// <summary>
        /// Get a PropertyInfo object for an expression. Allows the expression to be constructed
        /// as a parameter to this method.
        /// </summary>
        /// <typeparam name="T">
        /// The Type of the object declaring the interesting property
        /// </typeparam>
        /// <param name="_expr">An expression identifying a property on type T</param>
        /// <returns>A PropertyInfo object for the identified property</returns>
        public static PropertyInfo GetPropertyInfo<T>( Expression<Func<T, object>> _expr ) => _expr.Body.GetPropertyInfo();



        /// <summary>
        /// Given what's assumed to be an individual getter or setter method for a property,
        /// figure out what the actual <see cref="PropertyInfo"/> object is for the property
        /// that the accessor belongs to
        /// </summary>
        /// <param name="_method">The actual get or set method to analyse</param>
        /// <returns>
        /// NULL if the parameter is not a get or set method, or the <see cref="PropertyInfo"/>
        /// object if it is.
        /// </returns>
        public static PropertyInfo GetPropertyInfo( this MethodBase _method )
        {
            var method = _method as MethodInfo;
            if (method == null) return null;

            var takesArg = method.GetParameters().Length == 1;
            var hasReturn = method.ReturnType != typeof( void );
            if (takesArg == hasReturn) return null;
            if (takesArg) // takesArg -> SET operation
            {
                return method.DeclaringType.GetProperties().FirstOrDefault( _prop => _prop.GetSetMethod() == method );
            }
            else // hasReturn -> GET operation
            {
                return method.DeclaringType.GetProperties().FirstOrDefault( _prop => _prop.GetGetMethod() == method );
            }
        }


        /// <summary>
        /// Set the value of a property or field on an object using the name of the member
        /// </summary>
        /// <param name="_object">The object containing the member to set</param>
        /// <param name="_memberName">The name of the member to set</param>
        /// <param name="_value">The value to assign to the member</param>
        /// <param name="_includeFields">If TRUE, then fields will be searched</param>
        /// <param name="_includeProperties">If TRUE, then properties will be searched</param>
        /// <param name="_includePrivate">
        /// If TRUE, then access modifiers (public/private/etc) will be ignored.
        /// </param>
        /// <returns>
        /// TRUE if the value was set, FALSE if there was no member with the specified name.
        /// </returns>
        public static bool SetMemberValue( this object _object, string _memberName, object _value, bool _includeFields = true, bool _includeProperties = true, bool _includePrivate = false )
        {
            if (_object == null)
                throw new ArgumentNullException( "_object" );
            if (string.IsNullOrEmpty( _memberName ))
                throw new ArgumentNullException( "_memberName" );

            var typ = _object.GetType();
            var bindFlag = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
            if (_includePrivate)
                bindFlag |= BindingFlags.NonPublic;

            if (_includeFields)
            {
                var fi = typ.GetField( _memberName, bindFlag );
                if (fi != null)
                {
                    var cnvVal = Convert.ChangeType( _value, fi.FieldType );
                    fi.SetValue( _object, cnvVal );
                    return true;
                }
            }

            if (_includeProperties)
            {
                var pi = typ.GetProperty( _memberName, bindFlag );
                if (pi != null)
                {
                    var cnvVal = Convert.ChangeType( _value, pi.PropertyType );
                    pi.SetValue( _object, cnvVal, null );
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// A meta-class (sentinel) for describing a specific type of situation
        /// </summary>
        public class MemberNotFound { }

        /// <summary>
        /// The sentinel value used to denote that a member is not found
        /// </summary>
        public static MemberNotFound MEMBER_NOT_FOUND { get; } = new MemberNotFound();

        /// <summary>
        /// Get the value of a property or field on an object using the name of the member
        /// </summary>
        /// <param name="_object">The object containing the member to get</param>
        /// <param name="_memberName">The name of the member to get</param>
        /// <param name="_includeFields">If TRUE, then fields will be searched</param>
        /// <param name="_includeProperties">If TRUE, then properties will be searched</param>
        /// <param name="_includePrivate">
        /// If TRUE, then access modifiers (public/private/etc) will be ignored.
        /// </param>
        /// <returns>
        /// The value of the member specified, or <see cref="MEMBER_NOT_FOUND"/> if there was no
        /// member on the object
        /// </returns>
        public static object GetMemberValue( this object _object, string _memberName, bool _includeFields = true, bool _includeProperties = true, bool _includePrivate = false )
        {
            if (_object == null)
                throw new ArgumentNullException( "_object" );
            if (string.IsNullOrEmpty( _memberName ))
                throw new ArgumentNullException( "_memberName" );

            var typ = _object.GetType();
            var bindFlag = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
            if (_includePrivate)
                bindFlag |= BindingFlags.NonPublic;

            if (_includeFields)
            {
                var fi = typ.GetField( _memberName, bindFlag );
                if (fi != null)
                {
                    return fi.GetValue( _object );
                }
            }

            if (_includeProperties)
            {
                var pi = typ.GetProperty( _memberName, bindFlag );
                if (pi != null)
                {
                    return pi.GetValue( _object, null );
                }
            }

            return MEMBER_NOT_FOUND;
        }


    }
}
