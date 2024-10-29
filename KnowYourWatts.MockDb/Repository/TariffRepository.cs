using KnowYourWatts.MockDb.Interfaces;
using KnowYourWatts.Server.DTO.Enums;
using KnowYourWatts.Server.DTO.Models;

namespace KnowYourWatts.MockDb.Repository;

public class TariffRepository(MockDatabase mockDbContext) : ITariffRepository
{
    private readonly MockDatabase _mockDbContext = mockDbContext;

    public TariffTypeAndPrice? GetTariffByType(TariffType tariffType)
    {
        return _mockDbContext.TarrifType.FirstOrDefault(t => t.TariffType == tariffType);
    }
}
