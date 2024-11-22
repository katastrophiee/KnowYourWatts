using KnowYourWatts.ClientUI.DTO.Enums;
using KnowYourWatts.ClientUI.DTO.Requests;

namespace KnowYourWatts.ClientUI.Interfaces;

public interface IServerRequestHandler
{
    void SendRequest<T>(RequestType requestType, T request) where T : IUsageRequest;
    void CreateRequest(decimal initialReading, RequestType requestType, TariffType tariffType, int billingPeriod, decimal standingCharge);
    decimal? GetResponse();
}
