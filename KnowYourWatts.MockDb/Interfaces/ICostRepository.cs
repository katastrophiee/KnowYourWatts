using KnowYourWatts.Server.DTO.Enums;

namespace KnowYourWatts.MockDb.Interfaces;

public interface ICostRepository
{
    void AddOrUpdateClientTotalCost(string mpan, decimal additionalCost, RequestType requestType);
}
