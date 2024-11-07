using KnowYourWatts.ClientUI.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;

namespace KnowYourWatts.ClientUI;

public static class MauiProgram
{
    private static ClientSocket clientSocket;

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

        //var host = Dns.GetHostEntry("localhost");
        //var ipAddress = host.AddressList[0];
        //var remoteEndPoint = new IPEndPoint(ipAddress, 11000);

        //clientSocket.ConnectClient();
        //builder.Services.AddSingleton(clientSocket);
        //builder.Services.AddSingleton<IRandomisedValueProvider, RandomisedValueProvider>();
        //builder.Services.AddSingleton<IServerRequestHandler, ServerRequestHandler>();
        //builder.Services.AddSingleton<MainPage>();

        //try
        //{
        //    clientSocket = new(
        //        host,
        //        ipAddress,
        //        remoteEndPoint
        //        );
        //}
        //catch (Exception ex) 
        //{
        //    Console.WriteLine("An error occured when starting the client: " + ex.ToString());
        //}

#if DEBUG
        builder.Logging.AddDebug();
#endif


        return builder.Build();
    }



}
