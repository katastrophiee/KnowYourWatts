using KnowYourWatts.DTO.Enums;

namespace KnowYourWatts.DTO.Requests;

public sealed class CurrentUsageRequest
{
    public TarrifType TarrifType { get; set; }
    public double CurrentReading { get; set; }
}
