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
                var requestData = JsonConvert.DeserializeObject<CurrentUsageRequest>(jsonData.Data);

                CalculationResponse response;
                switch (jsonData.Type)
                {
                    case RequestType.CurrentUsage:
                        response = CalculateCurrentUsage(requestData);
                        break;

                    case RequestType.TodaysUsage:
                        response = CalculateDailyUsage(requestData);
                        break;

                    case RequestType.WeeklyUsage:
                        response = CalculateWeeklyUsage(requestData);
                        break;

                    default:
                        // should probs change to be logging
                        Console.WriteLine("Request type not recognized: " + jsonData.Type);
                        response = new($"The request type {jsonData.Type} was not recognized.");
                        break;
                }

                _handler.Send(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(response)));
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
        // we'll calculate the result then convert it to json and send it back to the client

        SmartMeterCalculationResponse data = null; // change to SmartMeterCalculationResponse from after calc

        return new(data);
    }

    private CalculationResponse CalculateDailyUsage(CurrentUsageRequest? request)
    {
        SmartMeterCalculationResponse data = null; // change to SmartMeterCalculationResponse from after calc

        return new(data);
    }
    private CalculationResponse CalculateWeeklyUsage(CurrentUsageRequest? request)
    {
        try
        {
            if (request is not null)
            {
                var math = new ServerMathLogic();
                var calculatedCost = math.CalculateCost(request.TarrifType, request.CurrentReading);

                return new(calculatedCost);
            }

            return new("The request data was empty.");
        }
        catch (Exception ex)
        {
            return new("An unknown error occured when trying to calulate the weekly cost: " + ex.Message);
        }
    }
}
