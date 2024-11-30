using System.Text;

namespace KnowYourWatts.ClientUI;

class RandomisedValueProvider : IRandomisedValueProvider
{ 
    public string GenerateMpanForClient()
    {
        var random = new Random();

        var mpan = new StringBuilder();

        for (int i = 0; i < 13; i++)
        {
            mpan.Append(random.Next(0, 9));
        }
        return mpan.ToString();
       
    }
    public decimal GenerateRandomReading()
    {
        var random = new Random();

        var reading = random.NextDouble();

        return (decimal)reading;
    }

    public int GenerateRandomTimeDelay()
    {
        var random = new Random();

        int delay = random.Next(15000, 60000);

        return delay;
    }

    public int GenerateRandomTarrif()
    {
        var random = new Random();
        int tarrif = random.Next(0, 3);
        return tarrif;
    }

    public decimal GenerateRandomStandingCharge()
    {
        var random = new Random();
        double standingCharge = random.NextDouble();
        return (decimal)standingCharge;
    }
}
