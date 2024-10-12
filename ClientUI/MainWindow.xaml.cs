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
        // Potentially initialize? Will look later
        // Create instance of SocketClient class
        private SocketClient _socketClient;
        public MainWindow()
        {
            InitializeComponent();
        }
        // On program window closing, close socket connection.
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _socketClient.Close();
        }

    }
}