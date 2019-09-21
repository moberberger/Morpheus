using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Morpheus
{
    using _Getter = Func<object, object>;
    using _Setter = Action<object, object>;

    /// <summary>
    /// This class allows extremely fast access to both Fields and Properties on an arbitrary
    /// object. This provides both GET and SET operations on the members.
    /// </summary>
    /// <remarks>
    /// Using this class's <see cref="CreateAccessors(MemberInfo)"/> method directly will result
    /// in slightly faster access times to the field or property than using an instantiated
    /// object of this class.
    /// </remarks>
    public class PropertyOrFieldProxy
    {
        /// <summary>
        /// The <see cref="MemberInfo"/> used to create this object
        /// </summary>
        public MemberInfo MemberInfo { get; private set; }

        /// <summary>
        /// The <see cref="MemberInfo"/> used to create this object, as a
        /// <see cref="FieldInfo"/>
        /// </summary>
        public FieldInfo FieldInfo => MemberInfo as FieldInfo;

        /// <summary>
        /// The <see cref="MemberInfo"/> used to create this object, as a
        /// <see cref="PropertyInfo"/>
        /// </summary>
        public PropertyInfo PropertyInfo => MemberInfo as PropertyInfo;


        /// <summary>
        /// The delegate for getting the value of the member
        /// </summary>
        protected _Getter m_getter;

        /// <summary>
        /// The delegate for setting the value of the member
        /// </summary>
        protected _Setter m_setter;



        /// <summary>
        /// Allow subclass to set things up for itself
        /// </summary>
        protected PropertyOrFieldProxy() { }

        /// <summary>
        /// Create a new proxy for a field identified by a <see cref="FieldInfo"/> object.
        /// </summary>
        /// <param name="_fieldInfo">
        /// The <see cref="FieldInfo"/> object used to construct this proxy
        /// </param>
        public PropertyOrFieldProxy( FieldInfo _fieldInfo )
        {
            Init( _fieldInfo );
        }

        /// <summary>
        /// Create a new proxy for a property identified by a <see cref="PropertyInfo"/> object.
        /// </summary>
        /// <param name="_propertyInfo">
        /// The <see cref="PropertyInfo"/> object used to construct this proxy
        /// </param>
        public PropertyOrFieldProxy( PropertyInfo _propertyInfo )
        {
            Init( _propertyInfo );
        }

        /// <summary>
        /// Initialize using either MemberInfo
        /// </summary>
        /// <param name="_member"></param>
        protected void Init( MemberInfo _member )
        {
            MemberInfo = _member;
            (m_getter, m_setter) = CreateAccessors( _member );
        }

        /// <summary>
        /// Retrieve the value of the field or property from the supplied object.
        /// </summary>
        /// <param name="_instanceObject">
        /// The instance object (the "this") used to retrieve the field or property value from
        /// </param>
        /// <returns>
        /// The value of the field or property on the _instanceObject provided
        /// </returns>
        /// <remarks>
        /// <code>
        ///class Data
        ///{
        ///    public int fieldPrimitive;
        ///    public string fieldObject;
        ///    public double propertyPrimitive { get; set; }
        ///    public Data propertyObject { get; set; }
        ///}
        ///
        ///
        ///[TestMethod]
        ///public void TestFieldPrimitiveGet()
        ///{
        ///    var d = new Data { fieldPrimitive = 7 };
        ///    var fi = typeof( Data ).GetField( "fieldPrimitive" );
        ///    var proxy = new PropertyOrFieldProxy( fi );
        ///    var gotten = proxy.Get( d );
        ///
        ///    Assert.AreEqual( d.fieldPrimitive, gotten );
        ///}
        ///
        ///
        ///[TestMethod]
        ///public void TestFieldObjectGet()
        ///{
        ///    var d = new Data { fieldObject = "Hi" };
        ///    var fi = typeof( Data ).GetField( "fieldObject" );
        ///    var proxy = new PropertyOrFieldProxy( fi );
        ///    var gotten = proxy.Get( d );
        ///
        ///    Assert.AreEqual( d.fieldObject, gotten, "Got wrong value" );
        ///}
        /// </code>
        /// </remarks>
        public object Get( object _instanceObject ) => m_getter( _instanceObject );


        /// <summary>
        /// Set the value of a field or property on a supplied instance object.
        /// </summary>
        /// <param name="_instanceObject">
        /// The instance object (the "this") that will have a field/property set to a new value
        /// </param>
        /// <param name="_value">
        /// The new value for the field/property on the supplied _instanceObject
        /// </param>
        /// <remarks>
        /// <code>
        ///class Data
        ///{
        ///    public int fieldPrimitive;
        ///    public string fieldObject;
        ///    public double propertyPrimitive { get; set; }
        ///    public Data propertyObject { get; set; }
        ///}
        ///
        ///
        ///
        ///[TestMethod]
        ///public void TestFieldPrimitiveSet()
        ///{
        ///    int newVal = 69;
        ///
        ///    var d = new Data { fieldPrimitive = 7 };
        ///    var fi = typeof( Data ).GetField( "fieldPrimitive" );
        ///    var proxy = new PropertyOrFieldProxy( fi );
        ///    proxy.Set( d, newVal );
        ///
        ///    Assert.AreEqual( newVal, d.fieldPrimitive );
        ///}
        ///
        ///
        ///[TestMethod]
        ///public void TestFieldObjectSet()
        ///{
        ///    string newVal = "Bye";
        ///
        ///    var d = new Data { fieldObject = "Hi" };
        ///    var fi = typeof( Data ).GetField( "fieldObject" );
        ///    var proxy = new PropertyOrFieldProxy( fi );
        ///    proxy.Set( d, newVal );
        ///
        ///    Assert.AreEqual( newVal, d.fieldObject );
        ///}
        /// </code>
        /// </remarks>
        public void Set( object _instanceObject, object _value ) => m_setter( _instanceObject, _value );


        /// <summary>
        /// Helper to create the Getter and Setter functions.
        /// </summary>
        /// <param name="_member">The <see cref="MemberInfo"/> for the proxy</param>
        /// <returns>A Getter and a Setter lambda for the _member.</returns>
        /// 
        /// <remarks>
        /// <code>
        ///class Data
        ///{
        ///    public int fieldPrimitive;
        ///    public string fieldObject;
        ///    public double propertyPrimitive { get; set; }
        ///    public Data propertyObject { get; set; }
        ///}
        ///
        ///
        ///
        ///[TestMethod]
        ///public void TestCreateAccessors()
        ///{
        ///    string newVal = "Bye";
        ///
        ///    var d = new Data { fieldObject = "Hi" };
        ///    var fi = typeof( Data ).GetField( "fieldObject" );
        ///    (var getter, var setter) = PropertyOrFieldProxy.CreateAccessors( fi );
        ///
        ///    var gotten = getter( d );
        ///    Assert.AreEqual( "Hi", gotten );
        ///
        ///    setter( d, newVal );
        ///    Assert.AreEqual( newVal, d.fieldObject );
        ///}
        ///</code>
        /// <para> <i> NOTE: Using this static method's return value does improve performance
        /// over instantiating a class of this type. </i> </para>
        /// </remarks>
        public static (_Getter, _Setter) CreateAccessors( MemberInfo _member )
        {
            if (_member == null) throw new ArgumentNullException();

            var exObjParam = Expression.Parameter( typeof( object ), "theObject" );
            var exTypedThisParam = Expression.Convert( exObjParam, _member.DeclaringType );

            (var memberType, var exMember) = GetPropertyOrFieldInfo( _member, exTypedThisParam );

            var exNewValueParam = Expression.Parameter( typeof( object ), "newValue" );
            var exNewValueConverted = Expression.Convert( exNewValueParam, memberType );

            var exAssignment = Expression.Assign( exMember, exNewValueConverted );

            var exSetter = Expression.Lambda<_Setter>( exAssignment, exObjParam, exNewValueParam );

            var exReturnValueAsObject = Expression.Convert( exMember, typeof( object ) );
            var exGetter = Expression.Lambda<_Getter>( exReturnValueAsObject, exObjParam );

            var setter = exSetter.Compile();
            var getter = exGetter.Compile();
            return (getter, setter);
        }

        private static (Type, Expression) GetPropertyOrFieldInfo( MemberInfo _member, UnaryExpression _exTypedThisParam )
        {
            switch (_member)
            {
            case FieldInfo fi:
                return (fi.FieldType, Expression.Field( _exTypedThisParam, fi ));
            case PropertyInfo pi:
                return (pi.PropertyType, Expression.Property( _exTypedThisParam, pi ));
            default:
                throw new ArgumentException( $"{_member.Name} is a {_member.MemberType}, but it must be a field or property" );
            }
        }
    }


    /// <summary>
    /// Helper, mainly for testing, as these classes are not really useful if you know the code
    /// structure at compile time.
    /// </summary>
    /// <typeparam name="T">The Type of object to apply proxies to</typeparam>
    /// <remarks>
    /// <code>
    ///class Data
    ///{
    ///    public int fieldPrimitive;
    ///    public string fieldObject;
    ///    public double propertyPrimitive { get; set; }
    ///    public Data propertyObject { get; set; }
    ///}
    ///
    ///
    ///[TestMethod]
    ///public void TestPropertyHelper()
    ///{
    ///    var d = new Data { propertyPrimitive = Math.E };
    ///    var proxy = new PropertyOrFieldProxy&lt;Data>( _x => _x.propertyPrimitive );
    ///    var gotten = proxy.Get( d );
    ///    Assert.AreEqual( d.propertyPrimitive, gotten );
    ///}
    /// </code>
    /// </remarks>
    public class PropertyOrFieldProxy<T> : PropertyOrFieldProxy where T : class
    {
        /// <summary>
        /// Helper constructor taking an Expression which must identify the member to proxy for
        /// </summary>
        /// <param name="_memberExpression">
        /// The <see cref="Expression"/> identifying the field or property
        /// </param>
        /// <remarks>
        /// <code>
        ///class Data
        ///{
        ///    public int fieldPrimitive;
        ///    public string fieldObject;
        ///    public double propertyPrimitive { get; set; }
        ///    public Data propertyObject { get; set; }
        ///}
        ///
        ///
        ///[TestMethod]
        ///public void TestPropertyHelper()
        ///{
        ///    var d = new Data { propertyPrimitive = Math.E };
        ///    var proxy = new PropertyOrFieldProxy&lt;Data>( _x => _x.propertyPrimitive );
        ///    var gotten = proxy.Get( d );
        ///    Assert.AreEqual( d.propertyPrimitive, gotten );
        ///}
        /// </code>
        /// </remarks>
        public PropertyOrFieldProxy( Expression<Func<T, object>> _memberExpression )
        {
            var member = _memberExpression.Body.GetMemberInfo();
            Init( member );
        }
    }
}
