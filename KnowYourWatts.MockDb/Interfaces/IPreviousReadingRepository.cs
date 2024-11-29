namespace KnowYourWatts.MockDb.Interfaces;

public interface IPreviousReadingRepository
{
    decimal? GetPreviousReadingByMpan(string mpan);

    void AddOrUpdatePreviousReading(string mpan, decimal currentUsage);
}
