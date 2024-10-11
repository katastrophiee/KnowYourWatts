namespace KnowYourWatts.DTO.Response;

public class CalculationResponse
{
    public SmartMeterCalculationResponse CalculationData { get; set; }
    public string Error { get; set; }

    public CalculationResponse(SmartMeterCalculationResponse data)
    {
        CalculationData = data;
    }

    public CalculationResponse(string error)
    {
        Error = error;
    }
}
