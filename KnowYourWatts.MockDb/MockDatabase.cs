using KnowYourWatts.Server.DTO.Enums;
using KnowYourWatts.Server.DTO.Models;
using Microsoft.EntityFrameworkCore;

namespace KnowYourWatts.MockDb;

public sealed class MockDatabase : DbContext
{
    public DbSet<PreviousReading> PreviousReadings { get; set; }
    public DbSet<TariffTypeAndPrice> TarrifType { get; set; }

    //change bc is db
    protected override void OnConfiguring(DbContextOptionsBuilder builder) => builder.UseInMemoryDatabase("MockSmartMeterDb");

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<PreviousReading>().HasKey(x => x.Mpan);
        builder.Entity<TariffTypeAndPrice>().HasKey(x => x.TariffType);

        builder.Entity<PreviousReading>().HasData(
            new PreviousReading { Mpan = "1234567890123", PreviousUsage = 23.43m },
            new PreviousReading { Mpan = "0987654321098", PreviousUsage = 65.20m }
        );

        // Mock database table containing various tariff types and their rates per KWh
        builder.Entity<TariffTypeAndPrice>().HasData(
            new TariffTypeAndPrice { TariffType = TariffType.Fixed, PriceInPence = 24.50m },
            new TariffTypeAndPrice { TariffType = TariffType.Flex, PriceInPence = 26.20m },
            new TariffTypeAndPrice { TariffType = TariffType.Green, PriceInPence = 27.05m },
            new TariffTypeAndPrice { TariffType = TariffType.OffPeak, PriceInPence = 23.64m }
        );
    }
}
