﻿using KnowYourWatts.Server.DTO.Enums;

namespace KnowYourWatts.Server.DTO.Models;

public sealed class PreviousReading
{
    public string Mpan { get; set; }

    public decimal PreviousUsage { get; set; }

    public DateTime ResetDate { get; set; }

    public RequestType RequestType { get; set; }
}
