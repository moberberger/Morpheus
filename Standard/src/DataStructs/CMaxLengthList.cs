using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Morpheus
{
    /// <summary>
    /// A fixed-length list that replaces the "oldest" elements with new elements as they are added.
    /// Keeps the most recently added N elements added.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the list</typeparam>
    [Serializable, DebuggerDisplay( "Current/Max = {Count}/{Capacity}" )]
    public class CMaxLengthList<T> : IList<T>, ICollection<T>, IEnumerable<T>
    {


        int IList<T>.IndexOf( T _item ) => throw new NotImplementedException();

        void IList<T>.Insert( int _index, T _item ) => throw new NotImplementedException();

        void IList<T>.RemoveAt( int _index ) => throw new NotImplementedException();

        T IList<T>.this[int _index]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        void ICollection<T>.Add( T _item ) => throw new NotImplementedException();

        void ICollection<T>.Clear() => throw new NotImplementedException();

        bool ICollection<T>.Contains( T _item ) => throw new NotImplementedException();

        void ICollection<T>.CopyTo( T[] _array, int _arrayIndex ) => throw new NotImplementedException();

        int ICollection<T>.Count => throw new NotImplementedException();

        bool ICollection<T>.IsReadOnly => throw new NotImplementedException();

        bool ICollection<T>.Remove( T _item ) => throw new NotImplementedException();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => throw new NotImplementedException();

        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
    }
}
