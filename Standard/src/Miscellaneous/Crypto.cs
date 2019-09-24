using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Morpheus
{
    /// <summary>
    /// Class to help with the encryption and decryption of strings. Any strings encrypted with
    /// an object of this class must be decrypted by the same object.
    /// </summary>
    public class Crypto
    {
        private readonly SymmetricAlgorithm m_algo = new RijndaelManaged();
        // SymmetricAlgorithm m_algo = new TripleDESCryptoServiceProvider(); SymmetricAlgorithm
        // m_algo = new RC2CryptoServiceProvider(); SymmetricAlgorithm m_algo = new
        // DESCryptoServiceProvider();

        private readonly ICryptoTransform m_encryptor;
        private readonly ICryptoTransform m_decryptor;

        /// <summary>
        /// 
        /// </summary>
        public const int TOTAL_SALT_LENGTH = 32;

        private readonly byte[] m_systemSalt;

        /// <summary>
        /// Retrieve the key as a string
        /// </summary>
        public string Key => BytesToString( m_algo.Key );

        /// <summary>
        /// Retrieve the initialization vector as a string
        /// </summary>
        public string IV => BytesToString( m_algo.IV );

        /// <summary>
        /// Retrieve the system salt as a string
        /// </summary>
        public string SystemSalt => BytesToString( m_systemSalt );


        /// <summary>
        /// The number of bytes in the system salt for this instantiation
        /// </summary>
        public int SystemSaltLength => m_systemSalt.Length;

        /// <summary>
        /// The number of bytes in the message salt for this instantiation
        /// </summary>
        public int MessageSaltLength => TOTAL_SALT_LENGTH - SystemSaltLength;

        /// <summary>
        /// Set up this instantiation
        /// </summary>
        public Crypto()
        {
            m_algo.GenerateKey();
            m_algo.GenerateIV();

            m_encryptor = m_algo.CreateEncryptor( m_algo.Key, m_algo.IV );
            m_decryptor = m_algo.CreateDecryptor( m_algo.Key, m_algo.IV );

            var systemSaltLength = Rng.Default.Next( TOTAL_SALT_LENGTH / 2 ) + TOTAL_SALT_LENGTH / 4;

            m_systemSalt = new byte[systemSaltLength];
            Rng.Default.NextBytes( m_systemSalt );
        }

        /// <summary>
        /// Set up the CCrypto with information from a previous CCrypto
        /// </summary>
        /// <param name="_key"></param>
        /// <param name="_iv"></param>
        /// <param name="_salt"></param>
        public Crypto( byte[] _key, byte[] _iv, byte[] _salt )
        {
            m_algo.Key = _key;
            m_algo.IV = _iv;

            m_encryptor = m_algo.CreateEncryptor( m_algo.Key, m_algo.IV );
            m_decryptor = m_algo.CreateDecryptor( m_algo.Key, m_algo.IV );

            m_systemSalt = _salt;
        }

        /// <summary>
        /// Set up the CCrypto with information from a previous CCrypto
        /// </summary>
        /// <param name="_key"></param>
        /// <param name="_iv"></param>
        /// <param name="_salt"></param>
        public Crypto( string _key, string _iv, string _salt )
        {
            m_algo.Key = StringToBytes( _key );
            m_algo.IV = StringToBytes( _iv );

            m_encryptor = m_algo.CreateEncryptor( m_algo.Key, m_algo.IV );
            m_decryptor = m_algo.CreateDecryptor( m_algo.Key, m_algo.IV );

            m_systemSalt = StringToBytes( _salt );
        }

        /// <summary>
        /// Encrypt a string into a new string
        /// </summary>
        /// <param name="_string">The string to encrypt</param>
        /// <returns>The encrypted string.</returns>
        public string Encrypt( string _string )
        {
            try
            {
                var enc = new UnicodeEncoding();
                var data = enc.GetBytes( _string );

                var messageSalt = new byte[MessageSaltLength];
                Rng.Default.NextBytes( messageSalt );

                var message = new byte[data.Length + TOTAL_SALT_LENGTH];
                Array.Copy( messageSalt, 0, message, 0, MessageSaltLength );
                Array.Copy( m_systemSalt, 0, message, MessageSaltLength, SystemSaltLength );
                Array.Copy( data, 0, message, TOTAL_SALT_LENGTH, data.Length );

                var memStream = new MemoryStream();
                var cryptStream = new CryptoStream( memStream, m_encryptor, CryptoStreamMode.Write );
                cryptStream.Write( message, 0, message.Length );
                cryptStream.FlushFinalBlock();

                var arr = memStream.ToArray();
                return BytesToString( arr );
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Decrypt a string that's been previously encrypted. The string must have been created
        /// with the <see cref="Encrypt"/> method of this class.
        /// </summary>
        /// <param name="_string">The string that had been encrypted with this class</param>
        /// <returns>The string that was originally encrypted.</returns>
        public string Decrypt( string _string )
        {
            try
            {
                var message = StringToBytes( _string );
                var memStream = new MemoryStream( message );
                var cryptStream = new CryptoStream( memStream, m_decryptor, CryptoStreamMode.Read );

                var result = new byte[message.Length];
                cryptStream.Read( result, 0, result.Length );

                for (var i = 0; i < SystemSaltLength; i++)
                {
                    var idx = i + MessageSaltLength;
                    if (m_systemSalt[i] != result[idx])
                        return null;
                }

                int newLen;
                for (newLen = result.Length; result[newLen - 2] == 0 && result[newLen - 1] == 0; newLen -= 2)
                    ;

                var enc = new UnicodeEncoding();
                var s = enc.GetString( result, TOTAL_SALT_LENGTH, newLen - TOTAL_SALT_LENGTH );

                return s;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Turn an array of bytes into a string
        /// </summary>
        /// <param name="_data"></param>
        /// <returns></returns>
        public static string BytesToString( byte[] _data ) => Convert.ToBase64String( _data );

        /// <summary>
        /// Turn a string into an array of bytes. The string must have been created from an
        /// original byte array using the <see cref="BytesToString"/> method of this class.
        /// </summary>
        /// <param name="_string"></param>
        /// <returns></returns>
        public static byte[] StringToBytes( string _string ) => Convert.FromBase64String( _string );
    }
}
