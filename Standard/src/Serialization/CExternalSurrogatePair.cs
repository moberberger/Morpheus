using System;
using System.Xml;

namespace Morpheus
{
    /// <summary>
    /// This helper class encapsulates a pair of IExternalSurrogates. When the "Serialize" or
    /// "Deserialize" methods are called, this class will call the first surrogate's Serialize
    /// method and, if it didn't return TRUE, then return the value of the second surrogate's
    /// return value. The same thing happens for the Deserialize operation.
    /// </summary>
    /// <remarks>
    /// While there's no reason not to let any application use this class, the usage model is
    /// tailor-fit to help <see cref="Serialization.CFramework"/> in an optimal fashion. Thus,
    /// if some of the API looks a little weird, that's why.
    /// </remarks>
    public class CExternalSurrogatePair : IExternalSurrogate
    {
        /// <summary>
        /// The first surrogate to be called
        /// </summary>
        public IExternalSurrogate Surrogate1 { get; }

        /// <summary>
        /// The second surrogate to be called
        /// </summary>
        public IExternalSurrogate Surrogate2 { get; }

        /// <summary>
        /// This static method acts as an "accumulator" where, as new surrogates are "found"
        /// that should be called in turn for a given Type, new Pairs are called and connected
        /// to each other.
        /// </summary>
        /// <param name="_working">
        /// The "Working" variable that is being "accumulated into"
        /// </param>
        /// <param name="_next">
        /// The "next" IExternalSurrogate that should be "accumulated into" the working
        /// surrogate
        /// </param>
        /// <returns>
        /// An IExternalSurrogate representing the "current" state of the "accumulator"
        /// </returns>
        public static IExternalSurrogate Update( IExternalSurrogate _working, IExternalSurrogate _next )
        {
            if (_working == null)
                return _next;
            if (_next == null)
                return _working;

            return new CExternalSurrogatePair( _working, _next );
        }

        /// <summary>
        /// Construct the Pair using Two separate surrogates.
        /// </summary>
        /// <param name="_surrogate1">The 1st surrogate</param>
        /// <param name="_surrogate2">The 2nd surrogate</param>
        public CExternalSurrogatePair( IExternalSurrogate _surrogate1, IExternalSurrogate _surrogate2 )
        {
            if (_surrogate1 == null || _surrogate2 == null)
                throw new ArgumentNullException( "Not allowed to construct a SurrogatePair with any NULL surrogates." );

            Surrogate1 = _surrogate1;
            Surrogate2 = _surrogate2;
        }

        /// <summary>
        /// Given an object and a "parent" xml element, add all of the information describing
        /// the object (its fields, for instance) to the parent element. The implementation of
        /// this method should be very cautious about modifying the parent element itself-
        /// generally the only acceptable thing to do with the parent is to -add- new attributes
        /// and elements. It is VERY DANGEROUS to look past the element itself- The
        /// implementation should not need to deal with ancestors and/or siblings of the
        /// passed-in element.
        /// </summary>
        /// <param name="_object">The object whose data needs to be added to _parentNode</param>
        /// <param name="_useType">
        /// Treat the "_object" parameter as if it were of this type
        /// </param>
        /// <param name="_parentElement">
        /// The "Parent" XmlElement that should recieve _object's data
        /// </param>
        /// <param name="_serializer">
        /// The serializer instance making this call- used to get current context.
        /// </param>
        /// <returns>
        /// TRUE if the surrogate was able to completely serialize the object, FALSE if the
        /// framework should perform its "default" function.
        /// </returns>
        public bool Serialize( object _object, Type _useType, XmlElement _parentElement, CSerializer _serializer )
        {
            if (Surrogate1.Serialize( _object, _useType, _parentElement, _serializer ))
                return true;

            return Surrogate2.Serialize( _object, _useType, _parentElement, _serializer );
        }

        /// <summary>
        /// Given an XmlElement and a "CWorkingObject" instance, try to deserialize the Xml into
        /// the working object. The surrogate may be responsible for object creation- the
        /// _workingObject should be checked to see if an object has already been created (by,
        /// for instance, a super-class). All of the information for the object should be
        /// wholely contained within the XmlElement supplied. The implementation should never
        /// need to "hunt around" the XML DOM for information about the object.
        /// </summary>
        /// <param name="_workingObject">
        /// The container for object that is being deserialized. If the "WorkingObject" property
        /// is a NULL value, then the deserializer is expected to create the object and use the
        /// <see cref="CWorkingObject.Set"/> method to assign the newly created object to the
        /// working object. If it is NOT a null value, then the deserializer is not allowed to
        /// modify this "Working Object" value.
        /// </param>
        /// <param name="_parentElement">
        /// The XmlElement containing the data to populate the object with
        /// </param>
        /// <param name="_deserializer">
        /// The CDeserializer instance making this call- This can be used to get the context
        /// information or any other relevent information.
        /// </param>
        /// <returns>
        /// TRUE if the surrogate was able to completely deserialize the object, FALSE if the
        /// framework should perform its "default" function.
        /// </returns>
        public bool Deserialize( CWorkingObject _workingObject, XmlElement _parentElement, CDeserializer _deserializer )
        {
            if (Surrogate1.Deserialize( _workingObject, _parentElement, _deserializer ))
                return true;

            return Surrogate2.Deserialize( _workingObject, _parentElement, _deserializer );
        }
    }
}
