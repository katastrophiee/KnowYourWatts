using KnowYourWatts.ClientUI.DTO.Enums;
using KnowYourWatts.ClientUI.DTO.Requests;
using KnowYourWatts.ClientUI.DTO.Response;
using KnowYourWatts.ClientUI.Interfaces;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;

namespace KnowYourWatts.ClientUI;

public class ServerRequestHandler : IServerRequestHandler
{
    private readonly ClientSocket _clientSocket;
    public event Action<string> ErrorMessage = null!;
    private readonly bool RunProcessQueue = true;
    private readonly BlockingCollection<ServerRequest> _requestQueue = [];
    private TaskCompletionSource<SmartMeterCalculationResponse?>? _response = null!;

    public ServerRequestHandler(ClientSocket clientSocket)
    {
        _clientSocket = clientSocket;
        Task.Run(ProcessQueue);
    }

    // Entry point for the UI to send a request to the server
    public async Task<SmartMeterCalculationResponse?> SendRequestToServer(
        decimal initialReading,
        decimal currentCost,
        RequestType requestType,
        TariffType tariffType,
        int billingPeriod,
        decimal standingCharge,
        string mpan)
    {
        //Create the request for the data
        var request = new CurrentUsageRequest(tariffType, initialReading, currentCost, billingPeriod, standingCharge);

        _response = new TaskCompletionSource<SmartMeterCalculationResponse?>();

        QueueRequestForServer(mpan, requestType, request);

        return await _response.Task;
    }


    private async Task SendRequest<T>(string mpan, RequestType requestType, T request) where T : IUsageRequest
    {
        
        try
        {
            // Create the request for the server
            var serverRequest = new ServerRequest
            {
                Mpan = mpan,
                RequestType = requestType,
                Data = JsonConvert.SerializeObject(request)
            };

            // Add it to the queue
            _requestQueue.Add(serverRequest);
        }
        catch (Exception ex)
        {
            ErrorMessage.Invoke(ex.Message);
        }
    }

    private async Task<SmartMeterCalculationResponse?> HandleServerResponse()
    {
        try
        {
            // Receive the response from the server
            byte[] buffer = new byte[1024];
            var bytesReceived = await _clientSocket.Socket.ReceiveAsync(buffer);

            var receivedData = Encoding.ASCII.GetString(buffer, 0, bytesReceived);

            var response = JsonConvert.DeserializeObject<SmartMeterCalculationResponse>(receivedData);

            // Close the connection
            _clientSocket.Socket.Shutdown(SocketShutdown.Both);

            // Check the response is not null
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
    }

    private async Task ProcessQueue()
    {
        // Ensure the queue is always being processed
        while (RunProcessQueue)
        {
            // Iterate through each item in the queue
            foreach (var request in _requestQueue.GetConsumingEnumerable())
            {
                try
                {
                    // Connect to the server
                    await _clientSocket.ConnectClientToServer();

                    // Send our request to the server
                    var data = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(request));
                    await _clientSocket.Socket.SendAsync(data);

                    // Handle the response from the server
                    var response = await HandleServerResponse();

                    // Set the response to this task as the request has been completed
                    _response?.SetResult(response);
                }
                catch (Exception ex)
                {
                    _response = null;
                    ErrorMessage.Invoke(ex.Message);
                }
                finally
                {
                    _response = null;
                }
            }
        }
    }
}
