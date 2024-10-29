using KnowYourWatts.Server.DTO.Enums;

namespace KnowYourWatts.Server.DTO.Requests;

public class SmartMeterCalculationRequest
{
    public TariffType TariffType { get; set; }

    public decimal CurrentReading { get; set; }

    public decimal PreviousReading { get; set; }

    public int BillingPeriod { get; set; }

    public decimal StandingCharge { get; set; }
}
