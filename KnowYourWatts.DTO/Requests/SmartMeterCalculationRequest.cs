namespace KnowYourWatts.DTO.Requests;

public sealed class SmartMeterCalculationRequest
{
    // Potentially poor wording of request items? Not sure if 'current rate' refers to current usage, or tariff rate.
    // Tariff rate is already covered in the ServerMathLogic class.
    // Added TariffType as this is needed to calculate the bill.
    public double TotalBill { get; set; }
    public double CurrentRateMoney { get; set; }
    public double CurrentRateKwh { get; set; }
    public string TariffType { get; set; }
}
