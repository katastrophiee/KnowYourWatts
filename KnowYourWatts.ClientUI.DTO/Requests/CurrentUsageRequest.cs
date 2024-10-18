using KnowYourWatts.DTO.Enums;

namespace KnowYourWatts.DTO.Requests;

public sealed class CurrentUsageRequest
{
    public TariffType TariffType { get; set; }
    public double CurrentReading { get; set; }
}
