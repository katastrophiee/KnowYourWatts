using System.Text;
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

namespace ClientUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window  
    {
        private DispatcherTimer timer;

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
    }
}