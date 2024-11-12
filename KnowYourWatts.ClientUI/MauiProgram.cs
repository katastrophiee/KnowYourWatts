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

        var host = Dns.GetHostEntry("localhost");
        var ipAddress = host.AddressList[0];
        var remoteEndPoint = new IPEndPoint(ipAddress, 11000);
        //Generate random Mpan

        builder.Services.AddSingleton<IRandomisedValueProvider, RandomisedValueProvider>();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton(host);
        builder.Services.AddSingleton(remoteEndPoint);
        builder.Services.AddSingleton(ipAddress);
        builder.Services.AddSingleton<ClientSocket>();

        try
        {
            clientSocket = new(
                host,
                ipAddress,
                remoteEndPoint
                );
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occured when starting the client: " + ex.ToString());
        }
        builder.Services.AddSingleton<Socket>(sp =>
        {
            var clientSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            return clientSocket;
        });
        builder.Services.AddSingleton(clientSocket);
        clientSocket.ConnectClient();
        builder.Services.AddSingleton<IServerRequestHandler, ServerRequestHandler>();


#if DEBUG
        builder.Logging.AddDebug();
#endif


        return builder.Build();
    }



}
