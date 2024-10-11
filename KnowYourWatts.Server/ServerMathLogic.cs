using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowYourWatts.Server
{
    internal class ServerMathLogic
    {
        // Dictionary containing various tariff types (string) and their rates per KWh
        private Dictionary<string, double> tariffs = new Dictionary<string, double>
        {
            { "Fixed", 24.50 },
            { "Flex", 26.20 },
            { "Green", 27.05 },
            { "OffPeak", 23.64 }
        };

        private double GetTariff(string tariffType)
        {
            if(tariffs.ContainsKey(tariffType))
            {
                return tariffs[tariffType];
            }
            else
            {
                throw new ArgumentException($"Tariff type '{tariffType}' does not exist.");
            }
        }

        public double CalculateCost(string tariffType, double energyUsage)
        {
            double tariff = GetTariff(tariffType);
            double cost = energyUsage * tariff;
            return cost;
        }
    }
}
