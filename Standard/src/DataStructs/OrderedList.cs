using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morpheus
{
    /// <summary>
    /// An Ordered List. Uses the Generic List's operations of BinarySearch and Sort to maintain
    /// order, according to the required IComparable implementation for the Type.
    /// </summary>
    /// <typeparam name="T">Any Type that implements IComparable</typeparam>
    public class OrderedList<T> : IList<T>
        where T : IComparable<T>
    {
        private readonly List<T> m_list;

        /// <summary>
        /// Underlying Capacity of the List object.
        /// </summary>
        public int Capacity => m_list.Capacity;

        /// <summary>
        /// Create an empty ordered list
        /// </summary>
        public OrderedList()
        {
            m_list = new List<T>();
        }

        /// <summary>
        /// Create an empty ordered list, but with a pre-allocated memory block
        /// </summary>
        /// <param name="_initialSize">The amount of memory to pre-allocate</param>
        public OrderedList( int _initialSize )
        {
            m_list = new List<T>( _initialSize );
        }

        /// <summary>
        /// Create and initialize an Ordered List with a set of elements. The OrderedList will
        /// contain those elements, but in sorted order.
        /// </summary>
        /// <param name="_elements">The elements to initialize the list with</param>
        public OrderedList( IEnumerable<T> _elements )
        {
            m_list = new List<T>( _elements );
            m_list.Sort();
        }



        /// <summary>
        /// The index of a specific element within the list. The IComparable{T} implementation
        /// will be used to detect equal elements.
        /// </summary>
        /// <param name="_item">
        /// The Item to search for. Only those fields used by the IComparable{T} implementation
        /// need to be populated. This is NOT a reference-equals check.
        /// </param>
        /// <returns>
        /// The Index of the Element in the List, or the ones-complement of the index where the
        /// element should be inserted IF the element does not already exist in the list.
        /// 
        /// Note that this is slightly different from the List{T} implementation in that it will
        /// only return -1 if the item is not found, where this implementation will return a
        /// negative number, but not necessarily -1
        /// </returns>
        public int IndexOf( T _item ) => m_list.BinarySearch( _item );

        /// <summary>
        /// Insert At Index --- NOT IMPLEMENTED as it is likely to perturb sorting order
        /// </summary>
        /// <param name="_index">N/A</param>
        /// <param name="_item">N/A</param>
        public void Insert( int _index, T _item ) => throw new InvalidOperationException( "Not allowed to INSERT into an ordered list." );

        /// <summary>
        /// Remove an element at a given index.
        /// </summary>
        /// <param name="_index">The Index to remove the element at</param>
        public void RemoveAt( int _index ) => m_list.RemoveAt( _index );

        /// <summary>
        /// Access an element at a given index. The SETTER is not implemented, as it is likely
        /// to perturb the ordering of elements in the list.
        /// </summary>
        /// <param name="_index">The index of the element to look up</param>
        /// <returns>The Element at the specified index</returns>
        public T this[int _index]
        {
            get => m_list[_index];
            set => throw new InvalidOperationException( "Not allowed to replace an element at a specific index in an ordered list." );
        }

        /// <summary>
        /// Add an item to an ordered list
        /// </summary>
        /// <param name="_item">The item to add</param>
        public void Add( T _item )
        {
            var idx = m_list.BinarySearch( _item );
            if (idx < 0)
                m_list.Insert( ~idx, _item );
            else
                m_list.Insert( idx, _item );
        }

        /// <summary>
        /// Remove all elements from this ordered list
        /// </summary>
        public void Clear() => m_list.Clear();

        /// <summary>
        /// Check to see if the list contains an element. This is NOT a reference-equals check.
        /// It uses IComparable == 0 to determine sameness
        /// </summary>
        /// <param name="_item">The item to check the list for</param>
        /// <returns>TRUE if the element exists in the list</returns>
        public bool Contains( T _item ) => m_list.BinarySearch( _item ) >= 0;

        /// <summary>
        /// Copy the ordered list (or part of it) to an array
        /// </summary>
        /// <param name="_array">The Array to copy the elements to</param>
        /// <param name="_index">The starting index within the destination array</param>
        public void CopyTo( T[] _array, int _index ) => m_list.CopyTo( _array, _index );

        /// <summary>
        /// The number of elements in the ordered list
        /// </summary>
        public int Count => m_list.Count;

        /// <summary>
        /// Always False- Ordered Lists don't have read-only versions (yet)
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Remove an Item from the ordered list based on the IComparable implementation
        /// </summary>
        /// <param name="_item">The item to remove from the list</param>
        /// <returns>TRUE if the item was removed, FALSE if it was not found</returns>
        public bool Remove( T _item )
        {
            var idx = m_list.BinarySearch( _item );
            if (idx < 0)
                return false;

            RemoveAt( idx );
            return true;
        }

        /// <summary>
        /// Get an enumeration of the items in this list. This is an ordered enumeration.
        /// </summary>
        /// <returns>
        /// An enumeration of the items in this list. This is an ordered enumeration.
        /// </returns>
        public IEnumerator<T> GetEnumerator() => m_list.GetEnumerator();

        /// <summary>
        /// Get an enumeration of the items in this list. This is an ordered enumeration.
        /// </summary>
        /// <returns>
        /// An enumeration of the items in this list. This is an ordered enumeration.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => m_list.GetEnumerator();

        /// <summary>
        /// Return an array perfectly sized to hold all of the elements of this list, in order
        /// </summary>
        /// <returns></returns>
        public T[] ToArray()
        {
            var retval = new T[m_list.Count];
            CopyTo( retval, 0 );
            return retval;
        }
    }
}
