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
        var existingTotalCost = ClientCosts.FirstOrDefault(r => r != null && r.Mpan == mpan && r.RequestType == requestType );

        var newTotalCost = additionalCost;

        if (existingTotalCost is not null)
        {
            //newTotalCost += existingTotalCost.TotalCost;
            ClientCosts.Remove(existingTotalCost);
        }

        ClientCosts.Add(new ClientCost { Mpan = mpan, TotalCost = Math.Round(newTotalCost,2,MidpointRounding.AwayFromZero), RequestType = requestType });
    }

    public decimal? GetPreviousTotalCostByMpanAndReqType(string mpan, RequestType requestType)
    {
        var previousReading = ClientCosts.FirstOrDefault(r => r != null && r.Mpan == mpan && r.RequestType == requestType);

        return previousReading?.TotalCost;
    }
}
