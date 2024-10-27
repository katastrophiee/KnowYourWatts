using KnowYourWatts.Server.DTO.Enums;

namespace KnowYourWatts.DTO.Requests;

public sealed class CurrentUsageRequest
{
    public TariffType TariffType { get; set; }
    public decimal CurrentReading { get; set; }
}
