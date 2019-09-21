using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Morpheus.Serialization
{
    /// <summary>
    /// This is the base class for both the serializer and the deserializer in the Morpheus library.
    /// </summary>
    public abstract class CFramework
    {
        #region Pre-constructed Type objects
        /// <summary>
        /// typeof(string)
        /// </summary>
        public static readonly Type TYPEOF_STRING = typeof( string );

        /// <summary>
        /// typeof(IntPtr)
        /// </summary>
        public static readonly Type TYPEOF_INTPTR = typeof( IntPtr );

        /// <summary>
        /// typeof(UIntPtr)
        /// </summary>
        public static readonly Type TYPEOF_UINTPTR = typeof( UIntPtr );

        /// <summary>
        /// typeof(Delegate)
        /// </summary>
        public static readonly Type TYPEOF_DELEGATE = typeof( Delegate );

        /// <summary>
        /// typeof(System.MulticastDelegate)
        /// </summary>
        public static readonly Type TYPEOF_MULTICAST_DELEGATE = typeof( MulticastDelegate );
        #endregion

        /// <summary>
        /// This dictionary contains External Surrogates defined by the Morpheus framework
        /// </summary>
        /// <remarks>
        /// Mainly contains help for Collections, but this is for anything in the .NET framework that needs help with 
        /// serialization. This feature is particularly helpful for framework elements that rely too heavily on OO principals 
        /// instead of Interface-driven design (a perfect example is "synchronized" or "readonly" forms of ArrayList). Since 
        /// there are no parameter-less constructors for those classes that inherited Arraylist, we need to define surrogates 
        /// that can generate those classes properly.
        /// </remarks>
        private static readonly Dictionary<Type, IExternalSurrogate> sm_frameworkSurrogates = new Dictionary<Type, IExternalSurrogate>();

        /// <summary>
        /// The static constructor sets up the external surrogates for framework classes.
        /// </summary>
        static CFramework()
        {
            sm_frameworkSurrogates.Add( typeof( Guid ), new CGuidSurrogate() );
            sm_frameworkSurrogates.Add( typeof( DateTime ), new CDateTimeSurrogate() );

            sm_frameworkSurrogates.Add( typeof( ArrayList ), new CArrayListSurrogate() );
            sm_frameworkSurrogates.Add( CSyncArrayListSurrogate.SyncArrayListType, new CSyncArrayListSurrogate() );
            sm_frameworkSurrogates.Add( CReadOnlyArrayListSurrogate.ReadOnlyArrayListType, new CReadOnlyArrayListSurrogate() );

            sm_frameworkSurrogates.Add( typeof( Queue ), new CQueueSurrogate() );
            sm_frameworkSurrogates.Add( CSyncQueueSurrogate.SyncQueueType, new CSyncQueueSurrogate() );

            sm_frameworkSurrogates.Add( typeof( Stack ), new CStackSurrogate() );
            sm_frameworkSurrogates.Add( CSyncStackSurrogate.SyncStackType, new CSyncStackSurrogate() );

            sm_frameworkSurrogates.Add( typeof( Hashtable ), new CHashtableSurrogate() );
            sm_frameworkSurrogates.Add( CSyncHashtableSurrogate.SyncHashtableType, new CSyncHashtableSurrogate() );

            sm_frameworkSurrogates.Add( typeof( SortedList ), new CSortedListSurrogate() );
            sm_frameworkSurrogates.Add( CSyncSortedListSurrogate.SyncSortedListType, new CSyncSortedListSurrogate() );

            sm_frameworkSurrogates.Add( typeof( List<> ), new CListSurrogate() );
            sm_frameworkSurrogates.Add( typeof( LinkedList<> ), new CLinkedListSurrogate() );
            sm_frameworkSurrogates.Add( typeof( Queue<> ), new CGenQueueSurrogate() );
            sm_frameworkSurrogates.Add( typeof( Stack<> ), new CGenStackSurrogate() );
            sm_frameworkSurrogates.Add( typeof( Dictionary<,> ), new CDictionarySurrogate() );
            sm_frameworkSurrogates.Add( typeof( SortedDictionary<,> ), new CDictionarySurrogate() );
            sm_frameworkSurrogates.Add( typeof( SortedList<,> ), new CDictionarySurrogate() );
            sm_frameworkSurrogates.Add( typeof( HashSet<> ), new CHashSetSurrogate() );
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The context for serialization / deserialization
        /// </summary>
        protected CSerializationContext m_context = null;

        /// <summary>
        /// This is used by surrogates to tell the framework to ignore certain fields when handling a class. If any surrogates
        /// are defined for the class, the surrogates may or may not ignore these ignored fields. Specifically, surrogates
        /// are the only way to actually declare fields as "ignored" fields.
        /// </summary>
        /// <remarks>
        /// We do not allocate an actual List for these fields until a surrogate actually declares an ignored field.
        /// This is because ignored fields should probably be pretty rare.
        /// </remarks>
        private List<string> m_ignoredFields;

        /// <summary>
        /// This stack contains the "vertical history" of the fields starting at the top of the object tree heading down
        /// to the current field being processed at whatever depth in the tree. The stack is used because any surrogate
        /// will always be interested in the "last-in" value.
        /// </summary>
        private readonly Stack<CFieldData> m_fieldStack = new Stack<CFieldData>();

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Construct with a specific context in mind. Use <see cref="CSerializationContext.Global"/> when
        /// the parameter is NULL.
        /// </summary>
        /// <param name="_context">The context for serialization</param>
        public CFramework( CSerializationContext _context )
        {
            m_context = _context ?? CSerializationContext.Global;
        }

        /// <summary>
        /// Construct with the "Global" context.
        /// </summary>
        public CFramework()
        {
            m_context = CSerializationContext.Global;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The context for serialization / deserialization
        /// </summary>
        public CSerializationContext Context => m_context;

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Push a field onto the fieldStack
        /// </summary>
        /// <param name="_field">The field to push onto the stack</param>
        internal void PushCurrentField( CFieldData _field ) =>
            //string indent = new string( ' ', m_fieldStack.Count );
            //Console.WriteLine( indent + "Push Field: " + _field.XmlName );
            m_fieldStack.Push( _field );

        /// <summary>
        /// Remove the top field from the fieldStack
        /// </summary>
        protected void PopField()
        {
            var fd = m_fieldStack.Pop();
            //string indent = new string( ' ', m_fieldStack.Count );
            //Console.WriteLine( indent + "Pop Field: " + fd.XmlName );
        }

        /// <summary>
        /// Get the "Top" field on the fieldStack, or NULL if the fieldStack is empty
        /// </summary>
        internal CFieldData TopField => m_fieldStack.Count == 0 ? null : m_fieldStack.Peek();

        /// <summary>
        /// Use context renamers to get the appropriate name for a field.
        /// </summary>
        /// <param name="_fieldData">The FieldData describing (in framework terms) this field</param>
        /// <returns>The name that should be used for the field</returns>
        internal string GetFieldName( CFieldData _fieldData )
        {
            var name = _fieldData.XmlName;

            if (m_context.FieldRenamer != null)
                name = m_context.FieldRenamer.ConvertFieldName( name, _fieldData.Field );

            if (m_context.FixM_)
                name = CFixM_.ConvertName( name );

            return name;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Allows the application/surrogate to ask the framework whether or not the object field being
        /// (de)serialized has a particular attribute associated with it.
        /// </summary>
        /// <param name="_attributeType">The Type of the attribute interested in </param>
        /// <returns>TRUE if the field has the attribute, FALSE if it doesn't OR if the surrogate received
        /// a value that is not a field (a top-level value)</returns>
        public bool HasAttribute( Type _attributeType )
        {
            var fd = TopField;
            return (fd == null) ? false : fd.HasAttribute( _attributeType );
        }

        /// <summary>
        /// Allows the application/surrogate to ask the framework for a single instance of an attribute
        /// of the Type specified as the generic-parameter to this generic-function.
        /// </summary>
        /// <typeparam name="TAttributeType">The Type of the attribute requested.</typeparam>
        /// <returns>NULL if there are no attributes of the specified Type, or the First attribute found
        /// if any attribute are present.</returns>
        public TAttributeType GetAttribute<TAttributeType>()
        {
            var fd = TopField;
            return (fd == null) ? default : fd.GetAttribute<TAttributeType>();
        }

        /// <summary>
        /// Allows the application/surrogate to ask the framework for all instances of attributes of
        /// the Type specified as the parameter to this method.
        /// </summary>
        /// <param name="_attributeType">The Type of the attributes interested in</param>
        /// <returns>An array of attributes all of which are of the Type specified.</returns>
        public object[] GetAttributes( Type _attributeType )
        {
            var fd = TopField;
            return fd?.GetAttributes( _attributeType );
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Given an array of strings, convert the array into a comma-separated list. Make sure to properly
        /// process the raw string elements to make sure that commas are "escaped".
        /// </summary>
        /// <param name="_array">The array with string elements in it.</param>
        /// <returns>A string containing all elements in the array as a comma-separated list.</returns>
        public static string ConvertStringArrayToCommaList( Array _array )
        {
            var count = _array.Length;
            var s = new StringBuilder();

            for (var i = 0; i < count; i++)
            {
                var preEscaped = _array.GetValue( i ) as string;
                var escaped = ProtectStringForStringlist( preEscaped );
                s.Append( escaped );
                s.Append( "," );
            }

            if (count > 0)
                s.Length--;

            return s.ToString();
        }

        // TODO: Look at a better way of replacing using, perhaps, StringBuilder?

        /// <summary>
        /// By "Protecting" a string, we are adding an ESCAPE character and "Escaping" both that escape char AND the comma, which is
        /// used only as a list item separator. This also deals with a special "Escape" for empty strings in order to separate them 
        /// from null strings.
        /// </summary>
        /// <param name="_rawString">The string to protect</param>
        /// <returns>A properly escaped string, suitable for inclusion in a comma-separated list of strings.</returns>
        public static string ProtectStringForStringlist( string _rawString )
        {
            if (_rawString == null)
                return @"";
            if (_rawString == "")
                return @"\_";

            var tmpString = _rawString.Replace( @"\", @"\\" );
            return tmpString.Replace( @",", @"\`" );
        }

        /// <summary>
        /// Unprotect the string from the stringList- remove escape sequences
        /// </summary>
        /// <param name="_string">the string to unprotect</param>
        /// <returns>the unprotected string</returns>
        public static string UnprotectStringFromStringlist( string _string )
        {
            if (_string == @"\_")
                return "";
            if (_string == "")
                return null;

            _string = _string.Replace( @"\\", @"\" );
            return _string.Replace( @"\`", @"," );
        }

        /// <summary>
        /// Determine if the Type passed in is valid for serialization. Essentially, pointers or equivalents are not valid.
        /// </summary>
        /// <param name="_type">The System.Type to check</param>
        /// <returns>TRUE if the Type is valid for serialization by the framework, FALSE if not.</returns>
        public static bool IsValidType( Type _type )
        {
            // Delegates never get serialized- it doesn't make sense to pass essentially a function pointer to any other process.
            //if (typeofDelegate.IsAssignableFrom( _type ))
            if (_type.BaseType == TYPEOF_MULTICAST_DELEGATE)
                return false;

            // Pointers are never serialized either.
            if (_type.IsPointer || _type == TYPEOF_INTPTR || _type == TYPEOF_UINTPTR)
                return false;

            return true;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Called by Serialize or Deserialize to initiate state variables for a start-to-finish (de)serialize operation.
        /// </summary>
        protected void StartProcessing()
        {
            m_fieldStack.Clear();
            m_ignoredFields = null;
        }

        /// <summary>
        /// Instruct the framework to ignore a field. Used by surrogates that handle only partial-class serialization. 
        /// The "Ignore Fields" feature of the framework only addresses fields declared in the "current" type- in other words, 
        /// fields declared in sub-classes are NOT ignored.
        /// </summary>
        /// <remarks>
        /// Multiple calls with the same field name will not cause errors, but it will incur a slight overhead, especially if other
        /// field names are ignored after the multiple.
        /// </remarks>
        /// <param name="_fieldName">The name of the field to ignore.</param>
        public void IgnoreField( string _fieldName )
        {
            if (m_ignoredFields == null)
                m_ignoredFields = new List<string>();

            m_ignoredFields.Add( _fieldName );
        }

        /// <summary>
        /// Clear the list of "ignored" fields
        /// </summary>
        protected void ClearIgnoredFields()
        {
            if (m_ignoredFields != null)
                m_ignoredFields.Clear();
        }

        /// <summary>
        /// Determine if a given field name is meant to be ignored.
        /// </summary>
        /// <param name="_fieldName">The FieldName to check for.</param>
        /// <returns>TRUE if a surrogate has requested that the field be ignored, FALSE if not.</returns>
        public bool IsIgnoredField( string _fieldName )
        {
            if (m_ignoredFields == null)
                return false;
            return m_ignoredFields.Contains( _fieldName );
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// This baseclass function allows serializers or deserializers to check for external surrogates for the given Type.
        /// This routine will look everywhere that external surrogates could be found- for now, that's the default Framework
        /// stuff and anything found in the Context.
        /// </summary>
        /// <param name="_type">The Type that we're checking for external surrogates</param>
        /// <returns>An IExternalSurrogate for _type, if anything exists.</returns>
        protected IExternalSurrogate GetExternalSurrogate( Type _type )
        {
            if (_type == null)
                return null;

            var surrogate = m_context.GetExternalSurrogate( _type );

            sm_frameworkSurrogates.TryGetValue( _type, out var predefinedFramework );
            surrogate = CExternalSurrogatePair.Update( surrogate, predefinedFramework );

            if (_type.IsGenericType)
            {
                var genericTypedef = _type.GetGenericTypeDefinition();

                var appDefined = m_context.GetExternalSurrogate( genericTypedef );
                surrogate = CExternalSurrogatePair.Update( surrogate, appDefined );

                sm_frameworkSurrogates.TryGetValue( genericTypedef, out predefinedFramework );
                surrogate = CExternalSurrogatePair.Update( surrogate, predefinedFramework );
            }

            return surrogate;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Used by surrogates that deal with series of elements (arrays, collections) to figure out what to name each element for the
        /// collection
        /// </summary>
        /// <returns>The name to use for each element.</returns>
        public string GetNameForCollectionElement()
        {
            var elementName = Context.ArrayElementName;
            var aen = GetAttribute<AElementName>();
            if (aen != null)
                elementName = aen.Name;

            return elementName;
        }


        /// <summary>
        /// Determine if a member name needs to be altered because it would otherwise not be a valid XML element name
        /// </summary>
        /// <remarks>
        /// Mainly caused by auto-generated backing fields for Properties with {get;set;} semantic
        /// </remarks>
        /// <param name="_memberName">The name of the reflected member</param>
        /// <returns>A string appropriate for use as an XmlElement name</returns>
        public string FixMemberName( string _memberName )
        {
            if (string.IsNullOrEmpty( _memberName ))
                return "_";

            if (_memberName[0] == '<') // its a backing field
            {
                // For now, all we know about is the format <PropertyName>k__BackingField format, so account for it.
                return _memberName.Substring( 1, _memberName.IndexOf( '>' ) - 1 );
            }

            return _memberName;
        }
    }
}