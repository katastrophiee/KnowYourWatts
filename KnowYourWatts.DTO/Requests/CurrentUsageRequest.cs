using KnowYourWatts.Server.DTO.Enums;

namespace KnowYourWatts.Server.DTO.Requests;

public sealed class CurrentUsageRequest
{
    public TariffType TariffType { get; set; }

    public decimal CurrentReading { get; set; }

    public int BillingPeriod { get; set; }

    public decimal ExistingCharge { get; set; }
}
