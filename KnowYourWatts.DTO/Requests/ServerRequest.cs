using KnowYourWatts.DTO.Enums;

namespace KnowYourWatts.DTO.Requests;

public class ServerRequest
{
    public RequestType RequestType { get; set; }

    public string Data { get; set; }
}
