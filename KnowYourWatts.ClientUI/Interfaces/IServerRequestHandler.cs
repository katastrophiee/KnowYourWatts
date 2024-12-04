using KnowYourWatts.ClientUI.DTO.Enums;
using KnowYourWatts.ClientUI.DTO.Response;

namespace KnowYourWatts.ClientUI.Interfaces;

public interface IServerRequestHandler
{
    event Action<string> ErrorMessage;

    Task<SmartMeterCalculationResponse?> SendRequestToServer(decimal initialReading, decimal currentCost, RequestType requestType, TariffType tariffType, int billingPeriod, decimal standingCharge, string mpan);
}
