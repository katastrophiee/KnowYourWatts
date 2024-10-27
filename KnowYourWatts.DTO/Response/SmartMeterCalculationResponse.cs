namespace KnowYourWatts.DTO.Response;

public class SmartMeterCalculationResponse(decimal cost)
{
    public decimal Cost { get; set; } = cost;
}
