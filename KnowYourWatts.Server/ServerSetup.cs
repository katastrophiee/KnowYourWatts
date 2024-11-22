using KnowYourWatts.Server.Interfaces;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using KnowYourWatts.MockDb.Interfaces;
using KnowYourWatts.MockDb.Repository;

namespace KnowYourWatts.Server;

public sealed class ServerSetup
{
    private static Server? Server;

    public static void Main()
    {
        BuildDependencies();
        Server?.Start();
    }

    private static void BuildDependencies()
    {
        var services = new ServiceCollection();

        var ipAddress = Dns.GetHostEntry("localhost").AddressList[0];
        var localEndPoint = new IPEndPoint(ipAddress, 11000);

        services.AddScoped<IConnectionHandler, ConnectionHandler>();
        services.AddScoped<ICalculationProvider, CalculationProvider>();
        services.AddScoped<IPreviousReadingRepository, PreviousReadingRepository>();
        services.AddScoped<ITariffRepository, TariffRepository>();
        services.AddScoped<ICostRepository, CostRepository>();
        services.AddScoped<IKeyHandler, KeyHandler>();

        var serviceProvider = services.BuildServiceProvider();

        try
        {
            Server = new(
                serviceProvider.GetRequiredService<IConnectionHandler>(),
                serviceProvider.GetRequiredService<IKeyHandler>(),
                ipAddress,
                localEndPoint
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while setting up the server: {ex}");
        }
    }
}
