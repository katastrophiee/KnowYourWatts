using KnowYourWatts.ClientUI.DTO.Enums;
using KnowYourWatts.ClientUI.DTO.Requests;
using KnowYourWatts.ClientUI.DTO.Response;
using KnowYourWatts.ClientUI.Interfaces;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace KnowYourWatts.ClientUI;

public class ServerRequestHandler(
    IRandomisedValueProvider randomisedValueProvider,
    ClientSocket clientSocket,
    IEncryptionHelper encryptionHelper): IServerRequestHandler
{   
    private readonly IRandomisedValueProvider _randomisedValueProvider = randomisedValueProvider;
    private readonly ClientSocket ClientSocket = clientSocket;
    private readonly IEncryptionHelper _encryptionHelper = encryptionHelper;
    private string PublicKey = "";
    public event Action<string> ErrorMessage= null;

    private bool RunProcessQueue;
    private readonly ConcurrentQueue<ServerRequest> _requestQueue = new ConcurrentQueue<ServerRequest>();
    private readonly ConcurrentDictionary<ServerRequest, TaskCompletionSource<SmartMeterCalculationResponse?>> _pendingResponses = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

   
    // Entry point for the UI to send a request to the server
    public async Task<SmartMeterCalculationResponse?> SendRequestToServer(
        decimal initialReading,
        decimal currentCost,
        RequestType requestType,
        TariffType tariffType,
        int billingPeriod,
        decimal standingCharge,
        string mpan,
        byte[] encryptedMpan)
    {

        var currentUsageRequest = new CurrentUsageRequest(tariffType, initialReading, currentCost, billingPeriod, standingCharge);
       

        // Tries to get the public key 5 times and errors if we can't
        for (var retryCount = 0; retryCount < 5; retryCount++)
        {
            if (string.IsNullOrEmpty(PublicKey))
            {
                await GetPublicKey(mpan);
            }
            else
            {
                break;
            }
        }

        // Error here
        if (string.IsNullOrEmpty(PublicKey))
        {
            ErrorMessage.Invoke("Failed to retrieve public key.");
           
        }

         encryptedMpan = _encryptionHelper.EncryptData(Encoding.ASCII.GetBytes(mpan), PublicKey);

        // Create a new request.
        var serverRequest = new ServerRequest
        {
            Mpan = mpan,
            EncryptedMpan = encryptedMpan,
            RequestType = requestType,
            Data = JsonConvert.SerializeObject(currentUsageRequest)
        };

        var tcs = new TaskCompletionSource<SmartMeterCalculationResponse>();
        _pendingResponses[serverRequest] = tcs;

        //adds request in a queue and processes the requests concurrently.
        _requestQueue.Enqueue(serverRequest);
        await Task.Run(() => ProcessQueue(_cancellationTokenSource.Token));

        return await tcs.Task;
    }

    public async Task<string> GetPublicKey(string mpan)
    {
        try
        {
            // If mpan is null or whitespace, throw error as empty MPAN is invalid
            if (string.IsNullOrWhiteSpace(mpan))
            {
                ErrorMessage.Invoke("MPAN cannot be null or empty.");
                return string.Empty;
            }

            // Wait for the socket to connect to the server
            await ClientSocket.ConnectClientToServer();

            // Creates a public key request
            var request = new ServerRequest
            {
                Mpan = mpan,
                EncryptedMpan = Array.Empty<byte>(),
                RequestType = RequestType.PublicKey,
                Data = ""
            };

            byte[] data = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(request));

            await ClientSocket.Socket!.SendAsync(data);

            // Receives public key as a response
            byte[] buffer = new byte[2048];
            var bytesReceived = await ClientSocket.Socket.ReceiveAsync(buffer);

            if (bytesReceived == 0)
            {
                ErrorMessage.Invoke("No data received from the server");
                return string.Empty;
            }

            // Receive the response from the server
            var response = Encoding.ASCII.GetString(buffer, 0, bytesReceived);

            // Return the response is null or whitespace
            if (string.IsNullOrWhiteSpace(response))
            {
                ErrorMessage.Invoke("Received an empty or invalid response from the server.");
                return string.Empty;
            }

            // Deserialize the certificate
            var certificateBase = JsonConvert.DeserializeObject<string>(response) ?? "";

            // If the deserialized response is null or whitespace, throw error as deserialization must have gone wrong
            if (string.IsNullOrWhiteSpace(certificateBase))
            {
                ErrorMessage.Invoke("No certificate was received from the server.");
                return string.Empty;
            }

            // Gets certificate data from the received certificate
            var certificateData = Convert.FromBase64String(certificateBase);
            var certificate = new X509Certificate2(certificateData);
            // Checks certificate public key
            using var rsa = certificate.GetRSAPublicKey();
            if (rsa == null)
            {
                ErrorMessage.Invoke("Failed to extract public key from the certificate.");
                return string.Empty;
            }
            // Extracts public key
            PublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());

            if (ClientSocket.Socket.Connected)
                ClientSocket.Socket.Shutdown(SocketShutdown.Both);

            return PublicKey;

        }
        // Catch SocketException if a socket error occurs in the try block
        catch (SocketException ex)
        {
            ErrorMessage.Invoke("Socket error: " + ex.Message);
            return string.Empty;
        }
        // Catch JsonException if an issue occurs with serialization or deserialization of request or response
        catch (JsonException ex)
        {
            ErrorMessage.Invoke("JSON error: " + ex.Message);
            return string.Empty;
        }
        // General exception handler block
        catch (Exception ex)
        {
            ErrorMessage.Invoke($"Unexpected error: {ex.Message}");
            return string.Empty;
        }
    }


    private async Task SendRequest<T>(string mpan, RequestType requestType, T request) where T : IUsageRequest
    {
        try
        {
            // Tries to get the public key 5 times and errors if we can't
            for (var retryCount = 0; retryCount < 5; retryCount++)
            {
                if (string.IsNullOrEmpty(PublicKey))
                {
                    await GetPublicKey(mpan);
                }
                else
                {
                    break;
                }
            }

            // Error here
            if (string.IsNullOrEmpty(PublicKey))
            {
                ErrorMessage.Invoke("Failed to retrieve public key.");
                return;
            }

            var encryptedMpan = _encryptionHelper.EncryptData(Encoding.ASCII.GetBytes(mpan), PublicKey);

            await ClientSocket.ConnectClientToServer();

            // Create a new request.
            var serverRequest = new ServerRequest
            {
                Mpan = mpan,
                EncryptedMpan = encryptedMpan,
                RequestType = requestType,
                Data = JsonConvert.SerializeObject(request)
            };
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
            byte[] buffer = new byte[2048];
            var bytesReceived = await ClientSocket.Socket.ReceiveAsync(buffer);

            var receivedData = Encoding.ASCII.GetString(buffer, 0, bytesReceived);

            var response = JsonConvert.DeserializeObject<SmartMeterCalculationResponse>(receivedData);

            // Close the connection
            ClientSocket.Socket.Shutdown(SocketShutdown.Both);

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
                        await ClientSocket.ConnectClientToServer();

                        // Send our request to the server
                        var data = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(request));
                        await ClientSocket.Socket.SendAsync(data);

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
