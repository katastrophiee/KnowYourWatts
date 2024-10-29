using KnowYourWatts.MockDb.Interfaces;
using KnowYourWatts.Server.DTO.Models;

namespace KnowYourWatts.MockDb.Repository;

public sealed class PreviousReadingRepository(MockDatabase mockDbContext) : IPreviousReadingRepository
{
    private readonly MockDatabase _mockDbContext = mockDbContext;

    public PreviousReading? GetPreviousReadingByMpan(string mpan)
    {
        var test = _mockDbContext.PreviousReadings.Where(r => !string.IsNullOrEmpty(r.Mpan));
        return _mockDbContext.PreviousReadings.FirstOrDefault(r => r.Mpan == mpan);
    }
}
