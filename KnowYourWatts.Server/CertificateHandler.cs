using KnowYourWatts.Server.Interfaces;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace KnowYourWatts.Server;

public sealed class CertificateHandler : ICertificateHandler
{
    public X509Certificate2 Certificate { get; set; }
    public CertificateHandler()
    {
        // Generate the certificate
        Certificate = GenerateSelfSignedCertificate();
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
    private byte[] DecryptData(byte[] data)
    {
        try
        {
            byte[] decryptedData;
            using RSA rsa = Certificate.GetRSAPrivateKey();
            {
                // Decrypt the data using the private key. Applies same padding as used during encryption
                decryptedData = rsa.Decrypt(data, RSAEncryptionPadding.Pkcs1); 
            }

            return decryptedData;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error decrypting data: {ex.Message}");
            return [];
        }
    }

    // This method generates a certificate. Self signing is used in the development context due to
    // inability to acquire a signed certificate, but an authentic signed certificate is important for real world application
    public X509Certificate2 GenerateSelfSignedCertificate()
    {
        var subjectName = new X500DistinguishedName("CN=KnowYourWattsServer");

        using var rsa = RSA.Create(1024);
        var request = new CertificateRequest(subjectName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        // Create a self-signed certificate
        var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(5));

        // Return the generated certificate
        return new X509Certificate2(certificate.Export(X509ContentType.Pfx));
    }
}