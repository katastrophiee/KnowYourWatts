using KnowYourWatts.ClientUI.DTO.Enums;

namespace KnowYourWatts.ClientUI.DTO.Requests;

public class WeeklyUsageRequest
{
    public TariffType TariffType { get; set; }
    public decimal CurrentReading { get; set; }
}
