using KnowYourWatts.Server.DTO.Enums;

namespace KnowYourWatts.MockDb.Interfaces;

public interface IPreviousReadingRepository
{
    decimal? GetPreviousReadingByMpanAndReqType(string mpan, RequestType requestType);

    void AddOrUpdatePreviousReading(string mpan, decimal currentUsage, RequestType requestType);
}
