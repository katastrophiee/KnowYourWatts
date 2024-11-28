using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace KnowYourWatts.ClientUI.Interfaces
{
    public interface IEncryptionHelper
    {
        public byte[] EncryptData(byte[] data, string publicKey);
    }
}
