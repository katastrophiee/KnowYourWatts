using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace KnowYourWatts.Server
{
    public class KeyHandler
    {
        // Adjust all with appropriate security measures?

        private const string containerName = "ServKeyContainer";
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

            SaveKeyInContainer(containerName);
        }
        // Decrypts the MPAN by applying the private key. In the future, this will implement the
        // GetKeyFromContainer method to first retrieve the private key from the container, as the key
        // will not be plainly stored.
        private static byte[] DecryptMPAN(byte[] data)
        {
            byte[] decryptedData;
            // _privateKey = GetKeyFromContainer(containerName);
            using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
            {
                // Import parameters from container fetch
                RSA.ImportParameters(GetKeyFromContainer(containerName));

                // Decrypt the data with the parameters imported from the key  
                decryptedData = RSA.Decrypt(data, true);
            }
            return decryptedData;
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

        private static void SaveKeyInContainer(string containerName)
        {
            // CspParameters only works on Windows? What is the alternative?
            CspParameters cspParams = new CspParameters
            {
                KeyContainerName = containerName,
                Flags = CspProviderFlags.UseMachineKeyStore // Optional: use machine-level key store
            };

            using (RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(cspParams))
            {
                rsaProvider.PersistKeyInCsp = true;
                rsaProvider.ImportParameters(_privateKey);
            }
        }

        private static RSAParameters GetKeyFromContainer(string containerName)
        {
            // Only works on windows
            CspParameters cspParams = new CspParameters
            {
                KeyContainerName = containerName,
                Flags = CspProviderFlags.UseMachineKeyStore // Optional: use machine-level key store
            };

            using (RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(cspParams))
            {
                return rsaProvider.ExportParameters(true);
            }
        }
    }
}
