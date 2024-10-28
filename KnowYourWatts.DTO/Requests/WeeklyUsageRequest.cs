using KnowYourWatts.Server.DTO.Enums;

namespace KnowYourWatts.Server.DTO.Requests;

public class WeeklyUsageRequest
{
    public TariffType TariffType { get; set; }
    public decimal CurrentReading { get; set; }
}
