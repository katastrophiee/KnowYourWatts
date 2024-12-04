using System.Net;
using System.Net.Security;
using System.Net.Sockets;

namespace KnowYourWatts.ClientUI;

public class ClientSocket
{
    public SslStream SslStream { get; set; } = null!;
    public string ErrorMessage { get; set; } = "";

    public async Task ConnectClientToServer()
    {
        try
        {
            var ipAddress = Dns.GetHostEntry("localhost").AddressList[0];
            var remoteEndPoint = new IPEndPoint(ipAddress, 11000);

            var socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            for (var retryCount = 0; retryCount < 5; retryCount++)
            {
                if (!socket.Connected)
                {
                    // Connect to the endpoint on the server
                    await socket.ConnectAsync(remoteEndPoint);
                }
                else
                {
                    var networkStream = new NetworkStream(socket, ownsSocket: true);
                    SslStream = new SslStream(networkStream, leaveInnerStreamOpen: false, (sender, certificate, chain, sslPolicyErrors) => true);
                    Console.WriteLine("Successfully connected to the server.");
                    return;
                }
            }

            ErrorMessage = "The client could not connect to the server.";
        }
        catch (Exception ex)
        {
            ErrorMessage = "An error occured when trying to connect to the server: " + ex.ToString();
        }
    }
}
