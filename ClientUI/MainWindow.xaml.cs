using KnowYourWatts.DTO.Enums;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ClientUI;

public partial class MainWindow : Window  
{
    private DispatcherTimer timer;
    private Socket clientSocket;
    private RequestType requestType;

    public MainWindow()
    {
        InitializeComponent();
        StartClock(); 
        ConnectToServer();
    }
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

    private void TabControlSelected (object sender, SelectionChangedEventArgs e)
    {
        var selectedTab = TabControl.SelectedItem as TabItem;
        if (selectedTab != null) 
        {
            if (selectedTab.Name == CurrentUsageTab.Name)
            {
                requestType = RequestType.CurrentUsage;
            }    
            else if (selectedTab.Name == TodaysUsageTab.Name)
            {
                requestType = RequestType.TodaysUsage;
            }
            else if (selectedTab.Name == WeeklyUsageTab.Name)
            {
                requestType = RequestType.WeeklyUsage;
            }
        }
    }
}