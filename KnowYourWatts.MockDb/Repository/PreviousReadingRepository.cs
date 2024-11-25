using KnowYourWatts.MockDb.Interfaces;
using KnowYourWatts.Server.DTO.Enums;
using KnowYourWatts.Server.DTO.Models;

namespace KnowYourWatts.MockDb.Repository;

public sealed class PreviousReadingRepository : IPreviousReadingRepository
{
    public List<PreviousReading> ClientPreviousReadings { get; set; }

    public PreviousReadingRepository()
    {
        ClientPreviousReadings = [];
    }

    public decimal? GetPreviousReadingByMpanAndReqType(string mpan, RequestType requestType) => ClientPreviousReadings.FirstOrDefault(r => r.Mpan == mpan && r.RequestType == requestType)?.PreviousUsage ?? 0;

    public void AddOrUpdatePreviousReading(string mpan, decimal currentUsage, RequestType requestType)
    {
        var existingReading = ClientPreviousReadings.FirstOrDefault(r => r.Mpan == mpan && r.RequestType == requestType);

        if (existingReading is not null)
            ClientPreviousReadings.Remove(existingReading);

        ClientPreviousReadings.Add(new PreviousReading { Mpan = mpan, PreviousUsage = currentUsage, RequestType = requestType });
    }
}
