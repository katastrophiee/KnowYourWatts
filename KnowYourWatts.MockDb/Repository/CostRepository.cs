using KnowYourWatts.MockDb.Interfaces;
using KnowYourWatts.Server.DTO.Enums;
using KnowYourWatts.Server.DTO.Models;

namespace KnowYourWatts.MockDb.Repository;

public class CostRepository : ICostRepository
{
    private List<ClientCost> ClientCosts { get; set; }

    public CostRepository()
    {
        ClientCosts = [];
    }

    public void AddOrUpdateClientTotalCost(string mpan, decimal additionalCost, RequestType requestType)
    {
        var existingTotalCost = ClientCosts.FirstOrDefault(r => r.Mpan == mpan && r.RequestType == requestType );

        var newTotalCost = additionalCost;

        if (existingTotalCost is not null)
        {
            newTotalCost += existingTotalCost.TotalCost;
            ClientCosts.Remove(existingTotalCost);
        }

        ClientCosts.Add(new ClientCost { Mpan = mpan, TotalCost = newTotalCost, RequestType = requestType });
    }
}
