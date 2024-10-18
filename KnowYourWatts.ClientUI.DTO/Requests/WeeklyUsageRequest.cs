using KnowYourWatts.DTO.Enums;

namespace KnowYourWatts.DTO.Requests;

public class WeeklyUsageRequest
{
    public TariffType TariffType { get; set; }
    public double CurrentReading { get; set; }
}
