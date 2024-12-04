using KnowYourWatts.ClientUI.DTO.Enums;
using KnowYourWatts.ClientUI.DTO.Requests;
using KnowYourWatts.ClientUI.DTO.Response;
using KnowYourWatts.ClientUI.Interfaces;
using Newtonsoft.Json;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace KnowYourWatts.ClientUI;

public class ServerRequestHandler(ClientSocket clientSocket, IEncryptionHelper encryptionHelper) : IServerRequestHandler
{   
    private readonly ClientSocket ClientSocket = clientSocket;
    private readonly IEncryptionHelper _encryptionHelper = encryptionHelper;

    private string PublicKey = "";

    public event Action<string> ErrorMessage;

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
        try
        {
            //Add retry back in here
            if (string.IsNullOrEmpty(PublicKey))
                await GetPublicKey();

            //Change to multiple req types
            var request = new CurrentUsageRequest(tariffType, initialReading, currentCost, billingPeriod, standingCharge);

            var encryptedMpan = _encryptionHelper.EncryptData(Encoding.ASCII.GetBytes(mpan), PublicKey);

            // Create a new request.
            var serverRequest = new ServerRequest
            {
                Mpan = mpan,
                EncryptedMpan = encryptedMpan,
                RequestType = requestType,
                Data = JsonConvert.SerializeObject(request)
            };

            await SendRequest(mpan, requestType, request);

            return await HandleServerResponse(request.CurrentReading);
        }
        catch (Exception ex)
        {
            ErrorMessage.Invoke(ex.Message);
            return null;
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
                    await GetPublicKey();
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

            if (!string.IsNullOrEmpty(ClientSocket.ErrorMessage))
            {
                ErrorMessage.Invoke(ClientSocket.ErrorMessage);
                return;
            }

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
            ErrorMessage.Invoke(ex.Message);
        }
    }

    private async Task<SmartMeterCalculationResponse?> HandleServerResponse(decimal currentCost)
    {
        try
        {
            byte[] buffer = new byte[2048];
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
    }

    public async Task GetPublicKey()
    {
        try
        {
            // Wait for the socket to connect to the server
            await ClientSocket.ConnectClientToServer();

            using var networkStream = new NetworkStream(ClientSocket.Socket, ownsSocket: false);
            using var sslStream = new SslStream(networkStream, leaveInnerStreamOpen: false, (sender, certificate, chain, sslPolicyErrors) => true);

            sslStream.AuthenticateAsClient("KnowYourWattsServer");

            if (!string.IsNullOrEmpty(ClientSocket.ErrorMessage))
            {
                ErrorMessage.Invoke(ClientSocket.ErrorMessage);
                return;
            }

            // Creates a public key request
            var request = new ServerRequest
            {
                Mpan = "",
                EncryptedMpan = [],
                RequestType = RequestType.PublicKey,
                Data = ""
            };

            byte[] data = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(request));
            byte[] buffer = new byte[2048];

            await sslStream.WriteAsync(data);

            // Receives public key as a response
            var bytesReceived = await sslStream.ReadAsync(buffer);

            if (bytesReceived == 0)
            {
                ErrorMessage.Invoke("No data received from the server");
                return;
            }

            // Receive the response from the server
            var response = Encoding.ASCII.GetString(buffer, 0, bytesReceived);

            // Return the response is null or whitespace
            if (string.IsNullOrWhiteSpace(response))
            {
                ErrorMessage.Invoke("Received an empty or invalid response from the server.");
                return;
            }

            // Deserialize the certificate
            var certificateBase = JsonConvert.DeserializeObject<string>(response) ?? "";

            // If the deserialized response is null or whitespace, throw error as deserialization must have gone wrong
            if (string.IsNullOrWhiteSpace(certificateBase))
            {
                ErrorMessage.Invoke("No certificate was received from the server.");
                return;
            }

            // Gets certificate data from the received certificate
            var certificateData = Convert.FromBase64String(certificateBase);
            var certificate = new X509Certificate2(certificateData);

            // Checks certificate public key
            using var rsa = certificate.GetRSAPublicKey();
            if (rsa == null)
            {
                ErrorMessage.Invoke("Failed to extract public key from the certificate.");
                return;
            }

            // Extracts public key
            PublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());

            if (ClientSocket.Socket.Connected)
                ClientSocket.Socket.Shutdown(SocketShutdown.Both);

            return;
        }
        // Catch SocketException if a socket error occurs in the try block
        catch (SocketException ex)
        {
            ErrorMessage.Invoke("Socket error: " + ex.Message);
            return;
        }
        // Catch JsonException if an issue occurs with serialization or deserialization of request or response
        catch (JsonException ex)
        {
            ErrorMessage.Invoke("JSON error: " + ex.Message);
            return;
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
            for (var retryCount = 0; retryCount < 5;  retryCount++)
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

            if (string.IsNullOrEmpty(PublicKey))
            {
                ErrorMessage.Invoke("Failed to retrieve public key.");
                return;
            }

            using var networkStream = new NetworkStream(ClientSocket.Socket, ownsSocket: false);
            using var sslStream = new SslStream(networkStream, leaveInnerStreamOpen: false, (sender, certificate, chain, sslPolicyErrors) => true);

            sslStream.AuthenticateAsClient("KnowYourWattsServer");

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

            await sslStream.WriteAsync(data);
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

            using var networkStream = new NetworkStream(ClientSocket.Socket, ownsSocket: false);
            using var sslStream = new SslStream(networkStream, leaveInnerStreamOpen: false, (sender, certificate, chain, sslPolicyErrors) => true);

            sslStream.AuthenticateAsClient("KnowYourWattsServer");

            byte[] buffer = new byte[2048];
            var bytesReceived = await sslStream.ReadAsync(buffer);

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
    }
}
