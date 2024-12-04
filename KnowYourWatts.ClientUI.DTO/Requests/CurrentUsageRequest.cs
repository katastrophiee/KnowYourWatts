﻿using KnowYourWatts.ClientUI.DTO.Enums;

namespace KnowYourWatts.ClientUI.DTO.Requests;

public sealed class CurrentUsageRequest(TariffType tariffType, decimal currentReading, decimal currentCost, int billingPeriod, decimal standingCharge) : IUsageRequest
{
    public TariffType TariffType { get; set; } = tariffType;

    public decimal CurrentReading { get; set; } = currentReading;

    public decimal CurrentCost { get; set; } = currentCost;

    public int BillingPeriod { get; set; } = billingPeriod;

    public decimal StandingCharge { get; set; } = standingCharge;
}
