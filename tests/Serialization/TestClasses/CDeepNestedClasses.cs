using System.Reflection;
using System.Xml;

namespace Morpheus.Standard.UnitTests.Serialization
{
    public class C6 : C5
    {
        public int Top = 1;

        [ASerializedName( "To_2" )]
        public string RenameMe = "to something else";

        [ADoNotSerialize]
        public string NoSerializeTop = "not me";
    }

    [AExplicitlySerialize]
    public class C5 : C4
    {
        [AExplicitlySerialize]
        public string Middle = "middle";

        public int NoSerializeMiddle = 2;

        public double ForImplicitSerialization = 123.456;

        public C5()
        {
        }

        public C5( XmlNode _xml, out bool _isDone )
        {
            _isDone = false;
        }
    }

    [AUseFieldRenamer( typeof( CTestClassesFieldRenamer_Static ) )]
    public class C4 : C3
    {
        public double PI = 3.14;
        public string PIE = "Apple";

        [ASerializedName( "EAT_PIE" )]
        public bool something = true;
    }

    [AUseFieldRenamer( typeof( CTestClassesFieldRenamer_Dynamic ), DynamicRenaming = true )]
    public class C3 : C2
    {
        public int ThisOne = 8;
        public string ThatOne = "eight";

        [ASerializedName( "Same" )]
        public bool _x = true;

        public DSomeDelegate m_delegate;
    }

    [ADoNotSerialize]
    public class C2 : C1
    {
        public string ShouldntSerialize = "can't see me";

        [AExplicitlySerialize]
        public int AlsoNoSerialization = 235;
    }

    public class C1
    {
        public int ReallyAtBase = 55;
    }


    public delegate void DSomeDelegate();

    public class CTestClassesFieldRenamer_Static : IFieldRenamer
    {
        public string ConvertFieldName( string _fieldName, FieldInfo _fieldInfo )
        {
            if (_fieldInfo.FieldType == typeof( double ))
                return _fieldName + "_double";
            else if (_fieldInfo.FieldType == typeof( string ))
                return _fieldName + "_string";
            else
                return _fieldName + "_bool";
        }
    }

    public class CTestClassesFieldRenamer_Dynamic : IFieldRenamer
    {
        public static bool UpperCase = false;

        public string ConvertFieldName( string _fieldName, FieldInfo _fieldInfo )
        {
            if (UpperCase)
                return _fieldName.ToUpper();
            else
                return _fieldName.ToLower();
        }
    }
}