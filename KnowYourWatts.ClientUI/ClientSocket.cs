using System.Net;
using System.Net.Sockets;

namespace KnowYourWatts.ClientUI;

public class ClientSocket (
    IPHostEntry host,
    IPAddress ipAddress,
    IPEndPoint remoteEndPoint)
{
    private readonly IPEndPoint _remoteEndPoint = remoteEndPoint;
    private readonly IPAddress _ipAddress = ipAddress;
    private readonly IPHostEntry _ipHostEntry = host;

    public void ConnectClient()
    {
        try
        {
            var clientSocket = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            // Connect to endpoint
            clientSocket.Connect(remoteEndPoint);
            if (clientSocket.Connected) 
            {
                Console.WriteLine("Connected");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occured when starting the client: " + ex.ToString());
        }
    }
}
