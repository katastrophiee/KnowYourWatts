using KnowYourWatts.Server.Interfaces;
using System.Net;
using System.Net.Sockets;

namespace KnowYourWatts.Server;

public sealed class Server(
    IConnectionHandler connectionHandler,
    IPAddress ipAddress,
    IPEndPoint localEndPoint)
{
    private readonly IConnectionHandler _connectionHandler = connectionHandler;
    private readonly IPAddress _ipAddress = ipAddress;
    private readonly bool _runServer = true;

    public void Start()
    {
        try
        {
            var listener = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);
            listener.Listen(30);
            Console.WriteLine($"Server started at {localEndPoint.Address}:{localEndPoint.Port}");

            while (_runServer)
            {
                if (listener.Poll(1000, SelectMode.SelectRead))
                {
                    var handler = listener.Accept();
                    ThreadPool.QueueUserWorkItem(state => _connectionHandler.HandleConnection(handler));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occured when starting the server: " + ex.ToString());
        }
    }
}