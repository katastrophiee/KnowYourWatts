namespace KnowYourWatts.ClientUI.Interfaces;

public interface IEncryptionHelper
{
    public byte[] EncryptData(byte[] data, string publicKey);
}
