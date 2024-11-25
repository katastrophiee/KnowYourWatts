using KnowYourWatts.Server.DTO.Enums;

namespace KnowYourWatts.Server.DTO.Requests;

public class SmartMeterCalculationRequest
{
    public string Mpan { get; set; }

    public TariffType TariffType { get; set; }

    public decimal CurrentReading { get; set; }

    public int BillingPeriod { get; set; }

    public decimal StandingCharge { get; set; }
    public RequestType RequestType { get; set; }
}
