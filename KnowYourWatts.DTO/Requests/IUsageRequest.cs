using KnowYourWatts.Server.DTO.Enums;

namespace KnowYourWatts.Server.DTO.Requests;

public interface IUsageRequest
{
    public TariffType TariffType { get; set; }

    public decimal CurrentReading { get; set; }

    public decimal CurrentCost { get; set; }

    public decimal StandingCharge { get; set; }

    public int BillingPeriod { get; set; }
    public RequestType RequestType { get; set; }
}   
