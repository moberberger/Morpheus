using System;
using System.Xml;

namespace Morpheus
{
    /// <summary>
    /// This interface is used by the serializer framework in order to provide custom external serialization and 
    /// deserialization for any Types. External serialization is used when an application does not have access to 
    /// the Type's definition.
    /// </summary>
    /// <remarks>
    /// External serializers are generally useful for two things-
    /// 
    /// 1) External Surrogates can be used for serializing classes that an application did not write, but would like to 
    ///     have control over how the class is serialized. Primarily, this will include classes in the .NET Framework
    ///     (mscorlib, etc) or in libraries written by other people.
    /// 2) They can be used if an application wants to serialize a type in a certain way under certain circumstances,
    ///     but still wants the serialization framework to be able to serialize the Type under "normal" circumstances.
    ///     This is accomplished using the <see cref="CSerializationContext"/> class to associate an external surrogate
    ///     with a serializer/deserializer.
    /// </remarks>
    public interface IExternalSurrogate
    {
        /// <summary>
        /// Given an object and a "parent" xml element, add all of the information describing the object (its fields, 
        /// for instance) to the parent element. The implementation of this method should be very cautious about 
        /// modifying the parent element itself- generally the only acceptable thing to do with the parent is to -add-
        /// new attributes and elements. It is VERY DANGEROUS to look past the element itself- The implementation 
        /// should not need to deal with ancestors and/or siblings of the passed-in element.
        /// </summary>
        /// <param name="_object">The object whose data needs to be added to _parentNode</param>
        /// <param name="_useType">The Type of the object to use- Important when applying a surrogate to a base 
        /// class when the subclass didn't have a surrogate</param>
        /// <param name="_parentElement">The "Parent" XmlElement that should recieve _object's data</param>
        /// <param name="_serializer">The serializer instance making this call- used to get current context.</param>
        /// <returns>TRUE if the surrogate was able to completely serialize the object, FALSE if the framework should 
        /// perform its "default" function.</returns>
        bool Serialize( object _object, Type _useType, XmlElement _parentElement, CSerializer _serializer );

        /// <summary>
        /// Given an XmlElement and a "CWorkingObject" instance, try to deserialize the Xml into the working object.
        /// The surrogate may be responsible for object creation- the _workingObject should be checked to see if
        /// an object has already been created (by, for instance, a super-class). All of the information for the 
        /// object should be wholely contained within the XmlElement supplied. The implementation should never need
        /// to "hunt around" the XML DOM for information about the object.
        /// </summary>
        /// <param name="_workingObject">The container for object that is being deserialized. If the "WorkingObject"
        /// property is a NULL value, then the deserializer is expected to create the object and use the 
        /// <see cref="CWorkingObject.Set"/> method to assign the newly created object to the working object. If it 
        /// is NOT a null value, then the deserializer is not allowed to modify this "Working Object" value.</param>
        /// <param name="_parentElement">The XmlElement containing the data to populate the object with</param>
        /// <param name="_deserializer">The CDeserializer instance making this call- This can be used to get the
        /// context information or any other relevent information.</param>
        /// <returns>TRUE if the surrogate was able to completely deserialize the object, FALSE if the framework should 
        /// perform its "default" function.</returns>
        bool Deserialize( CWorkingObject _workingObject, XmlElement _parentElement, CDeserializer _deserializer );
    }
}