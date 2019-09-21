using System.Xml;


namespace Morpheus
{
    /// <summary>
    /// This class can be used to replace a property's backing field. It provides PropertyChanged notifications for the property.
    /// </summary>
    /// <remarks>
    /// There is a typical usage model for using CProperty. The following code within a class will set up properties as notifiable properties:
    /// <code>
    ///public class CTestObjectWithProperties
    ///{
    ///    private CProperty&lt;int> m_age = new CProperty&lt;int>();
    ///    public int Age
    ///    {
    ///        get { return m_age.Value; }
    ///        set { m_age.Value = value; }
    ///    }
    ///    private CProperty&lt;string> m_name = new CProperty&lt;string>( "Default Values Are Fine" );
    ///    public string Name
    ///    {
    ///        get { return m_name.Value; }
    ///        set { m_name.Value = value; }
    ///    }
    ///    private CProperty&lt;EDispatchMode> m_mode = new CProperty&lt;EDispatchMode>();
    ///    public EDispatchMode Mode
    ///    {
    ///        get { return m_mode.Value; }
    ///        set { m_mode.Value = value; }
    ///    }
    ///}
    /// </code>
    /// </remarks>
    /// <typeparam name="T">The Type of the property</typeparam>
    public class CProperty<T> : CPropertyBase
    {
        /// <summary>
        /// Store the value of the property
        /// </summary>
        private T m_value;

        /// <summary>
        /// Default constructor does nothing
        /// </summary>
        public CProperty()
        {
            m_value = default;
        }

        /// <summary>
        /// Constructor with an initializer for the value
        /// </summary>
        /// <param name="_value">The initializer for the value</param>
        public CProperty( T _value )
        {
            m_value = _value;
        }

        /// <summary>
        /// The Value that the CProperty supports.
        /// Getting the value is straight-forward.
        /// Setting the value checks to see if the value has changed, and if it has, invokes the plumbing for handling Property Changed events.
        /// </summary>
        public T Value
        {
            get => m_value;
            set
            {
                if (m_value == null && value == null)
                    return;

                if (m_value == null || value == null || !m_value.Equals( value ))
                {
                    SignalPropertyChanged( m_value, value );
                    m_value = value;
                }
            }
        }

        /// <summary>
        /// Turn the property value into a string.
        /// </summary>
        /// <returns>The string form of the value</returns>
        public override string ToString() => m_value.ToString();

        /// <summary>
        /// Allows an object of this class to be implicitly cast into an object of the Type that this object controls.
        /// </summary>
        /// <param name="_property">The CProperty that is to be converted</param>
        /// <returns>An object of type T, basically the .Value for the object.</returns>
        public static implicit operator T( CProperty<T> _property ) => _property.Value;

        /// <summary>
        /// Given an object of type T, create a new CProperty wrapper for that object
        /// </summary>
        /// <param name="_value">The value to wrap in a CProperty object</param>
        /// <returns>A new CProperty wrapper for the object</returns>
        public static implicit operator CProperty<T>( T _value ) => new CProperty<T>( _value );

        /// <summary>
        /// Serialize the value wrapped by this object. This class should remove itself from any mention in serialization/deserialization
        /// </summary>
        /// <param name="_serializer">The serializer working on this object</param>
        /// <param name="_node">The node which will contain the serialized data</param>
        /// <returns>TRUE always, as this should be the entire serialization process for this object</returns>
        [AImplicitSerializer]
        public bool Serialize( CSerializer _serializer, XmlNode _node )
        {
            _serializer.AddObjectToElement( m_value, _node as XmlElement );
            return true;
        }


        /// <summary>
        /// Deserialize XML into the value wrapped by this CProperty. The XML should contain no mention of the CProperty class.
        /// </summary>
        /// <param name="_node">The node containing the XML serialization of the Value</param>
        /// <param name="_object">The working object which will receive the new CProperty after the XML has been deserialied into the Value</param>
        /// <param name="_framework">The deserializer working on this object</param>
        /// <returns>TRUE always, as this should be the entire deserialization process for this object</returns>
        [AImplicitDeserializer]
        private static bool Deserialize( CDeserializer _framework, XmlElement _node, CWorkingObject _object )
        {
            var newObj = _object.GetExistingOrCreateNew<CProperty<T>>();
            newObj.m_value = (T) _framework.FrameworkDeserialize( _node, typeof( T ) );
            return true;
        }
    }
}
