using System.Collections;
using System.ComponentModel;

namespace Morpheus
{
    /// <summary>
    /// This craptastic class makes up for the large deficiency that the base class (<see cref="BindingList&lt;T>"/>) has
    /// in that it won't support binding. CRAP-TASTIC, I tell you!
    /// </summary>
    /// <remarks>
    /// Check out the docs at <see cref="BindingList&lt;T>.ApplySortCore "/> for why I had to do this.
    /// </remarks>
    /// <typeparam name="T">The Type of the elements in the list</typeparam>
    public class CSortableBindingList<T> : BindingList<T>
    {
        /// <summary>
        /// Helper class to make sure that items are dereferenced properly using the <see cref="PropertyDescriptor"/>
        /// </summary>
        /// <typeparam name="TT"></typeparam>
        public class CBindingListComparer<TT> : IComparer<TT>
        {
            /// <summary>
            /// Give me access to the <see cref="PropertyDescriptor"/>
            /// </summary>
            private readonly CSortableBindingList<TT> m_bindingList;

            /// <summary>
            /// Construct with the <see cref="CSortableBindingList&lt;T>"/> that gives me access to the <see cref="PropertyDescriptor"/>
            /// </summary>
            /// <param name="_bindingList">The binding list to use this comparer with</param>
            public CBindingListComparer( CSortableBindingList<TT> _bindingList )
            {
                m_bindingList = _bindingList ?? throw new ArgumentNullException( string.Format( "Not allowed to pass in a null binding list" ) );
            }

            /// <summary>
            /// The comparison operator- dereferences each item using the <see cref="PropertyDescriptor"/>
            /// </summary>
            /// <param name="_first"></param>
            /// <param name="_second"></param>
            /// <returns></returns>
            public int Compare( TT _first, TT _second )
            {
                if (m_bindingList.m_sortProperty != null)
                {
                    var obj1 = (_first == null) ? null : m_bindingList.m_sortProperty.GetValue( _first );
                    var obj2 = (_second == null) ? null : m_bindingList.m_sortProperty.GetValue( _second );

                    var result = Comparer.Default.Compare( obj1, obj2 );
                    if (m_bindingList.m_sortDirection == ListSortDirection.Descending)
                        result = -result;

                    return result;
                }
                else
                {
                    return 0;
                }
            }
        }

        private ListSortDirection m_sortDirection;
        private PropertyDescriptor m_sortProperty;

        /// <summary>
        /// Construct an empty binding list
        /// </summary>
        public CSortableBindingList()
        {
        }

        /// <summary>
        /// Initializes a new instance of the BindingList class with the specified list.
        /// </summary>
        /// <param name="_items">The items in the list</param>
        public CSortableBindingList( IList<T> _items )
            : base( _items )
        {
        }

        /// <summary>
        /// Initializes a new instance of the BindingList class with the specified list.
        /// </summary>
        /// <param name="_items">The items in the list</param>
        public CSortableBindingList( IEnumerable<T> _items )
        {
            foreach (var item in _items)
                Add( item );
        }

        /// <summary>
        /// The direction of the sort (ascending or descending)
        /// </summary>
        protected override ListSortDirection SortDirectionCore => m_sortDirection;

        /// <summary>
        /// From ComponentModel, the property descriptor for the sort property
        /// </summary>
        protected override PropertyDescriptor SortPropertyCore => m_sortProperty;

        /// <summary>
        /// This class always supports sorting
        /// </summary>
        protected override bool SupportsSortingCore => true;

        /// <summary>
        /// If true, the list has been sorted in some way
        /// </summary>
        protected override bool IsSortedCore => m_sortProperty != null;

        /// <summary>
        /// Un-sort the list
        /// </summary>
        protected override void RemoveSortCore() => m_sortProperty = null;

        /// <summary>
        /// Sort the list according to some property 
        /// </summary>
        /// <param name="_property">The property to sort (from ComponentModel)</param>
        /// <param name="_direction">The direction to sort</param>
        protected override void ApplySortCore( PropertyDescriptor _property, ListSortDirection _direction )
        {
            m_sortProperty = _property;
            m_sortDirection = _direction;

            RaiseListChangedEvents = false;
            Items.InsertionSort( new CBindingListComparer<T>( this ) );
            RaiseListChangedEvents = true;
        }
    }

}
