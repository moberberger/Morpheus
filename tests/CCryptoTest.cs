using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;


namespace Morpheus.Standard.UnitTests
{
    /// <summary>
    /// This is a test class for CCrypto and is intended
    /// to contain all CCrypto Unit Tests
    ///</summary>
    [TestClass()]
    public class CCryptoTest
    {
        [TestMethod]
        public void TestStandardUsage()
        {
            var crypt = new Crypto();
            var target = "homer is sooooo cool!!";

            var crypted = crypt.Encrypt( target );
            Assert.AreNotEqual<string>( target, crypted, "Should be different strings" );

            var actual = crypt.Decrypt( crypted );
            Assert.AreEqual<string>( target, actual, "Should be equal again" );
        }

        [TestMethod]
        public void TestNewCryptoObject()
        {
            var c1 = new Crypto();
            var target = "Bart is the biatch.";

            var crypted = c1.Encrypt( target );
            Assert.AreNotEqual<string>( target, crypted, "Should be different strings" );

            var c2 = new Crypto( c1.Key, c1.IV, c1.SystemSalt );
            var actual = c2.Decrypt( crypted );
            Assert.AreEqual<string>( target, actual, "Should be equal again" );
        }

        [TestMethod]
        public void TestNewCryptoObjectWithSaltError()
        {
            var c1 = new Crypto();
            var target = "Bart is the biatch.";

            var crypted = c1.Encrypt( target );
            Assert.AreNotEqual<string>( target, crypted, "Should be different strings" );

            var salt = Crypto.StringToBytes( c1.SystemSalt );
            salt[0] = (byte) ~salt[0];
            var key = Crypto.StringToBytes( c1.Key );
            var iv = Crypto.StringToBytes( c1.IV );

            var c2 = new Crypto( key, iv, salt );
            var actual = c2.Decrypt( crypted );
            Assert.IsNull( actual, "Should be NULL with a bad Salt." );

            salt[0] = (byte) ~salt[0];
            var c3 = new Crypto( key, iv, salt );
            actual = c3.Decrypt( crypted );
            Assert.AreEqual<string>( target, actual, "Should be equal again" );
        }

        [TestMethod]
        public void CryptErrorTest()
        {
            var c1 = new Crypto();
            var s = c1.Encrypt( null );
            Assert.IsNull( s, "Expected null return value from encrypting a null string" );

            s = c1.Decrypt( null );
            Assert.IsNull( s, "Expected null return value from decrypting a null string" );
        }


    }
}
