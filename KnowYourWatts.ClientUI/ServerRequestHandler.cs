﻿using KnowYourWatts.ClientUI.DTO.Enums;
using KnowYourWatts.ClientUI.DTO.Interfaces;
using KnowYourWatts.ClientUI.DTO.Requests;
using KnowYourWatts.ClientUI.DTO.Response;
using KnowYourWatts.ClientUI.Interfaces;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace KnowYourWatts.ClientUI;

public class ServerRequestHandler(ClientSocket clientSocket, IEncryptionHelper encryptionHelper) : IServerRequestHandler
{
    private readonly ClientSocket ClientSocket = clientSocket;
    private readonly IEncryptionHelper _encryptionHelper = encryptionHelper;
    private string PublicKey = "";
    public event Action<string> ErrorMessage = null!;

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
            if (string.IsNullOrEmpty(PublicKey))
                await GetPublicKey();

            // Create a new request object containing the data to use for the cost calculation
            var request = new UsageRequest(tariffType, initialReading, currentCost, billingPeriod, standingCharge);

            var encryptedMpan = _encryptionHelper.EncryptData(Encoding.ASCII.GetBytes(mpan), PublicKey);

            // Create a new request for the server containing the calulation data
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

            if (string.IsNullOrEmpty(PublicKey))
            {
                ErrorMessage.Invoke("Failed to retrieve public key.");
                return;
            }

            var encryptedMpan = _encryptionHelper.EncryptData(Encoding.ASCII.GetBytes(mpan), PublicKey);

            await ClientSocket.ConnectClientToServer();

            // Authenticate the stream as a client.
            ClientSocket.SslStream.AuthenticateAsClient("KnowYourWattsServer");

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

            await ClientSocket.SslStream.WriteAsync(data);
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

            var bytesReceived = await ClientSocket.SslStream.ReadAsync(buffer);

            var receivedData = Encoding.ASCII.GetString(buffer, 0, bytesReceived);

            var response = JsonConvert.DeserializeObject<SmartMeterCalculationResponse>(receivedData);

            // Close the stream.
            ClientSocket.SslStream.Close();

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

            ClientSocket.SslStream.AuthenticateAsClient("KnowYourWattsServer");

            if (!string.IsNullOrEmpty(ClientSocket.ErrorMessage))
            {
                ErrorMessage.Invoke(ClientSocket.ErrorMessage);
                return;
            }

            // Creates a request to get the public key from the server
            var request = new ServerRequest
            {
                Mpan = "",
                EncryptedMpan = [],
                RequestType = RequestType.PublicKey,
                Data = ""
            };

            byte[] data = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(request));
            byte[] buffer = new byte[2048];

            await ClientSocket.SslStream.WriteAsync(data);

            // Receives certificate as a response
            var bytesReceived = await ClientSocket.SslStream.ReadAsync(buffer);

            if (bytesReceived == 0)
            {
                ErrorMessage.Invoke("No data received from the server");
                return;
            }

            // Get string from the bytes received
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
            if (rsa is null)
            {
                ErrorMessage.Invoke("Failed to extract public key from the certificate.");
                return;
            }

            // Extracts public key
            PublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());

            if (ClientSocket.SslStream.CanRead && ClientSocket.SslStream.CanWrite)
                ClientSocket.SslStream.Close();

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
            return;
        }
    }
}