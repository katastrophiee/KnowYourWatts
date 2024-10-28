using KnowYourWatts.ClientUI.DTO.Enums;

namespace KnowYourWatts.ClientUI.DTO.Requests;

public class DailyUsageRequest
{
    public TariffType TariffType { get; set; }
    public decimal CurrentReading { get; set; }
}
