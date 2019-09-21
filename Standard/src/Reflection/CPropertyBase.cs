using System;
using System.Collections.Generic;
using System.Reflection;

namespace Morpheus
{
    /// <summary>
    /// This is the base class for CProperty. It contains the guts required for hooking up
    /// notifications.
    /// </summary>
    public abstract class CPropertyBase
    {
        /// <summary>
        /// This is the delegate that notifications must match.
        /// </summary>
        /// <param name="_object">The object containing the property that's changing</param>
        /// <param name="_propertyName">The name of the property (CFixM_ is applied)</param>
        /// <param name="_previousValue">The value of the property before changing</param>
        /// <param name="_newValue">The new value to be assigned to the property</param>
        public delegate void DPropertyChange( object _object, string _propertyName, object _previousValue, object _newValue );


        private object m_parentObject;
        private string m_propertyName;
        private event DPropertyChange OnPropertyChange;


        /// <summary>
        /// Called to register change notification handlers with the object. This method will
        /// register the change handler with ALL CProperty fields in the object.
        /// </summary>
        /// <param name="_object">
        /// The object containing presumably 1 or more CProperty fields
        /// </param>
        /// <param name="_changeHandler">
        /// The handler to be registered with each CProperty field in the object
        /// </param>
        public static void RegisterChangeHandler( object _object, DPropertyChange _changeHandler )
        {
            if (_object == null)
                throw new ArgumentNullException( "Must specify an object" );

            foreach (var prop in GetCPropertiesOnObject( _object ))
            {
                prop.OnPropertyChange += _changeHandler;
                var x = prop.OnPropertyChange.GetInvocationList();
            }
        }

        /// <summary>
        /// Called to remove change notification handlers from the object.
        /// </summary>
        /// <param name="_object">
        /// The object containing presumably 1 or more CProperty fields
        /// </param>
        /// <param name="_changeHandler">
        /// The handler to be removed from each CProperty field in the object
        /// </param>
        public static void RemoveChangeHandler( object _object, DPropertyChange _changeHandler )
        {
            if (_object == null)
                throw new ArgumentNullException( "Must specify an object" );

            foreach (var prop in GetCPropertiesOnObject( _object ))
            {
                prop.OnPropertyChange -= _changeHandler;
            }
        }

        /// <summary>
        /// For a given object, return a CPropertyBase for each CProperty found on that object's
        /// Type
        /// </summary>
        /// <param name="_object">The object to search</param>
        /// <returns></returns>
        private static IEnumerable<CPropertyBase> GetCPropertiesOnObject( object _object )
        {
            var typ = _object.GetType();
            var propType = typeof( CPropertyBase );

            foreach (var fi in typ.GetFields( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public ))
            {
                if (propType.IsAssignableFrom( fi.FieldType ))
                {
                    var prop = (CPropertyBase) fi.GetValue( _object );
                    if (prop == null) // shouldn't be the case, but...
                    {
                        prop = (CPropertyBase) Activator.CreateInstance( fi.FieldType );
                        fi.SetValue( _object, prop );
                    }
                    prop.m_parentObject = _object;
                    prop.m_propertyName = CFixM_.ConvertName( fi.Name );
                    yield return prop;
                }
            }
        }

        /// <summary>
        /// Called by CPropery (which inherits from this) to signal a value change
        /// </summary>
        /// <param name="_previous">The previous value for the property</param>
        /// <param name="_new">The value that the property WILL have</param>
        protected void SignalPropertyChanged( object _previous, object _new ) => OnPropertyChange?.Invoke( m_parentObject, m_propertyName, _previous, _new );
    }
}
