using System;
using System.Configuration;
using System.Security.Cryptography;
using Betlln.Security;
using NUnit.Framework;

namespace DtsTests
{
    [TestFixture]
    public class SecureDataProviderTests
    {
        [SetUp]
        public void RunBeforeEachTest()
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.GenerateKey();
                aes.GenerateIV();
                string key = Convert.ToBase64String(aes.Key);
                string iv = Convert.ToBase64String(aes.IV);
                Console.WriteLine($"key: {key}");
                Console.WriteLine($"IV: {iv}");
                ConfigurationManager.AppSettings["DataProtectionKey"] = key;
                ConfigurationManager.AppSettings["DataProtectionVector"] = iv;
            }
        }

        [TestCase("abcdefgh")]
        [TestCase("hi there i am a monkey")]
        [TestCase("qt0-9nW)9gtyiq35pujq;lg  2qwo;yarjg")]
        [TestCase("\t\t\t\t\t\t\t\t\t\t\t\t\t\t")]
        public void FullCycleTest(string plainText)
        {
            SecureDataProvider classUnderTest = new SecureDataProvider();

            byte[] encryptedValue = classUnderTest.Encrypt(plainText);
            string decryptedValue = classUnderTest.Decrypt(encryptedValue);
            Console.WriteLine($"Raw Length: {plainText.Length}  Encrypted Length: {encryptedValue.Length}");

            Assert.AreEqual(plainText, decryptedValue);
        }
    }
}