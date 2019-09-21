using System;

namespace Morpheus
{
    /// <summary>
    /// This class deals with the deserialization process and the need to assocate a the 
    /// current working-object of a deserialization as soon as that object is created. This is 
    /// because of the needs of resolving multiple-references to the same object.
    /// </summary>
    public class CWorkingObject
    {
        /// <summary>
        /// The Working Object for the instance.
        /// </summary>
        public object WorkingObject { get; private set; }

        /// <summary>
        /// Used in Surrogate Deserialization to understand which superclass of a Type is being
        /// deserialized. Useful when default deserialization happens for a subclass, but 
        /// surrogate deserialization occurs in a base class.
        /// </summary>
        public Type WorkingType { get; set; }

        /// <summary>
        /// The deserializer that is in control of the deserialzation dealing with this working 
        /// object
        /// </summary>
        private CDeserializer m_deserializer = null;

        /// <summary>
        /// The refID that is to be associated with the working object when it is set
        /// </summary>
        private string m_refId = null;

        /// <summary>
        /// Return TRUE if the current working object is referencing something, FALSE if 
        /// it is NULL
        /// </summary>
        public bool IsSet => WorkingObject != null;

        /// <summary>
        /// Set the working object to some new object. This may only be called once for a 
        /// workingObject instance
        /// </summary>
        /// <param name="_object">The object that is to be the new working object</param>
        public void Set( object _object )
        {
            // If the existing working object is not null, then throw exception. It is 
            //  not allowable to re-set the working object.
            if (WorkingObject != null)
            {
                throw new InvalidOperationException(
                    "The Working Object of a Deserialization has already been created for the given Xml element." );
            }

            // Set the instance's current working object.
            WorkingObject = _object;

            // Before anything else is done, make sure that the Deserializer knows about 
            //  this new object
            UpdateDeserializer();
        }

        /// <summary>
        /// Update the Deserializer's reference table with the information found in this 
        /// working object.
        /// </summary>
        private void UpdateDeserializer()
        {
            if (m_deserializer != null && m_refId != null)
                m_deserializer.SetObjectRefId( WorkingObject, m_refId );
        }

        /// <summary>
        /// Used by the framework when it realizes that the working object needs to be 
        /// assocated with a reference ID
        /// </summary>
        /// <param name="_deserializer">The instance of the deserializer working on this 
        /// object</param>
        /// <param name="_refId">The ID to be associated with the object WHEN it is 
        /// assigned.</param>
        internal void SetRefInfo( CDeserializer _deserializer, string _refId )
        {
            m_deserializer = _deserializer;
            m_refId = _refId;

            if (WorkingObject != null)
                UpdateDeserializer();
        }

        /// <summary>
        /// Use this method in your deserializer when you don't really care if the 
        /// WorkingObject has been created yet or if it needs to be created. This method will 
        /// check for a previously created object, and if it doesn't exist, it will create a 
        /// new object using a parameter-less constructor.
        /// </summary>
        /// <typeparam name="TObjectType">The Type of the object that the surrogate is 
        /// expecting</typeparam>
        /// <returns>A non-NULL object reference cast to the Type specified</returns>
        /// <exception cref="InvalidCastException">Thrown if there is an existing working 
        /// object, but that object cannot be cast to the Type specified.</exception>
        public TObjectType GetExistingOrCreateNew<TObjectType>() where TObjectType : new()
        {
            if (WorkingObject != null)
                return (TObjectType) WorkingObject;

            var newObject = new TObjectType();
            Set( newObject );
            return newObject;
        }

        /// <summary>
        /// Use this method in your deserializer when you don't really care if the 
        /// WorkingObject has been created yet or if it needs to be created. This method will 
        /// check for a previously created object, and if it doesn't exist, it will create a 
        /// new object using a parameter-less constructor.
        /// </summary>
        /// <param name="_objectType">The Type of the object that the surrogate is expecting
        /// </param>
        /// <returns>A non-NULL object reference cast to the Type specified</returns>
        /// <exception cref="InvalidCastException">Thrown if there is an existing working 
        /// object, but that object cannot be cast to the Type specified.</exception>
        public object GetExistingOrCreateNew( Type _objectType )
        {
            if (WorkingObject != null)
                return WorkingObject;

            var newObject = Activator.CreateInstance( _objectType );
            Set( newObject );
            return newObject;
        }

        /// <summary>
        /// Helper function that returns the existing workingObject pre-cast to the
        /// specified Type.
        /// </summary>
        /// <typeparam name="TObjectType">
        /// The Type to pre-cast the working object to
        /// </typeparam>
        /// <returns>The Working Object pre-cast to the specified Type.</returns>
        public TObjectType GetWorkingObject<TObjectType>() => (TObjectType) WorkingObject;
    }
}