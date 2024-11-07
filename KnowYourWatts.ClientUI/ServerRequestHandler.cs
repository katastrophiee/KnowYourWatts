using KnowYourWatts.ClientUI.DTO.Enums;
using KnowYourWatts.ClientUI.DTO.Requests;
using KnowYourWatts.ClientUI.Interfaces;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace KnowYourWatts.ClientUI;

public class ServerRequestHandler(IRandomisedValueProvider randomisedValueProvider, Socket clientSocket): IServerRequestHandler
{   
    private Socket ClientSocket = clientSocket;
    private string? Mpan;
    private readonly IRandomisedValueProvider _randomisedValueProvider = randomisedValueProvider;

    public async void SendCurrentReadingsAtRandom(decimal initialReading, RequestType requestType, TariffType tariffType)
    {
        //try
        //{

        //    await Task.Delay(delay);

        //    var currentUsageRequest = new CurrentUsageRequest
        //    {
        //        TariffType = tariffType,
        //        CurrentReading = initialReading
        //    };

        //    initialReading += (decimal)random.NextDouble();
        //    //example. Will have if statements to send different request types based on the tab selected on the main window.
        //    SendRequest(requestType, currentUsageRequest);
        //}
        //catch
        //{

        //}
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
                //ConnectToServer();
            }

            if (Mpan is null || string.IsNullOrEmpty(Mpan))
            {
               _randomisedValueProvider.GenerateMpanForClient();
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

}
