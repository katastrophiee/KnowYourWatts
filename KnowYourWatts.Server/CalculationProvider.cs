using KnowYourWatts.MockDb.Interfaces;
using KnowYourWatts.Server.DTO.Requests;
using KnowYourWatts.Server.DTO.Response;
using KnowYourWatts.Server.Interfaces;

namespace KnowYourWatts.Server;

public sealed class CalculationProvider(
    ITariffRepository tariffRepository,
    IPreviousReadingRepository previousReadingRepository,
    ICostRepository costRepository) : ICalculationProvider
{
    private readonly ITariffRepository _tariffRepository = tariffRepository;
    private readonly IPreviousReadingRepository  _previousReadingRepository = previousReadingRepository;
    private readonly ICostRepository _costRepository = costRepository;

    public SmartMeterCalculationResponse CalculateCost(SmartMeterCalculationRequest request)
    {
        var previousReading = _previousReadingRepository.GetPreviousReadingByMpan(request.Mpan);

        if (previousReading is not null && request.CurrentReading < previousReading)
            return new("Current energy reading cannot be less than previous energy reading.");

        var tarrifPrice = _tariffRepository.GetTariffPriceByType(request.TariffType);

        if (tarrifPrice is null)
            return new($"Tariff type '{request.TariffType}' does not exist.");

        //We use the difference between the current and previous readings to get the energy used
        var energyUsed = previousReading is not null 
            ? request.CurrentReading - previousReading.Value
            : request.CurrentReading;

        //We then calculate the cost of the electricity used using the tariff type
        var costOfElectricity = energyUsed * tarrifPrice.PriceInPence;

        //We then calculate the total standing charge for the billing period
        var totalStandingCharge = request.StandingCharge * request.BillingPeriod;

        //We add the two to get the total cost before VAT
        var totalBeforeVat = costOfElectricity + totalStandingCharge;

        //Then we calculate the VAT amount
        var vatAmount = totalBeforeVat * 0.05m;

        //Finally we add the VAT amount to the total cost before VAT and divide by 100 to get the total cost in pounds
        var totalCostWithVat = (totalBeforeVat + vatAmount) / 100;

        //We round the total cost to 2 decimal places to match what the website returns
        var totalCost = Math.Round(totalCostWithVat, 2, MidpointRounding.AwayFromZero);

        //We pass the cost calculated here to the mock database to add it to the existing cost and save it
        _costRepository.AddOrUpdateClientTotalCost(request.Mpan, totalCost);

        //We need to save the a new previous reading to the mock database for the next time we calculate the cost
        _previousReadingRepository.AddOrUpdatePreviousReading(request.Mpan, request.CurrentReading);

        return new SmartMeterCalculationResponse(totalCost);
    }
}