using KnowYourWatts.DTO.Response;
using KnowYourWatts.Server.Interfaces;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Text;
using KnowYourWatts.DTO.Requests;
using KnowYourWatts.DTO.Enums;

namespace KnowYourWatts.Server;

public sealed class ConnectionHandler(ICalculationProvider calculationProvider) : IConnectionHandler
{
    private readonly ICalculationProvider _calculationProvider = calculationProvider;

    //not a bad way, violets the solid principles - doesn't invert the control
    // dependency injection to do it properly, instead of how it is created so far

    public void HandleConnection(Socket handler)
    {
        try
        {
            if (!handler.Connected)
            {
                Console.WriteLine("No target address for a response was provided from the recieved request.");
                return;
            }

            byte[] buffer = new byte[1024];
            int bytesReceived = handler.Receive(buffer);

            if (bytesReceived > 0)
            {
                string data = Encoding.ASCII.GetString(buffer, 0, bytesReceived);

                Console.WriteLine("Data received: " + data);

                var jsonData = JsonConvert.DeserializeObject<ServerRequest>(data);

                if (jsonData is not null)
                {
                    CalculationResponse calculationResponse = jsonData.RequestType switch
                    {
                        RequestType.CurrentUsage => CalculateCurrentUsage(JsonConvert.DeserializeObject<CurrentUsageRequest>(jsonData.Data)),
                        RequestType.TodaysUsage => CalculateDailyUsage(JsonConvert.DeserializeObject<DailyUsageRequest>(jsonData.Data)),
                        RequestType.WeeklyUsage => CalculateWeeklyUsage(JsonConvert.DeserializeObject<WeeklyUsageRequest>(jsonData.Data)),
                        _ => new CalculationResponse($"The request type {jsonData.RequestType} was not recognized.")
                    };

                    handler.Send(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(calculationResponse)));
                }
                else
                {
                    var response = Encoding.ASCII.GetBytes("The request did not contain any data.");
                    handler.Send(response);
                }
            }
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Socket error: " + ex.Message);
            var response = Encoding.ASCII.GetBytes("A socket error occurred");
            handler.Send(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error handling connection: " + ex.ToString());
            var response = Encoding.ASCII.GetBytes("An unknown error occured");
            handler.Send(response);
        }
    }

    private CalculationResponse CalculateCurrentUsage(CurrentUsageRequest? request)
    {
        try
        {
            if (request is not null)
            {
                var calculatedCost = _calculationProvider.CalculateCost(request.TariffType, request.CurrentReading);

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
                var calculatedCost = _calculationProvider.CalculateCost(request.TariffType, request.CurrentReading);

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
                var calculatedCost = _calculationProvider.CalculateCost(request.TariffType, request.CurrentReading);

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