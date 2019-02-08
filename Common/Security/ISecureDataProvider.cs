namespace Betlln.Security
{
    public interface ISecureDataProvider
    {
        byte[] Encrypt(string plainText);
        string Decrypt(byte[] encryptedData);
    }
}