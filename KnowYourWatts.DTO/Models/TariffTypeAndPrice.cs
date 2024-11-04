using KnowYourWatts.Server.DTO.Enums;

namespace KnowYourWatts.Server.DTO.Models;

public sealed class TariffTypeAndPrice
{
    public TariffType TariffType { get; set; }

    public decimal PriceInPence { get; set; }
}
