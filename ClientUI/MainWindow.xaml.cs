using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Newtonsoft.Json;
using KnowYourWatts.DTO.Requests;
using KnowYourWatts.DTO.Enums;

namespace ClientUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window  
    {
        private Socket clientSocket;
        public MainWindow()
        {
            InitializeComponent();
            StartClock();
        }

        private void StartClock()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMinutes(1);
            timer.Tick += Timer_Tick;
            timer.Start();  

            TimeDisplay.Text = DateTime.Now.ToString("HH:mm");
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            TimeDisplay.Text = DateTime.Now.ToString("HH:mm");
        }
        private void ConnectToServer()
        {
            try
            {
                var host = Dns.GetHostEntry("localhost");
                var ipAddress = host.AddressList[0];
                var remoteEndPoint = new IPEndPoint(ipAddress, 11000);

                clientSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(remoteEndPoint);
                
            }
            catch (Exception ex)
            {
               MessageBox.Show(ex.Message);
            }
        }
        private void SendCurrentUsage(object sender, RoutedEventArgs e)
        {
            
        }
        private void SendRequest(RequestType requestType,CurrentUsageRequest currentUsageRequest)
        {
            if(clientSocket == null || !clientSocket.Connected)
            {
                //Display error message
                return;
            }
            try
            {
                var request = new ServerRequest
                {
                    Type = requestType,
                    Data = JsonConvert.SerializeObject(currentUsageRequest)
                };

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}