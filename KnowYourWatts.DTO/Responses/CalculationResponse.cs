namespace KnowYourWatts.Server.DTO.Responses;

public class CalculationResponse
{
    public decimal? Cost { get; set; }

    public string? ErrorMessage { get; set; }

    public CalculationResponse(decimal cost)
    {
        Cost = cost;
    }

    public CalculationResponse(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }
}