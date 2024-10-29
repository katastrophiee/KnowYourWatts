using KnowYourWatts.MockDb.Interfaces;
using KnowYourWatts.Server.DTO.Models;

namespace KnowYourWatts.MockDb.Repository;

public sealed class PreviousReadingRepository : IPreviousReadingRepository
{
    public List<PreviousReading> ClientPreviousReadings { get; set; }

    public PreviousReadingRepository()
    {
        ClientPreviousReadings = [];
    }

    public decimal? GetPreviousReadingByMpan(string mpan) => ClientPreviousReadings.FirstOrDefault(r => r.Mpan == mpan)?.PreviousUsage ?? 0;

    public void AddOrUpdatePreviousReading(string mpan, decimal currentUsage)
    {
        var existingReading = ClientPreviousReadings.FirstOrDefault(r => r.Mpan == mpan);

        if (existingReading is not null)
            ClientPreviousReadings.Remove(existingReading);

        ClientPreviousReadings.Add(new PreviousReading { Mpan = mpan, PreviousUsage = currentUsage });
    }
}
