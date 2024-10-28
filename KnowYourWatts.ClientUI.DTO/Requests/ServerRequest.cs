using KnowYourWatts.ClientUI.DTO.Enums;

namespace KnowYourWatts.ClientUI.DTO.Requests;

public class ServerRequest
{
    public RequestType Type { get; set; }

    public string Data { get; set; }
}
