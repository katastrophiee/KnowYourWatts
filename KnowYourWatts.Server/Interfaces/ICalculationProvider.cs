﻿using KnowYourWatts.Server.DTO.Requests;
using KnowYourWatts.Server.DTO.Responses;

namespace KnowYourWatts.Server.Interfaces;

public interface ICalculationProvider
{
    SmartMeterCalculationResponse CalculateCost(SmartMeterCalculationRequest request);
}
