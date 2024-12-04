using KnowYourWatts.ClientUI.Interfaces;
using System.Security.Cryptography;

namespace KnowYourWatts.ClientUI;

public class EncryptionHelper : IEncryptionHelper
{
    public byte[] EncryptData(byte[] data, string publicKey)
    {
        using var rsa = new RSACryptoServiceProvider();

        rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);

        return rsa.Encrypt(data, RSAEncryptionPadding.Pkcs1);
    }
}