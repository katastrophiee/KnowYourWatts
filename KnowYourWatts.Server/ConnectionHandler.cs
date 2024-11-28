﻿using KnowYourWatts.Server.Interfaces;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Text;
using KnowYourWatts.Server.DTO.Requests;
using KnowYourWatts.Server.DTO.Enums;
using System.Net.WebSockets;
using KnowYourWatts.Server.DTO.Responses;
using System;

namespace KnowYourWatts.Server;

public sealed class ConnectionHandler(
    ICalculationProvider calculationProvider,
    IKeyHandler keyHandler) : IConnectionHandler
{
    //We use dependency injection to ensure we follow the SOLID principles
    private readonly ICalculationProvider _calculationProvider = calculationProvider;
    private readonly IKeyHandler _keyHandler = keyHandler;

    public void HandleConnection(Socket handler)
    {
        try
        {
            byte[] buffer = new byte[1024];

            //Ensure we are connected to the client and can respond to them
            if (!handler.Connected || handler.RemoteEndPoint is null)
            {
                //We log an error to the console here as we are unable to respond to the client
                Console.WriteLine("No target address for a response was provided from the received request.");
                return;
            }
            int bytesReceived = handler.Receive(buffer);

            //Check we received some data from the client
            if (bytesReceived == 0)
            {
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
                var response = Encoding.ASCII.GetBytes(SerializeErrorResponse("The request did not contain any data when converted to an object."));
                handler.Send(response);
                return;
            }

            if (request.RequestType == RequestType.PublicKey)
            {
                // Send the public key to the client
                handler.Send(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(_keyHandler.PublicKey)));
                return;
            }

            if (string.IsNullOrEmpty(request.Mpan))
            {
                Console.WriteLine("Error: No MPAN was provided with the request.");
                var response = Encoding.ASCII.GetBytes(SerializeErrorResponse("No MPAN was provided with the request."));
                Console.WriteLine(response);
                handler.Send(response);
                return;
            }

            if (request.EncryptedMpan.Length == 0)
            {
                Console.WriteLine("Error: MPAN length is invalid.");
                var reponse = Encoding.ASCII.GetBytes(SerializeErrorResponse("MPAN length is invalid."));
                handler.Send(reponse);
                return;
            }
            //end

            var decryptedMpan = _keyHandler.DecryptClientMpan(request.EncryptedMpan);

            // If decrypted MPAN does not match request MPAN, return error as may have wrong key.
            if (request.Mpan != decryptedMpan)
            {
                Console.WriteLine("Error: The decrypted MPAN does not match the request MPAN");
                var resposnse = Encoding.ASCII.GetBytes(SerializeErrorResponse("The decrypted MPAN does not match the request MPAN"));
                handler.Send(resposnse);
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

            //We put the response object into JSON and send it back to the client
            handler.Send(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(calculationResponse)));
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Socket error: " + ex.Message);
            return;
        }
        catch (JsonReaderException ex)
        {
            Console.WriteLine($"JSON Reader Error: {ex}");
            return;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling connection: {ex}");
            return;
        }
    }

    private static string SerializeErrorResponse(string errorMessage) => JsonConvert.SerializeObject(new CalculationResponse(errorMessage));

    private CalculationResponse CalculateUsage<T>(string mpan, T? request) where T : IUsageRequest
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
                BillingPeriod = request.BillingPeriod,
                StandingCharge = request.StandingCharge
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

    private CalculationResponse CalculateCurrentUsage(string mpan, CurrentUsageRequest? request) => CalculateUsage(mpan, request);

    private CalculationResponse CalculateDailyUsage(string mpan, DailyUsageRequest? request) => CalculateUsage(mpan, request);

    private CalculationResponse CalculateWeeklyUsage(string mpan, WeeklyUsageRequest? request) => CalculateUsage(mpan, request);
}