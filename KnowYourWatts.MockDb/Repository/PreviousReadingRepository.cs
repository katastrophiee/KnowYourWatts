using KnowYourWatts.MockDb.Interfaces;
using KnowYourWatts.Server.DTO.Enums;
using KnowYourWatts.Server.DTO.Models;
using System.Collections.Concurrent;

namespace KnowYourWatts.MockDb.Repository;

public sealed class PreviousReadingRepository : IPreviousReadingRepository
{
    private ConcurrentDictionary<(string mpan, RequestType requestType), PreviousReading> ClientPreviousReadings { get; set; }

    public PreviousReadingRepository()
    {
        ClientPreviousReadings = new ConcurrentDictionary<(string mpan, RequestType requestType), PreviousReading>();
    }

    public decimal? GetPreviousReadingByMpanAndReqType(string mpan, RequestType requestType)
    {
        var key = (mpan, requestType);

        if (ClientPreviousReadings.TryGetValue(key, out var previousReading))
        {
            if (DateTime.Now >= previousReading.ResetDate)
            {
                ClientPreviousReadings.Remove(key, out _);
                return null;
            }

            return previousReading.PreviousUsage;
        }

        return null;
    }

    public void AddOrUpdatePreviousReading(string mpan, decimal currentUsage, RequestType requestType)
    {
        var key = (mpan, requestType);

        var resetDate = requestType == RequestType.TodaysUsage
            ? DateTime.Now.Date.AddDays(1).AddTicks(-1)
            : DateTime.Now.Date.AddDays((DayOfWeek.Monday - DateTime.Now.DayOfWeek + 7) % 7).AddTicks(-1);

        var newPreviousReading = new PreviousReading
        {
            Mpan = mpan,
            PreviousUsage = currentUsage,
            ResetDate = resetDate,
            RequestType = requestType
        };

        ClientPreviousReadings.AddOrUpdate(key, newPreviousReading, (k, existingReading) => newPreviousReading);
    }
}
