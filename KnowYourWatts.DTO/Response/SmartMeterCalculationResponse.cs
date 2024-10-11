namespace KnowYourWatts.DTO.Response;

public class SmartMeterCalculationResponse(double cost)
{
    public double Cost { get; set; } = cost;
}
