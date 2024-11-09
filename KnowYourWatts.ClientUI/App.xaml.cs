using System.Net;

namespace KnowYourWatts.ClientUI;

public partial class App : Application
{
    private readonly ClientSocket _clientSocket;
    public App(ClientSocket clientSocket)
    {
        InitializeComponent();
        /*var host = Dns.GetHostEntry("localhost");
        var ipAddress = host.AddressList[0];
        var remoteEndPoint = new IPEndPoint(ipAddress, 11000);
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
        }*/
        _clientSocket = clientSocket;
        _clientSocket.ConnectClient();
        MainPage = new AppShell();

    }
}
