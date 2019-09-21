using System;

namespace Morpheus.Standard.UnitTests.Serialization
{
    public class CPerson
    {
        public string m_name = "Homer";
        public int m_age = 45;
        public CAddress m_address = new CAddress();
        public string[] m_kidsNames = new string[] { "Maggie", "Lisa", "Bart" };
        public int[] m_kidsAges = new int[] { 1, 7, 9 };
        public string m_aNullValue = null;
        public CAddress m_otherAddress = new CSuperAddress();

        public void Alternate()
        {
            m_name = "Alyssa";
            m_age = 25;
            m_kidsNames = new string[] { "Mike", "Michael", "Martin", "Unholy" };
            m_kidsAges = new int[] { 37 };
            m_address.m_street = "Plateau Rd.";
            m_address.m_city = "Reno";
            m_address.m_zip = 89509;

            m_otherAddress.m_street = "Arthur Ave.";
            m_otherAddress.m_city = "Cronulla";
            m_otherAddress.m_zip = 2020;
            ((CSuperAddress) m_otherAddress).m_country = "Australia";
        }
    }

    public class CAddress
    {
        public string m_street = "Evergreen Terrace";
        public string m_city = "Springfield";
        public int m_zip = 45678;

        private static readonly Random sm_rng = new Random( 1234 );

        public static CAddress Get()
        {
            var a = new CAddress
            {
                m_street = (sm_rng.Next( 1900 ) * 5).ToString() + " Evergreen Terrace",
                m_city = "Springfield " + (sm_rng.Next( 10 ) + 1),
                m_zip = 10000 + sm_rng.Next( 90000 )
            };

            return a;
        }
    }

    public class CSuperAddress : CAddress
    {
        public string m_country = "USA";
    }
}