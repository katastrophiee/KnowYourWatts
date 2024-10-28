﻿using KnowYourWatts.Server.Interfaces;
using System.Net;
using Microsoft.Extensions.DependencyInjection;

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

        var host = Dns.GetHostEntry("localhost");
        var ipAddress = host.AddressList[0];
        var localEndPoint = new IPEndPoint(ipAddress, 11000);

        services.AddScoped<IConnectionHandler, ConnectionHandler>();
        services.AddScoped<ICalculationProvider, CalculationProvider>();

        var serviceProvider = services.BuildServiceProvider();

        try
        {
            Server = new(
                serviceProvider.GetRequiredService<IConnectionHandler>(),
                host,
                ipAddress,
                localEndPoint
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while setting up the server. {ex}");
        }
    }
}