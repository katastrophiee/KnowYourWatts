using System.Security.Cryptography.X509Certificates;

namespace KnowYourWatts.Server.Interfaces;

public interface ICertificateHandler
{
    string DecryptClientMpan(byte[] data);

    X509Certificate2 Certificate { get; set; }

    public X509Certificate2 GenerateSelfSignedCertificate();
}
