﻿namespace KnowYourWatts.Server.Interfaces;

public interface IKeyHandler
{
    string PublicKey { get; set; }

    string DecryptClientMpan(byte[] data);

    byte[] EncryptData(byte[] data, string publicKey);
}