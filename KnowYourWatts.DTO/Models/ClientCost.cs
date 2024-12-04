using KnowYourWatts.Server.DTO.Enums;

namespace KnowYourWatts.Server.DTO.Models;

public class ClientCost
{
    public string Mpan { get; set; }

    public decimal TotalCost { get; set; }

    public DateTime ResetDate { get; set; }

    public RequestType RequestType { get; set; }
}
