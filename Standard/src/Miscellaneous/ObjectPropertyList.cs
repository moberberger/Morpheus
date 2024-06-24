using System.Reflection;

namespace Morpheus;

public class ObjectPropertyList : CSortableBindingList<ObjectPropertyList.Accessor>
{
    public class Order : Attribute
    {
        public int OrderValue { get; }
        /// <summary>
        /// All members with no <see cref="Order"/> attribute are sorted at value 0.
        /// </summary>
        public Order( int orderValue ) => OrderValue = orderValue;
    }

    public class Ignore : Attribute { }

    public class Accessor
    {
        private PropertyInfo PropertyInfo { get; }
        private object TheObject { get; }
        internal int Order { get; } = 0;

        public Accessor( PropertyInfo pi, object theObject )
        {
            PropertyInfo = pi;
            TheObject = theObject;
            var orderAttr = pi.GetCustomAttribute<Order>();
            if (orderAttr != null)
                Order = orderAttr.OrderValue;
        }
        public string Name => PropertyInfo.Name;
        public string Value
        {
            get => PropertyInfo.GetValue( TheObject )?.ToString() ?? "";
            set => PropertyInfo.SetValue( TheObject, Convert.ChangeType( value, PropertyInfo.PropertyType ) );
        }
    }

    public ObjectPropertyList( object obj ) =>
        AddRange(
            obj.GetType()
            .GetProperties( BindingFlags.Public | BindingFlags.Instance )
            .Where( pi => pi.PropertyType.IsPrimitive || pi.PropertyType == typeof( string ) )
            .Where( pi => pi.GetCustomAttribute<Ignore>() is null )
            .Select( pi => new Accessor( pi, obj ) )
            .OrderBy( acc => acc.Order )
        );

    public IEnumerable<IEnumerable<object>> ForGrid() =>
        this.Select( acc => new object[] { acc.Name, acc.Value } );
}