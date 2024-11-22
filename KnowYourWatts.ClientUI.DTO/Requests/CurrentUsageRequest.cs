using KnowYourWatts.ClientUI.DTO.Enums;

namespace KnowYourWatts.ClientUI.DTO.Requests;

public sealed class CurrentUsageRequest : IUsageRequest
{
    public TariffType TariffType { get; set; }

    public decimal CurrentReading { get; set; }

    public int BillingPeriod { get; set; }

    public decimal StandingCharge { get; set; }
}
