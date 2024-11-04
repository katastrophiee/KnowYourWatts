using KnowYourWatts.MockDb.Interfaces;
using KnowYourWatts.Server.DTO.Enums;
using KnowYourWatts.Server.DTO.Models;

namespace KnowYourWatts.MockDb.Repository;

public class TariffRepository : ITariffRepository
{
    public List<TariffTypeAndPrice> TariffTypesAndPrices { get; set; }

    public TariffRepository()
    {
        TariffTypesAndPrices =
        [
            new() { TariffType = TariffType.Fixed, PriceInPence = 24.50m },
            new() { TariffType = TariffType.Flex, PriceInPence = 26.20m },
            new() { TariffType = TariffType.Green, PriceInPence = 27.05m },
            new() { TariffType = TariffType.OffPeak, PriceInPence = 23.64m }
        ];
    }

    public TariffTypeAndPrice? GetTariffPriceByType(TariffType tariffType)
    {
        return TariffTypesAndPrices.FirstOrDefault(t => t.TariffType == tariffType);
    }
}
