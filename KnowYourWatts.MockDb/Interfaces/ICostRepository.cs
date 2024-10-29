namespace KnowYourWatts.MockDb.Interfaces;

public interface ICostRepository
{
    void AddOrUpdateClientTotalCost(string mpan, decimal additionalCost);
}
