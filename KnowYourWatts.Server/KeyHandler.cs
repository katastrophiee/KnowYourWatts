using KnowYourWatts.Server.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace KnowYourWatts.Server;

public sealed class KeyHandler : IKeyHandler
{
    public static RSA CryptographyKey { get; set; } = null!;
    public string PublicKey { get; set; } = null!;
    private static RSAParameters PrivateKey;
    private const string ContainerName = "ServKeyContainer";

    public KeyHandler()
    {
        CryptographyKey = RSA.Create();

        PublicKey = Convert.ToBase64String(CryptographyKey.ExportRSAPublicKey());
        PrivateKey = CryptographyKey.ExportParameters(true);

        SaveKeyInContainer(ContainerName);
    }

    // Public entry point
    public string DecryptClientMpan(byte[] data)
    {
        try
        {
            var decryptedMpan = DecryptData(data);

            if (decryptedMpan.Length > 0)
            {
                return Encoding.UTF8.GetString(decryptedMpan);
            }
            else
            {
                Console.WriteLine($"The decrypted MPAN was empty.");
                return string.Empty;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unkown error occured when decrypting data: {ex.Message}");
            return string.Empty;
        }

    }

    // Decrypts the MPAN by applying the private key after retrieving it from a secure
    // container. This is the actual function for decryption which should be hidden, hence the private
    // modifier to public method ReceiveData.
    private static byte[] DecryptData(byte[] data)
    {
        try
        {
            byte[] decryptedData;
            using (var rsa = new RSACryptoServiceProvider())
            {
                // Import parameters from the secure container
                rsa.ImportParameters(GetKeyFromContainer(ContainerName));

                // Decrypt the data using the private key. Ensure the padding matches the encryption padding.
                decryptedData = rsa.Decrypt(data, RSAEncryptionPadding.Pkcs1); // Adjust padding as needed
            }

            return decryptedData;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error decrypting data: {ex.Message}");
            return [];
        }
    }

    private static void SaveKeyInContainer(string containerName)
    {
        // CspParameters only works on Windows? What is the alternative?
        var cspParams = new CspParameters
        {
            KeyContainerName = containerName,
            Flags = CspProviderFlags.UseMachineKeyStore // Optional: use machine-level key store
        };

        using (RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(cspParams))
        {
            rsaProvider.PersistKeyInCsp = true;
            rsaProvider.ImportParameters(PrivateKey);
        }
    }

    private static RSAParameters GetKeyFromContainer(string containerName)
    {
        // Only works on windows
        var cspParams = new CspParameters
        {
            KeyContainerName = containerName,
            Flags = CspProviderFlags.UseMachineKeyStore // Optional: use machine-level key store
        };

        using RSACryptoServiceProvider rsaProvider = new(cspParams);
        return rsaProvider.ExportParameters(true);
    }
}
