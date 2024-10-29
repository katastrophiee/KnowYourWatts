using KnowYourWatts.MockDb.Interfaces;
using KnowYourWatts.Server.DTO.Requests;
using KnowYourWatts.Server.DTO.Response;
using KnowYourWatts.Server.Interfaces;

namespace KnowYourWatts.Server;

public sealed class CalculationProvider(ITariffRepository tariffRepository) : ICalculationProvider
{
    private readonly ITariffRepository _tariffRepository = tariffRepository;

    public SmartMeterCalculationResponse CalculateCost(SmartMeterCalculationRequest request)
    {
        if (request.CurrentReading < request.PreviousReading)
            return new("Current energy reading cannot be less than previous energy reading.");

        var tarrif = _tariffRepository.GetTariffByType(request.TariffType);

        if (tarrif is null)
            return new($"Tariff type '{request.TariffType}' does not exist.");

        var energyUsed = request.CurrentReading - request.PreviousReading;

        var costOfElectricity = energyUsed * tarrif.PriceInPence;

        var totalStandingCharge = request.StandingCharge * request.BillingPeriod;

        var totalBeforeVAT = costOfElectricity + totalStandingCharge;

        var vatAmount = totalBeforeVAT * 0.05m;

        var totalCost = (totalBeforeVAT + vatAmount) / 100;

        return new SmartMeterCalculationResponse(Math.Round(totalCost, 2, MidpointRounding.AwayFromZero));
    }
}