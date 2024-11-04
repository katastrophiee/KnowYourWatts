using KnowYourWatts.ClientUI.DTO.Enums;
using KnowYourWatts.ClientUI.DTO.Requests;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace KnowYourWatts.ClientUI;

public class ServerRequestHandler
{
    private Socket ClientSocket = null!;
    private string? Mpan;

    public void ConnectToServer()
    {
        try
        {
            // Get host and create a new socket
            var host = Dns.GetHostEntry("localhost");
            var ipAddress = host.AddressList[0];
            var remoteEndPoint = new IPEndPoint(ipAddress, 11000);

            ClientSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            // Connect to endpoint
            ClientSocket.Connect(remoteEndPoint);

            GenerateMpanForClient();
        }
        catch (Exception ex)
        {
            
        }
    }


    public async void SendCurrentReadingsAtRandom(decimal initialReading, RequestType requestType, TariffType tariffType)
    {
        var random = new Random();

        try
        {
            int delay = random.Next(15000, 60000);
            await Task.Delay(delay);

            var currentUsageRequest = new CurrentUsageRequest
            {
                TariffType = tariffType,
                CurrentReading = initialReading
            };

            initialReading += (decimal)random.NextDouble();
            //example. Will have if statements to send different request types based on the tab selected on the main window.
            SendRequest(requestType, currentUsageRequest);
        }
        catch
        {

        }
    }

    public void SendRequest<T>(RequestType requestType, T request) where T : IUsageRequest
    {
        try
        {
            // If the socket is null and the socket is not connected, throw an invalid operation exception (method cannot be performed).
            // Use this exception as you wish, this is a basic implementation.
            if (ClientSocket?.Connected != true)
            {
                //try and reconnect instead of showing error.
                ConnectToServer();
            }

            if (Mpan is null || string.IsNullOrEmpty(Mpan))
            {
                GenerateMpanForClient();
            }

            // Create a new request.
            var serverRequest = new ServerRequest
            {
                Mpan = Mpan,
                RequestType = requestType,
                Data = JsonConvert.SerializeObject(request)
            };

            string dataToSend = JsonConvert.SerializeObject(serverRequest);
            byte[] data = Encoding.UTF8.GetBytes(dataToSend);
            ClientSocket.SendAsync(data);
        }
        catch (Exception ex)
        {
            
        }
    }

    private void GenerateMpanForClient()
    {
        var random = new Random();

        var mpan = new StringBuilder();

        for (int i = 0; i < 13; i++)
        {
            mpan.Append(random.Next(0, 9));
        }

        Mpan = mpan.ToString();
    }
}
