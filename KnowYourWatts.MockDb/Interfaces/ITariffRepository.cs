using KnowYourWatts.Server.DTO.Enums;
using KnowYourWatts.Server.DTO.Models;

namespace KnowYourWatts.MockDb.Interfaces;

public interface ITariffRepository
{
    TariffTypeAndPrice? GetTariffPriceByType(TariffType tariffType);
}
