namespace KnowYourWatts.Server.DTO.Models;

public sealed class PreviousReading
{
    public string Mpan { get; set; }

    public decimal PreviousUsage { get; set; }
}
