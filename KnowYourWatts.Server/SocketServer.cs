using KnowYourWatts.DTO.Enums;
using KnowYourWatts.DTO.Requests;
using KnowYourWatts.DTO.Response;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace KnowYourWatts.Server;

public class SocketServer
{
    private static Server? Server;

    public static void Main(string[] args)
    {
        var host = Dns.GetHostEntry("localhost");
        var ipAddress = host.AddressList[0];
        var localEndPoint = new IPEndPoint(ipAddress, 11000);

        Server = new Server(host, ipAddress, localEndPoint);
        Server.Start();
    }
}

public class ConnectionHandler(Socket handler)
{
    private readonly Socket _handler = handler;
    private readonly ServerMathLogic _meterCalculations = new();

    public void HandleConnection()
    {
        try
        {
            byte[] buffer = new byte[1024];
            int bytesReceived = _handler.Receive(buffer);

            string data = Encoding.ASCII.GetString(buffer, 0, bytesReceived);

            //remove after testing
            Console.WriteLine("Data received: " + data);

            var jsonData = JsonConvert.DeserializeObject<ServerRequest>(data);

            if (jsonData is not null)
            {
                CalculationResponse calculationResponse = jsonData.Type switch
                {
                    RequestType.CurrentUsage => CalculateCurrentUsage(JsonConvert.DeserializeObject<CurrentUsageRequest>(jsonData.Data)),
                    RequestType.TodaysUsage => CalculateDailyUsage(JsonConvert.DeserializeObject<DailyUsageRequest>(jsonData.Data)),
                    RequestType.WeeklyUsage => CalculateWeeklyUsage(JsonConvert.DeserializeObject<WeeklyUsageRequest>(jsonData.Data)),
                    _ => new CalculationResponse($"The request type {jsonData.Type} was not recognized.")
                };

                _handler.Send(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(calculationResponse)));
            }
            else
            {
                var response = Encoding.ASCII.GetBytes("The request did not contain any data.");
                _handler.Send(response);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error handling connection: " + ex.ToString());
            var response = Encoding.ASCII.GetBytes("An unknown error occured");
            _handler.Send(response);
        }
    }

    private CalculationResponse CalculateCurrentUsage(CurrentUsageRequest? request)
    {
        try
        {
            if (request is not null)
            {
                var calculatedCost = _meterCalculations.CalculateCost(request.TariffType, request.CurrentReading);

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
                var calculatedCost = _meterCalculations.CalculateCost(request.TariffType, request.CurrentReading);

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
                var calculatedCost = _meterCalculations.CalculateCost(request.TariffType, request.CurrentReading);

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
