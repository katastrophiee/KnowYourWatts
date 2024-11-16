using KnowYourWatts.Server.DTO.Enums;

namespace KnowYourWatts.Server.DTO.Requests;

public class ServerRequest
{
    //A Meter Point Administration Number - a unique 13-digit number that identifies a smart meter
    public string Mpan { get; set; }

    public byte[] encryptedMPAN { get; set; }

    public RequestType RequestType { get; set; }

    public string Data { get; set; }
}
