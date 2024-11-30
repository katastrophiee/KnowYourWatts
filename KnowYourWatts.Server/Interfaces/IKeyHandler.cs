using System.Security.Cryptography.X509Certificates;

namespace KnowYourWatts.Server.Interfaces;

public interface IKeyHandler
{
    X509Certificate2 Certificate { get; set; }

    string DecryptClientMpan(byte[] data);
}
