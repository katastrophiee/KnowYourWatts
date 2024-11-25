using KnowYourWatts.ClientUI.DTO.Enums;
using KnowYourWatts.ClientUI.DTO.Requests;
using KnowYourWatts.ClientUI.DTO.Response;
using KnowYourWatts.ClientUI.Interfaces;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Text;

namespace KnowYourWatts.ClientUI;

public class ServerRequestHandler(
    IRandomisedValueProvider randomisedValueProvider,
    ClientSocket clientSocket) : IServerRequestHandler
{
    private readonly ClientSocket ClientSocket = clientSocket;
    private readonly IRandomisedValueProvider _randomisedValueProvider = randomisedValueProvider;
    public event Action<string> ErrorMessage;

    public async Task<SmartMeterCalculationResponse?> SendRequestToServer(
        decimal initialReading,
        RequestType requestType,
        TariffType tariffType,
        int billingPeriod,
        decimal standingCharge,
        string mpan)
    {

        var currentUsageRequest = new CurrentUsageRequest
        {
            TariffType = tariffType,
            CurrentReading = initialReading,
            BillingPeriod = billingPeriod,
            StandingCharge = standingCharge
        };

        //initialReading += _randomisedValueProvider.GenerateRandomReading();

        await SendRequest(mpan, requestType, currentUsageRequest);

        return await HandleServerResponse(currentUsageRequest.CurrentReading);
    }


    private async Task SendRequest<T>(string mpan, RequestType requestType, T request) where T : IUsageRequest
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
                RequestType = requestType,
                Data = JsonConvert.SerializeObject(request)
            };

            byte[] data = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(serverRequest));

            await ClientSocket.Socket!.SendAsync(data);
        }
        catch (Exception ex)
        {
            ErrorMessage.Invoke(ex.Message);
        }
    }

    private async Task<SmartMeterCalculationResponse?> HandleServerResponse(decimal currentCost)
    {
        try
        {
            byte[] buffer = new byte[1024];
            var bytesReceived = await ClientSocket.Socket.ReceiveAsync(buffer);

            var receivedData = Encoding.ASCII.GetString(buffer, 0, bytesReceived);

            var response = JsonConvert.DeserializeObject<SmartMeterCalculationResponse>(receivedData);

            ClientSocket.Socket.Shutdown(SocketShutdown.Both);

            if (response is null)
            {
                ErrorMessage.Invoke("No response was received from the server.");
                return null;
            }

            return response;
        }
        catch (Exception ex)
        {
            ErrorMessage.Invoke(ex.Message);
            return null;
        }

        //check the requestype
        // get the response data, convert to string
        // populate the MeterReading Model class
        //display on Screen
    }
}
