using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace KnowYourWatts.Server.Interfaces;

public interface ICertificateHandler
{
    string DecryptClientMpan(byte[] data);

    X509Certificate2 Certificate { get; set; }

    public X509Certificate2 GenerateSelfSignedCertificate();
}
