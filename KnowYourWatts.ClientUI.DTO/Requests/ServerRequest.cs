using KnowYourWatts.ClientUI.DTO.Enums;

namespace KnowYourWatts.ClientUI.DTO.Requests;

public sealed class ServerRequest
{
    //A Meter Point Administration Number - a unique 13-digit number that identifies a smart meter
    public string Mpan { get; set; }

    public byte[] EncryptedMpan { get; set; }

    public RequestType RequestType { get; set; }

    public string Data { get; set; }
}
