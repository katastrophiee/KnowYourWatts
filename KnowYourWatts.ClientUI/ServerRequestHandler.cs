using KnowYourWatts.ClientUI.DTO.Enums;
using KnowYourWatts.ClientUI.DTO.Requests;
using KnowYourWatts.ClientUI.DTO.Response;
using KnowYourWatts.ClientUI.Interfaces;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace KnowYourWatts.ClientUI;

public class ServerRequestHandler(
    IRandomisedValueProvider randomisedValueProvider,
    ClientSocket clientSocket): IServerRequestHandler
{   
    private readonly IRandomisedValueProvider _randomisedValueProvider = randomisedValueProvider;
    private readonly ClientSocket ClientSocket = clientSocket;
    private string PublicKey;

    public async Task<SmartMeterCalculationResponse> SendRequestToServer(
        decimal initialReading,
        RequestType requestType,
        TariffType tariffType,
        int billingPeriod,
        decimal standingCharge,
        string mpan,
        byte[] encryptedMpan)
    {
        var currentUsageRequest = new CurrentUsageRequest
        {
            TariffType = tariffType,
            CurrentReading = initialReading,
            BillingPeriod = billingPeriod,
            StandingCharge = standingCharge
        };

        //initialReading += _randomisedValueProvider.GenerateRandomReading();

        await SendRequest(mpan, encryptedMpan, requestType, currentUsageRequest);

        return await HandleServerResponse(currentUsageRequest.CurrentReading);
    }

    public async Task<string> GetPublicKey(string mpan)
    {
        await ClientSocket.ConnectClientToServer();

        var request = new ServerRequest
        {
            Mpan = mpan,
            EncryptedMpan = Array.Empty<byte>(),
            RequestType = RequestType.PublicKey,
            Data = ""
        };

        byte[] data = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(request));

        await ClientSocket.Socket!.SendAsync(data);

        byte[] buffer = new byte[1024];
        var bytesReceived = await ClientSocket.Socket.ReceiveAsync(buffer);

        var response = Encoding.ASCII.GetString(buffer, 0, bytesReceived);

        PublicKey = response;

        if (response is null)
        {
            //return new();
            // change to error in future
        }

        ClientSocket.Socket.Shutdown(SocketShutdown.Both);

        return PublicKey;

        //return response;
    }


    private async Task SendRequest<T>(string mpan, byte[] encryptedMpan, RequestType requestType, T request) where T : IUsageRequest
    {
        try
        {
            // If the socket is null and the socket is not connected, throw an invalid operation exception (method cannot be performed).
            // Use this exception as you wish, this is a basic implementation.

            await ClientSocket.ConnectClientToServer();

            // Create a new request.
            var serverRequest = new ServerRequest
            {
                Mpan = mpan,
                EncryptedMpan = encryptedMpan,
                RequestType = requestType,
                Data = JsonConvert.SerializeObject(request)
            };

            byte[] data = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(serverRequest));

            await ClientSocket.Socket!.SendAsync(data);
        }
        catch (Exception ex)
        {
            //error page
        }
    }

    private async Task<SmartMeterCalculationResponse> HandleServerResponse(decimal currentCost)
    {
        try
        {
            byte[] buffer = new byte[1024];
            var bytesReceived = await ClientSocket.Socket.ReceiveAsync(buffer);

            var receivedData = Encoding.ASCII.GetString(buffer, 0, bytesReceived);

            var response = JsonConvert.DeserializeObject<SmartMeterCalculationResponse>(receivedData);

            if (response is null)
            {
                return new();
                // change to error in future
            }

            ClientSocket.Socket.Shutdown(SocketShutdown.Both);

            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return new();
        }

        //check the requestype
        // get the response data, convert to string
        // populate the MeterReading Model class
        //display on Screen
    }
}
