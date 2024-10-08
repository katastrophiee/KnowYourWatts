namespace KnowYourWatts.DTO.Requests;

public sealed class SmartMeterCalculationRequest
{
    public double TotalBill { get; set; }
    public double CurrentRateMoney { get; set; }
    public double CurrentRateKwh { get; set; }
}
