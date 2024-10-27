using KnowYourWatts.DTO.Response;
using KnowYourWatts.Server.DTO.Enums;
using KnowYourWatts.Server.Interfaces;

namespace KnowYourWatts.Server;

public sealed class CalculationProvider : ICalculationProvider
{
    // Dictionary containing various tariff types and their rates per KWh
    // Will want to move to 'db' eventually

    //change all decimals to decimal - avoids rounding errors
    private readonly Dictionary<TariffType, decimal> tariffs = new()
    {
        { TariffType.Fixed, 24.50m },
        { TariffType.Flex, 26.20m },
        { TariffType.Green, 27.05m },
        { TariffType.OffPeak, 23.64m }
    };

    public SmartMeterCalculationResponse CalculateCost(TariffType tariffType, decimal energyUsage)
    {
        if (energyUsage < 0)
            throw new ArgumentOutOfRangeException(nameof(energyUsage), "Energy usage cannot be negative.");

        if (!tariffs.TryGetValue(tariffType, out decimal tariff))
            throw new KeyNotFoundException($"Tariff type '{tariffType}' does not exist.");

        decimal cost = energyUsage * tariff;
        return new SmartMeterCalculationResponse(cost);
    }
}