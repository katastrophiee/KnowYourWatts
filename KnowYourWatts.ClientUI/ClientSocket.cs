using System.Net;
using System.Net.Sockets;

namespace KnowYourWatts.ClientUI;

public class ClientSocket(
    IPAddress ipAddress,
    IPEndPoint remoteEndPoint)
{
    public Socket Socket { get; set; }
    public IPAddress IPAddress { get; set; } = ipAddress;
    public IPEndPoint RemoteEndPoint { get; set; } = remoteEndPoint;


    public async Task ConnectClientToServer()
    {
        try
        {
            Socket = new Socket(IPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            for (var retryCount = 0; retryCount < 5; retryCount++)
            {
                if (!Socket.Connected)
                {
                    // Connect to the endpoint on the server
                    await Socket.ConnectAsync(RemoteEndPoint);
                }
                else
                {
                    Console.WriteLine("Successfully connected to the server.");
                    return;
                }
            }

            Console.WriteLine("The client could not connect to the server.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occured when starting the client: " + ex.ToString());
        }
    }
}
