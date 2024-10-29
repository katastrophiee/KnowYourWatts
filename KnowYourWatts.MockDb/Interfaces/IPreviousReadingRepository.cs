using KnowYourWatts.Server.DTO.Models;

namespace KnowYourWatts.MockDb.Interfaces;

public interface IPreviousReadingRepository
{
    PreviousReading? GetPreviousReadingByMpan(string mpan);
}
