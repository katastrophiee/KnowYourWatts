using KnowYourWatts.DTO.Response;
using KnowYourWatts.Server.DTO.Enums;

namespace KnowYourWatts.Server.Interfaces;

public interface ICalculationProvider
{
    SmartMeterCalculationResponse CalculateCost(TariffType tariffType, decimal energyUsage);
}
