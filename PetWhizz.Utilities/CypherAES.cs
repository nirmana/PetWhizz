using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using System.Security.Cryptography;
using System.IO;
using System.Reflection;

namespace PetWhizz.Utilities
{
    public class CypherAES
    {
        private static Logger m_Logger = LogManager.GetCurrentClassLogger();

        private RijndaelManaged m_RijndaelCipher = null;
        private ICryptoTransform m_Encryptor = null;
        private ICryptoTransform m_Decryptor = null;

        private MemoryStream m_MemStream = null;

        // setup everything here.
        public CypherAES(string key)
        {
            m_RijndaelCipher = new RijndaelManaged();

            // set defaults
            m_RijndaelCipher.Mode = CipherMode.CBC;
            m_RijndaelCipher.Padding = PaddingMode.PKCS7;
            m_RijndaelCipher.KeySize = GetKeySize(key);
            m_RijndaelCipher.BlockSize = 128; // block size will always be 128 regardless of keysize aes 128/256

            // set KEY and IV values
            m_RijndaelCipher.Key = Encoding.ASCII.GetBytes(key);
            m_RijndaelCipher.IV = new Byte[]
                                    {
                                        0, 0, 0, 0, 0, 0, 0, 0,
                                        0, 0, 0, 0, 0, 0, 0, 0
                                    }; // sarney notes: blank init;; // for additional security, we could seed this


            // create the encryption and decryption objects
            m_Encryptor = m_RijndaelCipher.CreateEncryptor();
            m_Decryptor = m_RijndaelCipher.CreateDecryptor();
        }

        private int GetKeySize(string key)
        {
            int keySize = Encoding.Default.GetByteCount(key);
            int encryptionKeySize = 0;

            if (keySize == 32)
                encryptionKeySize = 256;
            if (keySize == 16)
                encryptionKeySize = 128;

            if (encryptionKeySize == 0)
                throw new Exception("Invalid Encryption Key");
            return encryptionKeySize;
        }

        /// <summary>
        /// this is used to apply encryption to the input string and 
        /// the returned string is the encryted value
        /// </summary>
        /// <param name="sIn">String value to encrypt.</param>
        /// <returns></returns>
        public String encrypt(String sIn)
        {
            return cryptFunc(sIn, false);
        }

        /// <summary>
        /// This is used to decrypt the input string and
        /// the returned string is the decrypted vaule.
        /// </summary>
        /// <param name="sIn">String value to decrypt.</param>
        /// <returns></returns>
        public String decrypt(String sIn)
        {
            return cryptFunc(sIn, true);
        }

        /// <summary>
        /// This function will take the input string (from encrypt or decrypt functions) 
        /// and perform the necessary activity.
        /// </summary>
        /// <param name="sInText">String value to encrypt/decrypt.</param>
        /// <param name="fDecrypt">Boolean value used to identify encryption or decryption request.</param>
        /// <returns></returns>
        private String cryptFunc(String sInText, bool fDecrypt)
        {
            StreamReader sr = null;
            try
            {
                if ((sInText != null) && (sInText.Trim().Length > 0))
                {
                    String sOutText = "";

                    // check the decryption flag, and either encrypt or decrypt
                    if (fDecrypt)
                    {
                        // decryption
                        // the encrypted string was built using base64
                        byte[] inputBytes = Convert.FromBase64String(sInText);

                        m_MemStream = new MemoryStream(inputBytes, 0, inputBytes.Length);

                        CryptoStream cs = new CryptoStream(m_MemStream, m_Decryptor, CryptoStreamMode.Read);
                        sr = new StreamReader(cs);

                        sOutText = sr.ReadToEnd();

                        // close out all opened streams
                        sr.Close();
                        cs.Close();
                        m_MemStream.Close();
                    }
                    else
                    {
                        byte[] inputBytes = Encoding.ASCII.GetBytes(sInText.ToCharArray());  // C01

                        // encryption
                        m_MemStream = new MemoryStream();

                        CryptoStream cs = new CryptoStream(m_MemStream, m_Encryptor, CryptoStreamMode.Write);

                        // write out the encrypted content into MemoryStream
                        cs.Write(inputBytes, 0, inputBytes.Length);
                        cs.FlushFinalBlock();

                        byte[] outputBytes = m_MemStream.ToArray();

                        // convert to base64 string
                        sOutText = Convert.ToBase64String(outputBytes);

                        // close out all opened streams
                        cs.Close();
                        m_MemStream.Close();
                    }

                    // encrypted or decrypted output
                    return sOutText;
                }
            }
            catch (Exception ex)
            {
                m_Logger.Error(MethodBase.GetCurrentMethod().Name + ":" + " exception: " + ex.Message + ", " + ex.InnerException);
            }
            finally
            {
                Utils.StreamClose(sr);
            }

            return null;
        }
    }
}