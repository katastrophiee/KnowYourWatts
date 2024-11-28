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
    ClientSocket clientSocket,
    IEncryptionHelper encryptionHelper): IServerRequestHandler
{   
    private readonly IRandomisedValueProvider _randomisedValueProvider = randomisedValueProvider;
    private readonly ClientSocket ClientSocket = clientSocket;
    private readonly IEncryptionHelper _encryptionHelper = encryptionHelper;
    private string PublicKey;

    public async Task<SmartMeterCalculationResponse?> SendRequestToServer(
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

        await SendRequest(mpan, requestType, currentUsageRequest);

        return await HandleServerResponse(currentUsageRequest.CurrentReading);
    }

    public async Task<string> GetPublicKey(string mpan)
    {
        // If mpan is null or whitespace, throw error as empty MPAN is invalid
        if (string.IsNullOrWhiteSpace(mpan))
        {
            Console.WriteLine("MPAN cannot be null or empty.");
            return string.Empty;
        }


        try
        {
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
            byte[] buffer = new byte[1024];
            var bytesReceived = await ClientSocket.Socket.ReceiveAsync(buffer);

            if (bytesReceived == 0)
                throw new Exception("No data received from server.");

            // Deserialize the response
            var response = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
            // If the response is null or whitespace, throw exception
            if (string.IsNullOrWhiteSpace(response))
                throw new Exception("Received an empty or invalid response from the server.");
            // Deserialize the public key
            var deserializedResponse = JsonConvert.DeserializeObject<string>(response) ?? "";
            // If the deserialized response is null or whitespace, throw error as deserialization must have gone wrong
            if (string.IsNullOrWhiteSpace(deserializedResponse))
                throw new Exception("Response deserialization failed.");

            // Set the public key
            PublicKey = deserializedResponse;

            if (ClientSocket.Socket.Connected)
            {
                ClientSocket.Socket.Shutdown(SocketShutdown.Both);
            }

            return PublicKey;

        }
        // Catch SocketException if a socket error occurs in the try block
        catch (SocketException ex)
        {
            throw new Exception("An error occurred while communicating with the server.", ex);
        }
        // Catch JsonException if an issue occurs with serialization or deserialization of request or response
        catch (JsonException ex)
        {;
            throw new Exception("An error occurred while processing the server response.", ex);
        }
        // General exception handler block
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
            throw; // Re-throw the exception to notify the caller
        }
    }


    private async Task SendRequest<T>(string mpan, RequestType requestType, T request) where T : IUsageRequest
    {
        try
        {

            int timeout = 13000;
            var task = GetPublicKey(mpan);

            if (string.IsNullOrEmpty(PublicKey))
            {
                // Writes an exception to console if the delay of 13 seconds takes less time to complete than the public key being awaited.
                // Effectively sets a timeout of 13 seconds for retrieving the key.
                if (await Task.WhenAny(task, Task.Delay(timeout)) != task)
                {
                    Console.WriteLine("Public key retrieval timed out.");
                    return;
                }
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

            byte[] data = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(serverRequest));

            await ClientSocket.Socket!.SendAsync(data);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            // Maybe add error page later
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
