using KnowYourWatts.DTO.Enums;
using KnowYourWatts.DTO.Requests;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Net;
using System.Windows;
using System.Text;

namespace ClientUI;

internal class SocketClient
{
    private Socket clientSocket;
   
    public async void SendCurrentReadingsAtRandom(double initialReading, RequestType requestType, TariffType tariffType)
    {
        var random = new Random();
        
        try
        {
            int delay = random.Next(15000, 60000);
            await Task.Delay(delay);
            var currentUsageRequest = new CurrentUsageRequest
            {
                TariffType = tariffType,
                CurrentReading = initialReading
            };
            initialReading += random.NextDouble();
            //example. Will have if statements to send different request types based on the tab selected on the main window.
            SendRequest(requestType, currentUsageRequest);
        }
        catch 
        { 
        
        }
    }
    public void SendRequest(RequestType requestType, CurrentUsageRequest currentUsageRequest)
    {
        // If the socket is null and the socket is not connected, throw an invalid operation exception (method cannot be performed).
        // Use this exception as you wish, this is a basic implementation.
        if (clientSocket == null || !clientSocket.Connected)
        {
            //try and reconnect instead of showing error.
            throw new InvalidOperationException("An error occured when trying to send a request");
        }
        try
        {
            // Create a new request.
            var request = new ServerRequest
            {
                Type = requestType,
                Data = JsonConvert.SerializeObject(currentUsageRequest)
            };
            string dataToSend = JsonConvert.SerializeObject(request);
            byte[] data = Encoding.UTF8.GetBytes(dataToSend);
            clientSocket.SendAsync(data);
        }
        // Catch exception if request cannot be made (currently). Later will also catch request send?
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
    // Close socket method (call this in instances such as window closing, program ending, etc).
    public void Close()
    {
        clientSocket.Close();
    }
}
