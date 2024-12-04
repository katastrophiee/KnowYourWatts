using KnowYourWatts.ClientUI.Interfaces;
using System.Text;

namespace KnowYourWatts.ClientUI;

class RandomisedValueProvider : IRandomisedValueProvider
{
    private readonly Random _random = new();

    public string GenerateMpanForClient()
    {
        var mpan = new StringBuilder();

        for (int i = 0; i < 13; i++)
        {
            mpan.Append(_random.Next(0, 9));
        }

        return mpan.ToString();
    }
    public decimal GenerateRandomReading()
    {
        var reading = _random.NextDouble();

        return (decimal)reading;
    }

    public int GenerateRandomTimeDelay()
    {
        return _random.Next(15000, 60000);
    }

    public int GenerateRandomTariff()
    {
        return _random.Next(0, 3);
    }

    public decimal GenerateRandomStandingCharge()
    {
        var standingCharge = _random.NextDouble();

        return (decimal)standingCharge;
    }
}
