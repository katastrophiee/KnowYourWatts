using KnowYourWatts.MockDb.Interfaces;
using KnowYourWatts.Server.DTO.Enums;
using KnowYourWatts.Server.DTO.Models;
using System.Collections.Concurrent;

namespace KnowYourWatts.MockDb.Repository;

public class TariffRepository : ITariffRepository
{
    private ConcurrentDictionary<TariffType, TariffTypeAndPrice> TariffTypesAndPrices { get; set; }

    public TariffRepository()
    {
        TariffTypesAndPrices = new ConcurrentDictionary<TariffType, TariffTypeAndPrice>
        {
            [TariffType.Fixed] = new TariffTypeAndPrice { TariffType = TariffType.Fixed, PriceInPence = 24.50m },
            [TariffType.Flex] = new TariffTypeAndPrice { TariffType = TariffType.Flex, PriceInPence = 26.20m },
            [TariffType.Green] = new TariffTypeAndPrice { TariffType = TariffType.Green, PriceInPence = 27.05m },
            [TariffType.OffPeak] = new TariffTypeAndPrice { TariffType = TariffType.OffPeak, PriceInPence = 23.64m }
        };
    }

    public TariffTypeAndPrice? GetTariffPriceByType(TariffType tariffType)
    {
        TariffTypesAndPrices.TryGetValue(tariffType, out var tariffTypeAndPrice);
        return tariffTypeAndPrice;
    }
}
