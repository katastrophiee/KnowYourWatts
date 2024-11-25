﻿using KnowYourWatts.ClientUI.DTO.Enums;
using KnowYourWatts.ClientUI.DTO.Response;

namespace KnowYourWatts.ClientUI.Interfaces;

public interface IServerRequestHandler
{
    Task<SmartMeterCalculationResponse> SendRequestToServer(decimal initialReading, RequestType requestType, TariffType tariffType, int billingPeriod, decimal standingCharge, string mpan, byte[] encryptedMpan);

    Task<string> GetPublicKey(string mpan);
}
