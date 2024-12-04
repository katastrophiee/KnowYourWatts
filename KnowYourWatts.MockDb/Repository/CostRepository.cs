﻿using KnowYourWatts.MockDb.Interfaces;
using KnowYourWatts.Server.DTO.Enums;
using KnowYourWatts.Server.DTO.Models;
using System.Collections.Concurrent;

namespace KnowYourWatts.MockDb.Repository;

public class CostRepository : ICostRepository
{
    private ConcurrentDictionary<(string mpan, RequestType requestType), ClientCost> ClientCosts { get; set; }


    public CostRepository()
    {
        ClientCosts = new();
    }

    public void AddOrUpdateClientTotalCost(string mpan, decimal newTotalCost, RequestType requestType)
    {
        var key = (mpan, requestType);

        var resetDate = requestType == RequestType.TodaysUsage
            ? DateTime.Now.Date.AddDays(1).AddTicks(-1)
            : DateTime.Now.Date.AddDays((DayOfWeek.Monday - DateTime.Now.DayOfWeek + 7) % 7).AddTicks(-1);

        var newClientCost = new ClientCost 
        { 
            Mpan = mpan,
            TotalCost = Math.Round(newTotalCost, 2, MidpointRounding.AwayFromZero),
            ResetDate = resetDate,
            RequestType = requestType 
        };

        ClientCosts.AddOrUpdate(key, newClientCost, (k, existingCost) => newClientCost);
    }

    public decimal? GetPreviousTotalCostByMpanAndReqType(string mpan, RequestType requestType)
    {
        var key = (mpan, requestType);

        if (ClientCosts.TryGetValue(key, out var clientCost))
        {
            if (DateTime.Now >= clientCost.ResetDate)
            {
                ClientCosts.Remove(key, out _);
                return null;
            }

            return clientCost.TotalCost;
        }

        return null;
    }
}
