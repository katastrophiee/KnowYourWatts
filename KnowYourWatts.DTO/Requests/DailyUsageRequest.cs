using KnowYourWatts.DTO.Enums;

namespace KnowYourWatts.DTO.Requests;

public class DailyUsageRequest
{
    public TarrifType TarrifType { get; set; }
    public double CurrentReading { get; set; }
}
