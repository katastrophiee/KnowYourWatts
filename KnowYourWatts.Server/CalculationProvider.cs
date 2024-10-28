using KnowYourWatts.Server.DTO.Enums;
using KnowYourWatts.Server.DTO.Requests;
using KnowYourWatts.Server.DTO.Response;
using KnowYourWatts.Server.Interfaces;

namespace KnowYourWatts.Server;

public sealed class CalculationProvider : ICalculationProvider
{
    // Dictionary containing various tariff types and their rates per KWh
    // Will want to move to 'db' eventually

    //change all decimals to decimal - avoids rounding errors
    private readonly Dictionary<TariffType, decimal> tariffsInPence = new()
    {
        { TariffType.Fixed, 24.50m },
        { TariffType.Flex, 26.20m },
        { TariffType.Green, 27.05m },
        { TariffType.OffPeak, 23.64m }
    };

    public SmartMeterCalculationResponse CalculateCost(SmartMeterCalculationRequest request)
    {
        if (request.CurrentReading < request.PreviousReading)
            return new("Current energy reading cannot be less than previous energy reading.");

        if (!tariffsInPence.TryGetValue(request.TariffType, out decimal pricePerUnit))
            return new($"Tariff type '{request.TariffType}' does not exist.");

        var energyUsed = request.CurrentReading - request.PreviousReading;

        decimal costOfElectricity = energyUsed * pricePerUnit;

        decimal totalStandingCharge = request.ExistingCharge * request.BillingPeriod;

        decimal totalBeforeVAT = costOfElectricity + totalStandingCharge;

        decimal vatAmount = totalBeforeVAT * 0.05m;

        decimal totalCost = totalBeforeVAT + vatAmount;

        return new SmartMeterCalculationResponse(Math.Round(totalCost, 2, MidpointRounding.AwayFromZero));
    }

}