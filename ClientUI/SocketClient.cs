using KnowYourWatts.DTO.Enums;
using KnowYourWatts.DTO.Requests;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Net;
using System.Windows;
using System.Text;

namespace ClientUI
{
    internal class SocketClient
    {
        // Initiate new Socket
        private Socket clientSocket;
        // Connect to server
        public void ConnectToServer()
        {
            try
            {
                // Get host and create a new socket
                var host = Dns.GetHostEntry("localhost");
                var ipAddress = host.AddressList[0];
                var remoteEndPoint = new IPEndPoint(ipAddress, 11000);

                clientSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                // Connect to endpoint
                clientSocket.Connect(remoteEndPoint);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void SendCurrentUsage(object sender, RoutedEventArgs e)
        {

        }
        public void SendRequest(RequestType requestType, CurrentUsageRequest currentUsageRequest)
        {
            // If the socket is null and the socket is not connected, throw an invalid operation exception (method cannot be performed).
            // Use this exception as you wish, this is a basic implementation.
            if (clientSocket == null || !clientSocket.Connected)
            {
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
}
