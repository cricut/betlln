using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;

namespace Betlln.Security
{
    public class SecureDataProvider : ISecureDataProvider
    {
        private readonly byte[] _privateKey;
        private readonly byte[] _vector;

        public SecureDataProvider()
        {
            _privateKey = GetKey("DataProtectionKey");
            _vector = GetKey("DataProtectionVector");
        }

        private static byte[] GetKey(string appSettingName)
        {
            #if !DEBUG
            System.Configuration.Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (!configuration.AppSettings.SectionInformation.IsProtected)
            {
                throw new System.Security.Authentication.AuthenticationException("This configuration is not secure; the keys may have leaked. Change the keys, re-encrypt the data, and then protect the config file.");
            }
            #endif

            string keyValue = ConfigurationManager.AppSettings[appSettingName];
            return Convert.FromBase64String(keyValue);
        }

        public byte[] Encrypt(string plainText)
        {
            byte[] encrypted;

            using (Aes encryption = Aes.Create())
            {
                if (encryption == null)
                {
                    throw new CryptographicException("AES not available.");
                }

                encryption.Key = _privateKey;
                encryption.IV = _vector;

                ICryptoTransform encryptor = encryption.CreateEncryptor(encryption.Key, encryption.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream encryptedStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(encryptedStream))
                        {
                            streamWriter.Write(plainText);
                        }
                        encrypted = memoryStream.ToArray();
                    }
                }
            }

            return encrypted;
        }

        public string Decrypt(byte[] encryptedData)
        {
            string plaintext;

            using (Aes encryption = Aes.Create())
            {
                if (encryption == null)
                {
                    throw new CryptographicException("AES not available.");
                }

                encryption.Key = _privateKey;
                encryption.IV = _vector;

                ICryptoTransform decryptor = encryption.CreateDecryptor(encryption.Key, encryption.IV);

                using (MemoryStream memoryStream = new MemoryStream(encryptedData))
                {
                    using (CryptoStream decryptedStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader(decryptedStream))
                        {
                            plaintext = streamReader.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;
        }
    }
}
