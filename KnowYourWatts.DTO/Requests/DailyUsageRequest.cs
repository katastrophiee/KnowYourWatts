using KnowYourWatts.Server.DTO.Enums;

namespace KnowYourWatts.DTO.Requests;

public class DailyUsageRequest
{
    public TariffType TariffType { get; set; }
    public decimal CurrentReading { get; set; }
}
