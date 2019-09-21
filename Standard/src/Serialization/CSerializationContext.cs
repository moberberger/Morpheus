using System;
using System.Collections.Generic;

namespace Morpheus
{
    /// <summary>
    /// This class contains all of the information needed to customize the serialization process
    /// </summary>
    /// <remarks>
    /// THREAD SAFETY NOTICE- The Framework's Serializer and Deserializer will treat Context as if it
    /// were a CONSTANT OBJECT- The Framework will never modify any part of the Context object. It is
    /// the application's responsibility to not modify the context while the framework is using it.
    /// 
    /// 
    /// The role of the "Global" context is paramount to understanding how this class is designed.
    /// The goal is to allow an application to EASILY change the way the serialization process handles
    /// any single part of the process without having to create a lot of supporting code.
    /// 
    /// This is a recursive class, in that the application can have contexts that are derived from 
    /// any number of "child contexts", each of which deferring to the level below it until one of
    /// the contexts actually has an application value set.
    /// </remarks>
    public class CSerializationContext
    {
        #region Initialization
        /// <summary>
        /// The Global context is what's used to fall back on for every parameter in the context.
        /// </summary>
        public static CSerializationContext Global = new CSerializationContext( true );

        /// <summary>
        /// The internal inherited context is used to return a value when there is none specifically 
        /// set for the item at this level.
        /// </summary>
        private CSerializationContext m_parent = null;

        /// <summary>
        /// This private constructor is used by the static "Global" field to make sure nothing looks at 
        /// the "parent" member.
        /// </summary>
        /// <param name="_isGlobal">Any value that triggers this constructor override to be used by the
        /// Global field.</param>
        private CSerializationContext( bool _isGlobal )
        {
            ResetToGlobalDefault();
        }

        /// <summary>
        /// Default constructor for a serialization context
        /// </summary>
        public CSerializationContext()
        {
            m_parent = Global;
        }

        /// <summary>
        /// Construct a CSerialization context that inherits values from some other context.
        /// </summary>
        /// <remarks>
        /// This is generally used by the framework, but since an application could use this as well, it is 
        /// marked public rather than internal.
        /// </remarks>
        /// <param name="_parent">The context to inherit</param>
        public CSerializationContext( CSerializationContext _parent )
        {
            m_parent = _parent;
        }
        #endregion

        #region Make Global Context
        /// <summary>
        /// Make this object suitable to be the Global context
        /// </summary>
        public void ResetToGlobalDefault()
        {
            m_fieldRenamer = null;
            m_parent = null;
            ClearExternalSurrogates();
            ResetToDefaults();
        }

        /// <summary>
        /// This method sets all of the compile-time defaults for a context object.
        /// </summary>
        private void ResetToDefaults()
        {
            m_existsFields = EContextFields.ALL;
            m_instanceFields = EContextFields.NONE;

            SetFullNames();
        }

        /// <summary>
        /// Set the predefined names to their full values ("_Root" instead of "_R")
        /// </summary>
        public void SetFullNames()
        {
            m_rootElementName = "_Root";
            m_typeAttributeName = "_Type";
            m_nullAttributeName = "_Null";
            m_nullAttributeValue = "1";
            m_referenceIdAttributeName = "_RefID";
            m_referToAttributeName = "_ReferTo";
            m_arrayAttributeName = "_Array";
            m_arrayElementName = "_Element";
            m_arrayIndexAttributeName = "_Index";
            m_arrayLowerBoundAttribute = "_LowerBound";
            m_useEntitySemanticsAttributeName = "_UseEntitySemantics";
        }

        /// <summary>
        /// Set the predefined names to their short values ("_R" instead of "_Root")
        /// </summary>
        public void SetShortNames()
        {
            m_rootElementName = "_R";
            m_typeAttributeName = "_T";
            m_nullAttributeName = "_N";
            m_nullAttributeValue = "1";
            m_referenceIdAttributeName = "_ID";
            m_referToAttributeName = "_RID";
            m_arrayAttributeName = "_A";
            m_arrayElementName = "_";
            m_arrayIndexAttributeName = "_I";
            m_arrayLowerBoundAttribute = "_L";
            m_useEntitySemanticsAttributeName = "_UES";
        }

        /// <summary>
        /// Set values so that the short, concise output is generated
        /// </summary>
        public void SetConcise()
        {
            SetShortNames();
            RemoveNullValuesFromXml = true;
            FixM_ = true;
            ArrayElementsIncludeIndicies = false;
            AllArraysHaveExplicitElements = false;
            DuplicateStringsCanBeReferredTo = true;
            UseFullUtcDateTimeStrings = false;
        }

        /// <summary>
        /// Set values so that the long, verbose output is generated
        /// </summary>
        public void SetVerbose()
        {
            SetFullNames();
            RemoveNullValuesFromXml = false;
            FixM_ = true;
            ArrayElementsIncludeIndicies = true;
            AllArraysHaveExplicitElements = true;
            DuplicateStringsCanBeReferredTo = false;
            UseFullUtcDateTimeStrings = true;
        }
        #endregion

        #region Handle Context Flags
        /// <summary>
        /// Context Fields Flags direct the default serializers how to handle certain features
        /// </summary>
        [Flags]
        public enum EContextFields : ushort
        {
            /// <summary>
            /// No flags
            /// </summary>
            NONE = 0,

            /// <summary>
            /// can be used to save space arbitrarily by the application
            /// </summary>
            REMOVE_NULL_VALUES_FROM_XML = 0X0001,

            /// <summary>
            /// can be used to make output readable by removing the "m_" in front of field names
            /// </summary>
            FIX_M_ = 0X0002,

            /// <summary>
            /// Prevents condensed comma-separated-lists for simple array element types
            /// </summary>
            ARRAYS_WITH_EXPLICIT_ELEMENTS = 0x0004,

            /// <summary>
            /// Adds an attribute with an array index for all array-element nodes
            /// </summary>
            ARRAY_ELEMENTS_INCLUDE_INDICIES = 0x0008,

            /// <summary>
            /// Allows the serializer to "refer to" identical (usually interned) strings.
            /// </summary>
            DU_STRINGS_CAN_BE_REFERRED_TO = 0x0010,

            /// <summary>
            /// All possible flags
            /// </summary>
            ALL = 0xFFFF
        }

        private EContextFields m_instanceFields = EContextFields.NONE;
        private EContextFields m_existsFields = EContextFields.NONE;

        private static bool IsSet( EContextFields _set, EContextFields _field ) => ((ushort) _set & (ushort) _field) != 0;

        /// <summary>
        /// Get a context flag from the context.
        /// </summary>
        /// <param name="_field">The flag to get</param>
        /// <returns>True or False, depending the state of the flag requested</returns>
        internal bool GetFlag( EContextFields _field )
        {
            if (m_parent == null || IsSet( m_existsFields, _field ))
                // return this instance's context IF there's no parent OR the field exists in this context
                return IsSet( m_instanceFields, _field );
            else
                return m_parent.GetFlag( _field );
        }

        internal void SetFlag( EContextFields _field, bool _state )
        {
            m_existsFields |= _field;
            if (_state)
                m_instanceFields |= _field;
            else
                m_instanceFields &= ~_field;
        }

        /// <summary>
        /// Clear all context flags on the context
        /// </summary>
        /// <param name="_flag"></param>
        public void ClearFlag( EContextFields _flag )
        {
            if (this == Global)
                return;

            m_existsFields &= ~_flag;
        }
        #endregion

        #region Named Properties
        /// <summary>
        /// When this is set, any field with a null value will not be placed into the serialized XML
        /// </summary>
        /// <remarks>
        /// Surrogates of all kinds should adhere to this request, but they are not required to.
        /// </remarks>
        public bool RemoveNullValuesFromXml
        {
            get => GetFlag( EContextFields.REMOVE_NULL_VALUES_FROM_XML );
            set => SetFlag( EContextFields.REMOVE_NULL_VALUES_FROM_XML, value );
        }

        /// <summary>
        /// When this property is set, the framework will change all fields that start with "m_" into names that 
        /// do not have the "m_" in front of them. This produces more readable Xml.
        /// </summary>
        /// <remarks>
        /// For example, if a field is named "m_name", the framework will turn this into "Name" by removing the
        /// "m_" and changing the first letter of what's left into an uppercase value. The third character MUST
        /// be a lower-case letter [a-z] in order for this to work.
        /// </remarks>
        public bool FixM_
        {
            get => GetFlag( EContextFields.FIX_M_ );
            set => SetFlag( EContextFields.FIX_M_, value );
        }

        /// <summary>
        /// When TRUE, all Arrays will be serialized with explicit XmlElements for each element in the array.
        /// By default, primitives and strings are serialized with a comma-separated list.
        /// </summary>
        public bool AllArraysHaveExplicitElements
        {
            get => GetFlag( EContextFields.ARRAYS_WITH_EXPLICIT_ELEMENTS );
            set => SetFlag( EContextFields.ARRAYS_WITH_EXPLICIT_ELEMENTS, value );
        }

        /// <summary>
        /// When TRUE, any array element that has its own XmlElement will have the integer-index of the element
        /// included as an attribute on the Array-element's XmlElement.
        /// By Default, array-elements are serialized in order of their position in the array itself. When the 
        /// array is deserialized, the elements are loaded into the array based on their ordinal position in the
        /// child-nodes of the array-node.
        /// </summary>
        public bool ArrayElementsIncludeIndicies
        {
            get => GetFlag( EContextFields.ARRAY_ELEMENTS_INCLUDE_INDICIES );
            set => SetFlag( EContextFields.ARRAY_ELEMENTS_INCLUDE_INDICIES, value );
        }


        /// <summary>
        /// When TRUE, the serializer will recognize strings that are duplicate objects (usually happens with
        /// interned strings) and use a reference to a single version of the string. This may be useful
        /// when there are very large strings being serialized many times.
        /// </summary>
        public bool DuplicateStringsCanBeReferredTo
        {
            get => GetFlag( EContextFields.DU_STRINGS_CAN_BE_REFERRED_TO );
            set => SetFlag( EContextFields.DU_STRINGS_CAN_BE_REFERRED_TO, value );
        }

        /// <summary>
        /// This property is a helper to assign the appropriate IExternalSurrogate to this Context based on the 
        /// application's needs. The application can use "RegisterExternalSurrogate" to achieve the same effect.
        /// </summary>
        /// <remarks>
        /// Since DateTime is part of the framework, the "framework surrogates" aren't typically associated with
        /// a Context object. However, in the case of DateTime, it could very regularly be required that a more
        /// "Readable" form of the DateTime be used for serialization.
        /// </remarks>
        public bool UseFullUtcDateTimeStrings
        {
            get
            {
                var s = GetExternalSurrogate( typeof( DateTime ) );
                if (s == null)
                    // If there was no new external surrogate specified, then the "Default" external surrogate
                    return false; //  comes from the CFramework, and the concise surrogate is default there.

                return s.GetType() == typeof( CUtcDateTimeSurrogate );
            }
            set
            {
                if (value == UseFullUtcDateTimeStrings)
                    return;

                var surrogate = value
                                                   ? (IExternalSurrogate) new CUtcDateTimeSurrogate()
                                                   : new CDateTimeSurrogate();
                RegisterExternalSurrogate( typeof( DateTime ), surrogate );
            }
        }
        #endregion

        #region Field Renamer
        /// <summary>
        /// The field-renamer for this context instance
        /// </summary>
        private IFieldRenamer m_fieldRenamer = null;

        /// <summary>
        /// The Field Renamer for the serialization context
        /// </summary>
        public IFieldRenamer FieldRenamer
        {
            get => m_fieldRenamer ?? (m_parent?.FieldRenamer);
            set => m_fieldRenamer = value;
        }
        #endregion

        #region External Surrogates
        /// <summary>
        /// The dictionary linking serialization surrogates to System.Type's
        /// </summary>
        /// <remarks>
        /// Do not instantiate this unless someone actually registers a surrogate at this context-level.
        /// </remarks>
        private Dictionary<Type, IExternalSurrogate> m_externalSurrogates = null;

        private void ClearExternalSurrogates() => m_externalSurrogates = null;

        /// <summary>
        /// This method will retrieve an external surrogate for a particular type.
        /// </summary>
        /// <param name="_type">The System.Type to look for</param>
        /// <returns>An ISerializationSurrogate that has been registered for that Type, or NULL if none has been registered.</returns>
        public IExternalSurrogate GetExternalSurrogate( Type _type )
        {
            if (_type == null)
            {
                throw new ArgumentNullException( "_type",
                                                 "Must pass in non-null Type for external surrogate processing." );
            }

            if (m_externalSurrogates != null)
            {
                if (m_externalSurrogates.TryGetValue( _type, out var surrogate ))
                    return surrogate;
            }

            if (m_parent == null)
                return null;

            return m_parent.GetExternalSurrogate( _type );
        }

        /// <summary>
        /// Register an external surrogate with this serialization context. External Surrogates take priority over
        /// all other serialization techniques, including implicit surrogates.
        /// </summary>
        /// <param name="_type">The System.Type that is to use this exernal surrogate</param>
        /// <param name="_surrogate">The surrogate to register with the Type</param>
        /// <remarks>
        /// While each Type may only have one External Surrogate registered with it for any particular context, a
        /// particular Type may have different surrogates registered at different "levels" of the Context inheritance
        /// chain.
        /// </remarks>
        public void RegisterExternalSurrogate( Type _type, IExternalSurrogate _surrogate )
        {
            if (_type == null)
            {
                throw new ArgumentNullException( "_type",
                                                 "Must pass in non-null Type for external surrogate processing." );
            }

            if (m_externalSurrogates == null)
                m_externalSurrogates = new Dictionary<Type, IExternalSurrogate>();

            //m_externalSurrogates.Add( _type, _surrogate );
            m_externalSurrogates[_type] = _surrogate;
        }

        /// <summary>
        /// Remove a registered surrogate for a particular Type. This will remove the surrogate ONLY IF it exists for
        /// this specific context- Inherited contexts will NOT be searched for the type.
        /// </summary>
        /// <param name="_type">The Type for which the surrogate needs to be removed</param>
        /// <returns>TRUE if the Type was found, FALSE if it was not.</returns>
        public bool RemoveExternalSurrogate( Type _type )
        {
            if (_type == null)
            {
                throw new ArgumentNullException( "_type",
                                                 "Must pass in non-null Type for external surrogate processing." );
            }

            var retval = false;
            if (m_externalSurrogates != null)
                retval = m_externalSurrogates.Remove( _type );

            return retval;
        }
        #endregion

        #region Constant Strings

        /// <summary>
        /// The name of the root element
        /// </summary>
        public string RootElementName
        {
            get => m_rootElementName ?? m_parent.RootElementName;
            set => m_rootElementName = value;
        }
        private string m_rootElementName = null;


        /// <summary>
        /// The name of the attribute used for the Type of an element
        /// </summary>
        public string TypeAttributeName
        {
            get => m_typeAttributeName ?? m_parent.TypeAttributeName;
            set => m_typeAttributeName = value;
        }
        private string m_typeAttributeName = null;


        /// <summary>
        /// The name used for the attribute signalling a NULL value
        /// </summary>
        public string NullAttributeName
        {
            get => m_nullAttributeName ?? m_parent.NullAttributeName;
            set => m_nullAttributeName = value;
        }
        private string m_nullAttributeName = null;

        /// <summary>
        /// The value to assign to the XML attribute when NULL is indicated
        /// </summary>
        public string NullAttributeValue
        {
            get => m_nullAttributeValue ?? m_parent.NullAttributeValue;
            set => m_nullAttributeValue = value;
        }
        private string m_nullAttributeValue = null;

        /// <summary>
        /// The name of the attribute generated for a Reference ID
        /// </summary>
        public string ReferenceIdAttributeName
        {
            get => m_referenceIdAttributeName ?? m_parent.ReferenceIdAttributeName;
            set => m_referenceIdAttributeName = value;
        }
        private string m_referenceIdAttributeName = null;


        /// <summary>
        /// The name of the attrbute generated for a ReferTo
        /// </summary>
        public string ReferToAttributeName
        {
            get => m_referToAttributeName ?? m_parent.ReferToAttributeName;
            set => m_referToAttributeName = value;
        }
        private string m_referToAttributeName = null;


        /// <summary>
        /// The name of the attrbute generated for an Array Attribute
        /// </summary>
        public string ArrayAttributeName
        {
            get => m_arrayAttributeName ?? m_parent.ArrayAttributeName;
            set => m_arrayAttributeName = value;
        }
        private string m_arrayAttributeName = null;


        /// <summary>
        /// The name of the element generated for an object contained in an array 
        /// </summary>
        public string ArrayElementName
        {
            get => m_arrayElementName ?? m_parent.ArrayElementName;
            set => m_arrayElementName = value;
        }
        private string m_arrayElementName = null;


        /// <summary>
        /// The name of the attribute used for an index within an array
        /// </summary>
        public string ArrayIndexAttributeName
        {
            get => m_arrayIndexAttributeName ?? m_parent.ArrayIndexAttributeName;
            set => m_arrayIndexAttributeName = value;
        }
        private string m_arrayIndexAttributeName = null;

        /// <summary>
        /// The name for the attribute specifed for the lower bound of an array
        /// </summary>
        public string ArrayLowerBoundAttribute
        {
            get => m_arrayLowerBoundAttribute ?? m_parent.ArrayLowerBoundAttribute;
            set => m_arrayLowerBoundAttribute = value;
        }
        private string m_arrayLowerBoundAttribute = null;

        /// <summary>
        /// The name of the Attribute to add to an Element when the serializer finds that it
        /// uses EntitySemantics
        /// </summary>
        public string UseEntitySemanticsAttributeName
        {
            get => m_useEntitySemanticsAttributeName ?? m_parent.UseEntitySemanticsAttributeName;
            set => m_useEntitySemanticsAttributeName = value;
        }
        private string m_useEntitySemanticsAttributeName;
        #endregion
    }
}