using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace KnowYourWatts.Server
{
    public class KeyHandler
    {
        // Adjust all with appropriate security measures?

        private const string ContainerName = "ServKeyContainer";
        private static RSAParameters _privateKey;
        // Delete this later - store in container
        private static string _publicKey;
        
        // Generates the public and private keys - later, the keys should be stored in a container,
        // with only the public key being sent to the client
        public static void GenerateKeys()
        {
            using (RSA rsa = RSA.Create())
            {
                _privateKey = rsa.ExportParameters(true);
                _publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
            }
        }
        // Decrypts the MPAN by applying the private key. In the future, this will implement the
        // GetKeyFromContainer method to first retrieve the private key from the container, as the key
        // will not be plainly stored.
        public static string DecryptMPAN(byte[] data)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportParameters(_privateKey);
                byte[] decryptedBytes = rsa.Decrypt(data, RSAEncryptionPadding.Pkcs1);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }
        // This is a placeholder for now, this should be moved to the client when client functionality is being added
        //public static byte[] EncryptData(string data, string publicKey)
        //{
        //    byte[] dataBytes = Encoding.UTF8.GetBytes(data);

        //    using (RSA rsa = RSA.Create())
        //    {
        //        rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);
        //        return rsa.Encrypt(dataBytes, RSAEncryptionPadding.Pkcs1);
        //    }
        //}

        // Could be replaced with a get set?
        public static string GetPublicKey()
        {
            return _publicKey;
        }

        // IMPLEMENT LATER FOR BETTER SECURITY
        private void SaveKeyInContainer(string containerName)
        {
            // CspParameters only works on Windows? What is the alternative?
            CspParameters cspParams = new CspParameters
            {
                KeyContainerName = ContainerName,
                Flags = CspProviderFlags.UseMachineKeyStore // Optional: use machine-level key store
            };

            using (RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(cspParams))
            {
                rsaProvider.PersistKeyInCsp = true;
                rsaProvider.ImportParameters(_privateKey);
            }
        }

        public static RSAParameters GetKeyFromContainer(string containerName)
        {
            // Only works on windows
            CspParameters cspParams = new CspParameters
            {
                KeyContainerName = ContainerName,
                Flags = CspProviderFlags.UseMachineKeyStore // Optional: use machine-level key store
            };

            using (RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(cspParams))
            {
                return rsaProvider.ExportParameters(true);
            }
        }
    }
}
