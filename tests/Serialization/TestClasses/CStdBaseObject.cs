namespace Morpheus.Standard.UnitTests.Serialization
{
    public class CStdBaseObject
    {
        public static ETestStatus STATUS = ETestStatus.NONE;

        public string Name;
        public int Age;
    }

    public class CMySuperStd : CStdBaseObject
    {
        public string Sex;
    }
}