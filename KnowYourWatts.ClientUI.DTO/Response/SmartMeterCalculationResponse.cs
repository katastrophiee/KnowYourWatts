namespace KnowYourWatts.ClientUI.DTO.Response;

public sealed class SmartMeterCalculationResponse
{
    public decimal? Cost { get; set; }

    public string ErrorMessage { get; set; }
}