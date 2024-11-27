using KnowYourWatts.ClientUI.DTO.Enums;
using KnowYourWatts.ClientUI.DTO.Requests;
using KnowYourWatts.ClientUI.DTO.Response;
using KnowYourWatts.ClientUI.Interfaces;
using Newtonsoft.Json;
using System.Collections.Concurrent;
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
    private static ConcurrentQueue<byte[]> requestQueue = new ConcurrentQueue<byte[]>();
    private static ConcurrentQueue<CurrentUsageRequest> currentRequestQueue = new ConcurrentQueue<CurrentUsageRequest>();
    private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
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

        /*await Task.Run(async () =>
        {
            await SendRequest(mpan, requestType, currentUsageRequest);
        });*/
        /*ThreadPool.QueueUserWorkItem(async _ =>
        {
            await SendRequest(mpan, requestType, currentUsageRequest);
        });*/
        /* var response = new SmartMeterCalculationResponse();
         ThreadPool.QueueUserWorkItem(async _ =>
         {
             response = await HandleServerResponse(currentUsageRequest.CurrentReading);
         });*/
        await SendRequest(mpan, requestType, currentUsageRequest);
        currentRequestQueue.Enqueue(currentUsageRequest);
        if (currentRequestQueue.TryDequeue(out currentUsageRequest))
        {
            await semaphore.WaitAsync();
            try
            {
                await SendRequest(mpan, requestType, currentUsageRequest);

            }
            finally
            {
                semaphore.Release();
            }
        }

        return await HandleServerResponse(currentUsageRequest.CurrentReading);

    }

    /*//https://stackoverflow.com/questions/59186013/how-to-call-queue-client-sendasync-method-using-c-sharp*/
    private async Task SendRequest<T>(string mpan, RequestType requestType, T request) where T : IUsageRequest
    {
        
        try
        {
            // If the socket is null and the socket is not connected, throw an invalid operation exception (method cannot be performed).
            // Use this exception as you wish, this is a basic implementation.
            if (ClientSocket.Socket == null || !ClientSocket.Socket.Connected) 
            {
                await ClientSocket.ConnectClientToServer();
            }
              
            

            // Create a new request.
            var serverRequest = new ServerRequest
            {
                Mpan = mpan,
                RequestType = requestType,
                Data = JsonConvert.SerializeObject(request)
            };

            byte[] data = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(serverRequest));
            requestQueue.Enqueue(data);
            while (requestQueue.TryDequeue(out data))
            {
                await semaphore.WaitAsync();
                try
                {
                    await ClientSocket.Socket!.SendAsync(data);

                }
                finally
                {
                    semaphore.Release();
                }
                //await ClientSocket.Socket!.SendAsync(data);
            }
            //ThreadPool.QueueUserWorkItem(state => _connectionHandler.HandleConnection(handler));
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

            //await ClientSocket.ConnectClientToServer();
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
