using KnowYourWatts.ClientUI.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net;

namespace KnowYourWatts.ClientUI;

public static class MauiProgram
{
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

        builder.Services.AddScoped<IRandomisedValueProvider, RandomisedValueProvider>();
        builder.Services.AddSingleton<IServerRequestHandler, ServerRequestHandler>();
        builder.Services.AddSingleton<IEncryptionHelper, EncryptionHelper>();

        try
        {
            var clientSocket = new ClientSocket(
                ipAddress,
                remoteEndPoint
            );

            builder.Services.AddSingleton(clientSocket);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occured when starting the client: {ex}");
        }
    }
}
