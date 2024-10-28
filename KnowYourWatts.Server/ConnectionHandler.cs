﻿using KnowYourWatts.Server.Interfaces;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Text;
using KnowYourWatts.Server.DTO.Requests;
using KnowYourWatts.Server.DTO.Enums;
using KnowYourWatts.Server.DTO.Response;

namespace KnowYourWatts.Server;

public sealed class ConnectionHandler(ICalculationProvider calculationProvider) : IConnectionHandler
{
    //We use dependency injection to ensure we follow the SOLID principles
    private readonly ICalculationProvider _calculationProvider = calculationProvider;

    public void HandleConnection(Socket handler)
    {
        try
        {
            //Ensure we are connected to the client and can respond to them
            if (!handler.Connected || handler.RemoteEndPoint is null)
            {
                //We log an error to the console here as we are unable to respond to the client
                Console.WriteLine("No target address for a response was provided from the recieved request.");
                return;
            }

            byte[] buffer = new byte[1024];
            int bytesReceived = handler.Receive(buffer);

            if (bytesReceived == 0)
            {
                var response = Encoding.ASCII.GetBytes(SerializeErrorResponse("The request was received but it contained no data."));
                handler.Send(response);
                return;
            }

            string receivedData = Encoding.ASCII.GetString(buffer, 0, bytesReceived);

            Console.WriteLine("Data received " + receivedData);

            var request = JsonConvert.DeserializeObject<ServerRequest>(receivedData);

            if (request is null)
            {
                var response = Encoding.ASCII.GetBytes(SerializeErrorResponse("The request did not contain any data when converted to an object."));
                handler.Send(response);
                return;
            }

            // Add back in once MPAN is added properly
            //if (string.IsNullOrEmpty(request.Mpan))
            //{
            //    var response = Encoding.ASCII.GetBytes(SerializeErrorResponse("No MPAN was provided with the request."));
            //    handler.Send(response);
            //    return;
            //}

            var calculationResponse = request.RequestType switch
            {
                RequestType.CurrentUsage => CalculateCurrentUsage(JsonConvert.DeserializeObject<CurrentUsageRequest>(request.Data)),
                RequestType.TodaysUsage => CalculateDailyUsage(JsonConvert.DeserializeObject<DailyUsageRequest>(request.Data)),
                RequestType.WeeklyUsage => CalculateWeeklyUsage(JsonConvert.DeserializeObject<WeeklyUsageRequest>(request.Data)),
                _ => new($"The request type {request.RequestType} was not recognized.")
            };

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
            var response = Encoding.ASCII.GetBytes("An error occured when trying to deserialise the JSON");
            handler.Send(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling connection: {ex}");
            var response = Encoding.ASCII.GetBytes("An unknown error occured when handling the connection.");
            handler.Send(response);
        }
    }

    private static string SerializeErrorResponse(string errorMessage)
    {
        var response = new CalculationResponse(errorMessage);

        return JsonConvert.SerializeObject(response);
    }

    private CalculationResponse CalculateCurrentUsage(CurrentUsageRequest? request)
    {
        try
        {
            if (request is not null)
            {
                var calculateCostRequest = new SmartMeterCalculationRequest
                {
                    TariffType = request.TariffType,
                    CurrentReading = request.CurrentReading,
                    PreviousReading = 1,
                    BillingPeriod = 1,
                    ExistingCharge = 1
                };

                //need to get the previous reading and existing charge from MockDb
                //billing period is 1 as it is the current usage
                var calculatedCost = _calculationProvider.CalculateCost(calculateCostRequest);

                return new(calculatedCost);
            }

            return new("The request data was empty.");
        }
        catch (Exception ex)
        {
            return new("An unknown error occured when trying to calculate the current cost: " + ex.Message);
        }
    }

    private CalculationResponse CalculateDailyUsage(DailyUsageRequest? request)
    {
        try
        {
            if (request is not null)
            {
                var calculateCostRequest = new SmartMeterCalculationRequest
                {
                    TariffType = request.TariffType,
                    CurrentReading = request.CurrentReading,
                    PreviousReading = 1,
                    BillingPeriod = 1,
                    ExistingCharge = 1
                };

                var calculatedCost = _calculationProvider.CalculateCost(calculateCostRequest);

                return new(calculatedCost);
            }

            return new("The request data was empty.");
        }
        catch (Exception ex)
        {
            return new("An unknown error occured when trying to calculate the daily cost: " + ex.Message);
        }
    }

    private CalculationResponse CalculateWeeklyUsage(WeeklyUsageRequest? request)
    {
        try
        {
            if (request is not null)
            {
                var calculateCostRequest = new SmartMeterCalculationRequest
                {
                    TariffType = request.TariffType,
                    CurrentReading = request.CurrentReading,
                    PreviousReading = 1,
                    BillingPeriod = 7,
                    ExistingCharge = 1
                };

                var calculatedCost = _calculationProvider.CalculateCost(calculateCostRequest);

                return new(calculatedCost);
            }

            return new("The request data was empty.");
        }
        catch (Exception ex)
        {
            return new("An unknown error occured when trying to calculate the weekly cost: " + ex.Message);
        }
    }
}