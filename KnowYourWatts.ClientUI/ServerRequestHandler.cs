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

    private bool RunProcessQueue;
    private readonly ConcurrentQueue<ServerRequest> _requestQueue = new ConcurrentQueue<ServerRequest>();
    private readonly ConcurrentDictionary<ServerRequest, TaskCompletionSource<SmartMeterCalculationResponse?>> _pendingResponses = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public ServerRequestHandler(ClientSocket clientSocket)
    {
        _clientSocket = clientSocket;
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
        var serverRequest = new ServerRequest
        {
            Mpan = mpan,
            RequestType = requestType,
            Data = JsonConvert.SerializeObject(request)
        };

        var tcs = new TaskCompletionSource<SmartMeterCalculationResponse>();
        _pendingResponses[serverRequest] =  tcs;

        //adds request in a queue and processes the requests concurrently.
        _requestQueue.Enqueue(serverRequest);
        await Task.Run(() => ProcessQueue(_cancellationTokenSource.Token));

        return await tcs.Task;
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
    private async Task ProcessQueue(CancellationToken cancellationToken)
    {

        if (!RunProcessQueue)
        {
            RunProcessQueue = true;
            while (_requestQueue.TryDequeue(out var request))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                if(_pendingResponses.TryRemove(request,out var tcs))
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
                       tcs?.SetResult(response);
                    }
                    catch (Exception ex)
                    {
                        if (tcs != null)
                        {
                            tcs.SetException(ex);
                        }
                        ErrorMessage.Invoke(ex.Message);
                    }
                    finally
                    {
                        tcs = null;


                    }
                }      
            }
            RunProcessQueue = false;
        }
    }
}
