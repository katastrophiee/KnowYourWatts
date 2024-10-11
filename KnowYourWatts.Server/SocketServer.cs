using KnowYourWatts.DTO;
using KnowYourWatts.DTO.Requests;
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
                string responseData;
                switch (jsonData.Type)
                {
                    case RequestType.CurrentUsage:
                        var dailyRequest = JsonConvert.DeserializeObject<SmartMeterCalculationRequest>(jsonData.Data);
                        responseData = CalculateDailyUsage(dailyRequest);
                        break;

                    case RequestType.TodaysUsage:
                        //to do
                        responseData = "";
                        break;

                    case RequestType.WeeklyUsage:
                        //to do
                        responseData = "";
                        break;

                    default:
                        // should probs change to be logging
                        // we may wanna make a response object with a data and error field
                        responseData = "";
                        Console.WriteLine("Request type not recognized: " + jsonData.Type);
                        break;
                }

                // respond to client
                byte[] responseBytes = Encoding.ASCII.GetBytes(responseData);
                _handler.Send(responseBytes);
            }
            else
            {

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error handling connection: " + ex.ToString());
        }
    }

    private string CalculateDailyUsage(SmartMeterCalculationRequest? request)
    {
        ServerMathLogic math = new ServerMathLogic();
        double calculatedCost = math.CalculateCost(request.TariffType, request.CurrentRateKwh);
        // Not sure how to add this to the request, Kaytlen to follow up
        return "";
    }
}
