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

        // Public entry point
        public static byte[] ReceiveData(byte[] data)
        {
            try
            {
                byte[] decryptedMPAN = DecryptMPAN(data);
                return decryptedMPAN;
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine($"Error decrypting data: {ex.Message}");
                throw;
            }

        }

        // Decrypts the MPAN by applying the private key after retrieving it from a secure
        // container. This is the actual function for decryption which should be hidden, hence the private
        // modifier to public method ReceiveData.
        private static byte[] DecryptMPAN(byte[] data)
        {
            byte[] decryptedData;
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                // Import parameters from the secure container
                rsa.ImportParameters(GetKeyFromContainer(containerName));

                // Decrypt the data using the private key. Ensure the padding matches the encryption padding.
                decryptedData = rsa.Decrypt(data, RSAEncryptionPadding.Pkcs1); // Adjust padding as needed
            }
            return decryptedData;
        }
        // Should this be moved to the client or kept here and called from the client?
        public static byte[] EncryptData(byte[] data, string publicKey)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);
                return rsa.Encrypt(data, RSAEncryptionPadding.Pkcs1);
            }
        }

        // Could be replaced with a get set?
        // On the client side - client will use this method, then send public key over socket
        // Implement an event to receive key after connecting and enter decrypt - how?
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
