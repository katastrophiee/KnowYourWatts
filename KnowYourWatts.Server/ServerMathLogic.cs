using KnowYourWatts.DTO.Enums;
using KnowYourWatts.DTO.Response;

namespace KnowYourWatts.Server;

public sealed class ServerMathLogic
{
    // Dictionary containing various tariff types and their rates per KWh
    // Will want to move to 'db' eventually

    //change all doubles to decimal - avoids rounding errors
    private readonly Dictionary<string, double> tariffs = new()
    {
        //change to enum number on the left instead of string
        { "Fixed", 24.50 },
        { "Flex", 26.20 },
        { "Green", 27.05 },
        { "OffPeak", 23.64 }
    };

    public SmartMeterCalculationResponse CalculateCost(TarrifType tariffType, double energyUsage)
    {
        if (energyUsage < 0)
            throw new ArgumentOutOfRangeException(nameof(energyUsage), "Energy usage cannot be negative.");

        if (!tariffs.TryGetValue(tariffType.EnumDisplayName(), out double tariff))
            throw new KeyNotFoundException($"Tariff type '{tariffType}' does not exist.");

        double cost = energyUsage * tariff;
        return new SmartMeterCalculationResponse(cost);
    }
}