namespace KnowYourWatts.Server.Interfaces;

public interface IKeyHandler
{
    string PublicKey { get; set; }

    string DecryptClientMpan(byte[] data);
}
