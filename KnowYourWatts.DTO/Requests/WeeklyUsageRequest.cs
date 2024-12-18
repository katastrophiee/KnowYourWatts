﻿using KnowYourWatts.Server.DTO.Enums;
using KnowYourWatts.Server.DTO.Interfaces;

namespace KnowYourWatts.Server.DTO.Requests;

public class WeeklyUsageRequest : IUsageRequest
{
    public TariffType TariffType { get; set; }

    public decimal CurrentReading { get; set; }

    public decimal CurrentCost { get; set; }

    public int BillingPeriod { get; set; }

    public decimal StandingCharge { get; set; }
}
