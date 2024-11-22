using KnowYourWatts.Server.DTO.Requests;
using KnowYourWatts.Server.DTO.Responses;

namespace KnowYourWatts.Server.Interfaces;

public interface ICalculationProvider
{
    CalculationResponse CalculateCost(SmartMeterCalculationRequest request);
}
