using KnowYourWatts.ClientUI.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net;

namespace KnowYourWatts.ClientUI;

public static class MauiProgram
{
    private static ClientSocket ClientSocket;

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<MainPage>();

        BuildClientDependencies(ref builder);

        #if DEBUG
            builder.Logging.AddDebug();
        #endif

        return builder.Build();
    }

    private static void BuildClientDependencies(ref MauiAppBuilder builder)
    {
        var host = Dns.GetHostEntry("localhost");
        var ipAddress = host.AddressList[0];
        var remoteEndPoint = new IPEndPoint(ipAddress, 11000);
        //Generate random Mpan

        builder.Services.AddScoped<IRandomisedValueProvider, RandomisedValueProvider>();
        builder.Services.AddSingleton<IServerRequestHandler, ServerRequestHandler>();

        try
        {
            ClientSocket = new(
                ipAddress,
                remoteEndPoint
            );

            //await ClientSocket.ConnectClientToServer();

            builder.Services.AddSingleton(ClientSocket);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occured when starting the client: {ex}");
        }
    }
}
