using KnowYourWatts.ClientUI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace KnowYourWatts.ClientUI;

public class EncryptionHelper : IEncryptionHelper
{
    public byte[] EncryptData(byte[] data, string publicKey)
    {
        using (var rsa = new RSACryptoServiceProvider())
        {
            rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);

            return rsa.Encrypt(data, RSAEncryptionPadding.Pkcs1);
        }
    }
}
