﻿using KnowYourWatts.Server.DTO.Enums;

namespace KnowYourWatts.Server.DTO.Interfaces;

public interface IUsageRequest
{
    public TariffType TariffType { get; set; }

    public decimal CurrentReading { get; set; }

    public decimal CurrentCost { get; set; }

    public decimal StandingCharge { get; set; }

    public int BillingPeriod { get; set; }
}
