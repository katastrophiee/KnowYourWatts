using System.Net;
using System.Net.Sockets;

namespace KnowYourWatts.Server;

public class Server(IPHostEntry host, IPAddress ipAddress, IPEndPoint localEndPoint)
{
    private readonly IPHostEntry Host = host ?? throw new ArgumentNullException(nameof(host));
    private readonly IPAddress IpAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));
    private readonly IPEndPoint LocalEndPoint = localEndPoint ?? throw new ArgumentNullException(nameof(localEndPoint));
    private Socket Listener;
    private bool RunServer;

    public void Start()
    {
        try
        {
            Listener = new Socket(IpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Listener.Bind(LocalEndPoint);
            Listener.Listen(30);
            RunServer = true;

            Console.WriteLine($"Server started at {LocalEndPoint.Address}:{LocalEndPoint.Port}");

            while (RunServer)
            {
                if (Listener.Poll(1000, SelectMode.SelectRead))
                {
                    var handler = Listener.Accept();
                    ThreadPool.QueueUserWorkItem(state => HandleClient(handler));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occured when starting the server: " + ex.ToString());
        }
    }

    private static void HandleClient(Socket handler)
    {
        var connectionHandler = new ConnectionHandler(handler);
        connectionHandler.HandleConnection();
    }

    public void Stop()
    {
        RunServer = false;
        Listener.Close();
        Console.WriteLine("Server stopped.");
    }
}