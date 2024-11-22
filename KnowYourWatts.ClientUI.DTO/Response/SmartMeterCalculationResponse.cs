using System.Text.Json.Serialization;

namespace KnowYourWatts.ClientUI.DTO.Response;

public class SmartMeterCalculationResponse
{
    public decimal? Cost { get; set; }

    public string? ErrorMessage { get; set; }
   /* [JsonConstructor]
    public SmartMeterCalculationResponse(decimal cost)
    {
        Cost = cost;
    }

    public SmartMeterCalculationResponse(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }*/
}
;