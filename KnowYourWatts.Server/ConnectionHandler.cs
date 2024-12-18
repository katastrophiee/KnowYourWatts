﻿using KnowYourWatts.Server.Interfaces;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Text;
using KnowYourWatts.Server.DTO.Requests;
using KnowYourWatts.Server.DTO.Enums;
using KnowYourWatts.Server.DTO.Responses;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Security.Authentication;
using KnowYourWatts.Server.DTO.Interfaces;

namespace KnowYourWatts.Server;

public sealed class ConnectionHandler(
    ICalculationProvider calculationProvider,
    ICertificateHandler certificateHandler) : IConnectionHandler
{
    //We use dependency injection to ensure we follow the SOLID principles
    private readonly ICalculationProvider _calculationProvider = calculationProvider;
    private readonly ICertificateHandler _certificateHandler = certificateHandler;

    public void HandleConnection(Socket handler)
    {
        try
        {
            using var networkStream = new NetworkStream(handler);
            using var sslStream = new SslStream(networkStream, false, (sender, certificate, chain, sslPolicyErrors) => true);

            sslStream.AuthenticateAsServer(
                _certificateHandler.Certificate,
                false,
                SslProtocols.Tls12,
                false
            );
                
            if (MonitorGrid()) 
            {
                var response = Encoding.ASCII.GetBytes(SerializeErrorResponse("There is a problem with the Electricity grid"));
                sslStream.Write(response);
                return;
            }

            // Prepared generated certificate to be sent by converting to base64 format
            var exportedCert = _certificateHandler.Certificate.Export(X509ContentType.Cert);
            var base64Cert = Convert.ToBase64String(exportedCert);

            byte[] buffer = new byte[2048];

            //Ensure we are connected to the client and can respond to them
            if (!handler.Connected || handler.RemoteEndPoint is null)
            {
                //We log an error to the console here as we are unable to respond to the client
                Console.WriteLine("No target address for a response was provided from the received request.");
                return;
            }

            int bytesReceived = sslStream.Read(buffer);

            //Check we received some data from the client
            if (bytesReceived == 0)
            {
                Console.WriteLine("The request was received but it contained no data.");
                var response = Encoding.ASCII.GetBytes(SerializeErrorResponse("The request was received but it contained no data."));
                handler.Send(response);
                return;
            }

            //Convert the data to a string so we can use it as JSON
            var receivedData = Encoding.ASCII.GetString(buffer, 0, bytesReceived);

            //Convert the JSON to an object so we can use the request
            var request = JsonConvert.DeserializeObject<ServerRequest>(receivedData);
            if (request is null)
            {
                Console.WriteLine("The request did not contain any data when converted to an object.");
                var response = Encoding.ASCII.GetBytes(SerializeErrorResponse("The request did not contain any data when converted to an object."));
                sslStream.Write(response);
                return;
            }

            if (request.RequestType == RequestType.PublicKey)
            {
                // Send the certificate to the client
                sslStream.Write(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(base64Cert)));
                return;
            }

            if (string.IsNullOrEmpty(request.Mpan))
            {
                Console.WriteLine("No MPAN was provided with the request.");
                var response = Encoding.ASCII.GetBytes(SerializeErrorResponse("No MPAN was provided with the request."));
                Console.WriteLine(response);
                sslStream.Write(response);
                return;
            }

            if (request.EncryptedMpan.Length == 0)
            {
                Console.WriteLine("Error: MPAN length is invalid.");
                var reponse = Encoding.ASCII.GetBytes(SerializeErrorResponse("MPAN length is invalid."));
                sslStream.Write(reponse);
                return;
            }

            var decryptedMpan = _certificateHandler.DecryptClientMpan(request.EncryptedMpan);

            // If decrypted MPAN does not match request MPAN, return error as may have wrong key.
            if (request.Mpan != decryptedMpan)
            {
                Console.WriteLine("Error: The decrypted MPAN does not match the request MPAN");
                var response = Encoding.ASCII.GetBytes(SerializeErrorResponse("The decrypted MPAN does not match the request MPAN"));
                sslStream.Write(response);
                return;
            }

            //Based on our request type, we want to convert the string of JSON in the data property to the correct object
            var calculationResponse = request.RequestType switch
            {
                RequestType.CurrentUsage => CalculateCurrentUsage(request.Mpan, JsonConvert.DeserializeObject<CurrentUsageRequest>(request.Data)),
                RequestType.TodaysUsage => CalculateDailyUsage(request.Mpan, JsonConvert.DeserializeObject<DailyUsageRequest>(request.Data)),
                RequestType.WeeklyUsage => CalculateWeeklyUsage(request.Mpan, JsonConvert.DeserializeObject<WeeklyUsageRequest>(request.Data)),
                _ => new($"The request type {request.RequestType} was not recognized.")
            };

            if (calculationResponse is not null && !string.IsNullOrEmpty(calculationResponse.ErrorMessage))
                Console.WriteLine(calculationResponse.ErrorMessage);

            //We put the response object into JSON and send it back to the client
            sslStream.Write(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(calculationResponse)));
        }
        catch (SocketException ex)
        {
            //We don't respond to the client here as we are unable to communicate with them
            Console.WriteLine("Socket error: " + ex.Message);
        }
        catch (JsonReaderException ex)
        {
            Console.WriteLine($"JSON Reader Error: {ex}");
            var response = Encoding.ASCII.GetBytes("An error occured when trying to deserialise the JSON");
            handler.Send(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling connection: {ex}");
            var response = Encoding.ASCII.GetBytes("An unknown error occured when handling the request.");
            handler.Send(response);
        }
    }

    private static string SerializeErrorResponse(string errorMessage) => JsonConvert.SerializeObject(new CalculationResponse(errorMessage));

    private CalculationResponse CalculateUsage<T>(string mpan, RequestType requestType, T? request) where T : IUsageRequest
    {
        try
        {
            // Check the request data is not null in case the deserialization failed
            if (request is null)
                return new("The request data was empty.");

            var calculateCostRequest = new SmartMeterCalculationRequest
            {
                Mpan = mpan,
                TariffType = request.TariffType,
                CurrentReading = request.CurrentReading,
                CurrentCost = request.CurrentCost,
                BillingPeriod = request.BillingPeriod,
                StandingCharge = request.StandingCharge,
                RequestType = requestType
            };

            //Calculate the cost of the electricity used
            var calculatedCost = _calculationProvider.CalculateCost(calculateCostRequest);

            return calculatedCost;
        }
        catch (Exception ex)
        {
            return new($"An unknown error occurred when trying to calculate the cost: {ex.Message}");
        }
    }

    private static bool MonitorGrid()
    {
        Random rnd = new();
        while (true)
        {
            bool gridIssue = rnd.Next(0, 100) < 3; // 3% chance of a grid issue

            if (gridIssue)
                Console.WriteLine("Grid issue detected!");

            return gridIssue;
        }
    }

    private CalculationResponse CalculateCurrentUsage(string mpan, CurrentUsageRequest request) => CalculateUsage(mpan, RequestType.CurrentUsage, request);

    private CalculationResponse CalculateDailyUsage(string mpan, DailyUsageRequest request) => CalculateUsage(mpan, RequestType.TodaysUsage, request);

    private CalculationResponse CalculateWeeklyUsage(string mpan, WeeklyUsageRequest request) => CalculateUsage(mpan, RequestType.WeeklyUsage, request);
}