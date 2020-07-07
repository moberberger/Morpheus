using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace Morpheus
{
    /// <summary>
    /// This <see cref="IDictionary{TKey, TValue}"/> implementation allows the read-only
    /// encapsulation of another IDictionary to provide a value for a key if this dictionary
    /// doesn't contain a value for said key.
    /// </summary>
    public class EncapsulatingDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        #region Support for Interface Implementation and Class
        /// <summary>
        /// The dictionary representing this instance
        /// </summary>
        private Dictionary<TKey, TValue> m_dictionary = new Dictionary<TKey, TValue>();

        /// <summary>
        /// If m_dictionary doesn't contain a given TKey value, then this dictionary will be
        /// consulted.
        /// </summary>
        private IDictionary<TKey, TValue> m_encapsulatedDictionary = null;


        /// <summary>
        /// Construct with or without encapsulating another
        /// <see cref="IDictionary{TKey,TValue}"/> .
        /// </summary>
        /// <param name="toEncapsulate">The IDictionary to encapsulate</param>
        public EncapsulatingDictionary( IDictionary<TKey, TValue> toEncapsulate = null )
        {
            m_encapsulatedDictionary = toEncapsulate;
        }

        /// <summary>
        /// Internal IEnumerable method for getting all of "this" level's objects, PLUS any
        /// encapsulated dictionary's objects that DO NOT SHARE KEYS with this object's
        /// dictionary.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<KeyValuePair<TKey, TValue>> GetObjects()
        {
            foreach (var obj in m_dictionary)
                yield return obj;

            if (m_encapsulatedDictionary != null)
                foreach (var obj in m_encapsulatedDictionary)
                    if (!m_dictionary.ContainsKey( obj.Key ))
                        yield return obj;
        }

        /// <summary>
        /// How many objects in this dictionary PLUS how many objects in encapsulated
        /// dictionary(s) that are NOT in this dictionary.
        /// </summary>
        public int DeepCount { get => GetObjects().Sum( _ => 1 ); }

        /// <summary>
        /// The number of objects in this dictionary EXCLUSIVE OF any encapsulated dictionaries
        /// </summary>
        public int ShallowCount { get => m_dictionary.Count; }


        /// <summary>
        /// Does this EncapsulatingDictionary contain a key, without regard to any encapsulated
        /// dictionaries?
        /// </summary>
        /// <param name="key">The key to look for</param>
        /// <returns>
        /// TRUE -iff- the key exists in -this- dictionary REGARDLESS of its presence in an
        /// Encapsulated dictionary.
        /// </returns>
        public bool ContainsKeyShallow( TKey key ) => m_dictionary.ContainsKey( key );

        /// <summary>
        /// Return all EncapsulatingDictionary's being encapsulated in this object's history
        /// </summary>
        /// <returns>
        /// Enumeration in order from "highest" (this!) level EncapsulatingDictionary to the
        /// "lowest"
        /// </returns>
        public IEnumerable<IDictionary<TKey, TValue>> Trace()
        {
            for (IDictionary<TKey, TValue> current = this;
                current != null;
                current = (current as EncapsulatingDictionary<TKey, TValue>)?.m_encapsulatedDictionary)
            {
                yield return current;
            }
        }

        /// <summary>
        /// Return all objects through an EncapsulatingDictionary's history for a given key
        /// value
        /// </summary>
        /// <param name="key">The key to look for</param>
        /// <returns>
        /// Enumeration in order from "highest" level object for the key through to the lowest
        /// level's object for the key
        /// </returns>
        public IEnumerable<IDictionary<TKey, TValue>> TraceKey( TKey key ) =>
            Trace().Where( d =>
                (d as EncapsulatingDictionary<TKey, TValue>)?.ContainsKeyShallow( key )
                ?? d.ContainsKey( key ) );

        /// <summary>
        /// How many encapsulated layers are there?
        /// </summary>
        public int Depth { get => Trace().Count(); }

        #endregion



        #region IDictionary Implementation


        /// <summary>
        /// If the TKey value does not exist in either this nor the encapsulated IDictionary,
        /// then one of those better throw a <see cref="KeyNotFoundException"/>
        /// </summary>
        /// <param name="key">What to look for</param>
        /// <exception cref="KeyNotFoundException">
        /// When the requested key is not found
        /// </exception>
        /// <returns>The value for the key, if it exists</returns>
        public TValue this[TKey key]
        {
            get
            {
                if (m_encapsulatedDictionary == null)
                    return m_dictionary[key];

                if (m_dictionary.TryGetValue( key, out var retval ))
                    return retval;

                return m_encapsulatedDictionary[key];
            }
            set => m_dictionary[key] = value;
        }

        /// <summary>
        /// Add a new element to the dictionary, replacing a value if it already exists at this
        /// encapsulation level or superceding a value if it already exists in the encapsulated
        /// dictionary.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add( TKey key, TValue value ) => m_dictionary.Add( key, value );

        /// <summary>
        /// Add a new element to the dictionary, replacing a value if it already exists at this
        /// encapsulation level or superceding a value if it already exists in the encapsulated
        /// dictionary.
        /// </summary>
        /// <param name="item"></param>
        public void Add( KeyValuePair<TKey, TValue> item ) => m_dictionary.Add( item.Key, item.Value );

        /// <summary>
        /// Clear this dictionary- has no effect on any encapsulated dictionary.
        /// </summary>
        public void Clear() => m_dictionary.Clear();

        /// <summary>
        /// donut use
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains( KeyValuePair<TKey, TValue> item ) => m_dictionary.ContainsKey( item.Key ) || (m_encapsulatedDictionary?.ContainsKey( item.Key ) ?? false);

        /// <summary>
        /// Returns TRUE if the specified key exists in the dictionary or any encapsulated
        /// dictionaries
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>
        /// TRUE if the specified key exists in the dictionary or any encapsulated dictionaries
        /// </returns>
        public bool ContainsKey( TKey key ) => m_dictionary.ContainsKey( key ) || (m_encapsulatedDictionary?.ContainsKey( key ) ?? false);

        /// <summary>
        /// Remove an element from -this- dictionary, but leaves encapsulated dictionaries
        /// alone. This does not "mask" an encapsulated dictionary entry.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>TRUE if an element was removed, FALSE if not</returns>
        public bool Remove( TKey key ) => m_dictionary.Remove( key );

        /// <summary>
        /// donut use
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove( KeyValuePair<TKey, TValue> item ) => m_dictionary.Remove( item.Key );

        /// <summary>
        /// If a value for specified key exists in this or any encapsulated dictionaries, return
        /// TRUE and set the output parameter to the value associated with the key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue( TKey key, out TValue value ) => m_dictionary.TryGetValue( key, out value ) || (m_encapsulatedDictionary?.TryGetValue( key, out value ) ?? false);

        /// <summary>
        /// Copy ALL elements of this dictionary and encapsulated dictionaries to an Array
        /// beginning at a specified index.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo( KeyValuePair<TKey, TValue>[] array, int arrayIndex )
        {
            foreach (var obj in this)
                array[arrayIndex++] = obj;
        }

        /// <summary>
        /// Pass-through to the enumeration algorithm
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => GetObjects().GetEnumerator();

        /// <summary>
        /// Pass-through to the enumeration algorithm
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => GetObjects().GetEnumerator();


        /// <summary>
        /// Returns a list of the Keys in all dictionaries
        /// </summary>
        public ICollection<TKey> Keys => GetObjects().Select( x => x.Key ).ToList();

        /// <summary>
        /// Returns a list of the Values in all dictionaries
        /// </summary>
        public ICollection<TValue> Values => GetObjects().Select( x => x.Value ).ToList();

        /// <summary>
        /// The Count of elements in the collection produced by enumerating this object
        /// </summary>
        public int Count => DeepCount;

        /// <summary>
        /// Never read-only
        /// </summary>
        public bool IsReadOnly => false;

        #endregion
    }
}
