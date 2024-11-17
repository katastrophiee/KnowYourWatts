﻿namespace KnowYourWatts.Server.DTO.Responses;

public class SmartMeterCalculationResponse
{
    public decimal? Cost { get; set; }

    public string? ErrorMessage { get; set; }

    public SmartMeterCalculationResponse(decimal cost)
    {
        Cost = cost;
    }

    public SmartMeterCalculationResponse(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }
}