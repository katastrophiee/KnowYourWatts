using KnowYourWatts.ClientUI.DTO.Models;
using KnowYourWatts.ClientUI.Interfaces;
using System.Timers;

namespace KnowYourWatts.ClientUI;

public partial class MainPage : ContentPage
{
    readonly IRandomisedValueProvider _randomisedValueProvider;
    readonly IServerRequestHandler _serverRequestHandler;
    private System.Timers.Timer timer;

    private MeterReadings? MeterReading { get; set; }

    public MainPage(IRandomisedValueProvider randomisedValueProvider, IServerRequestHandler serverRequestHandler)
    {
        _randomisedValueProvider = randomisedValueProvider;
        _serverRequestHandler = serverRequestHandler;
        InitializeComponent();
        StartClock();
    }

    private void StartClock()//fix later
    {
        timer = new System.Timers.Timer(60000); // 60000 milliseconds = 1 minute
        timer.Elapsed += Timer_Elapsed;
        timer.Start();

        UpdateTimeDisplay();
    }

    //private void RandomReadingTimer()
    //{
    //    timer = new System.Timers.Timer(_randomisedValueProvider.GenerateRandomTimeDelay());
    //    timer.Elapsed += Timer_Elapsed;
    //    timer.Start();

    //    _serverRequestHandler.SendRequest();
    //}

    private void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(UpdateTimeDisplay);
    }

    private void UpdateTimeDisplay()
    {
        TimeDisplay.Text = DateTime.Now.ToString("HH:mm");
    }

    private void OnTabClicked(object sender, EventArgs e)
    {
        if (sender is Button clickedButton)
        {
            // Reset all tab backgrounds
            CurrentUsageTab.BackgroundColor = Color.FromArgb("#323232");
            TodayUsageTab.BackgroundColor = Color.FromArgb("#323232");
            WeekUsageTab.BackgroundColor = Color.FromArgb("#323232");

            // Set the clicked tab's background
            clickedButton.BackgroundColor = Color.FromArgb("#345365");

            
            // Update content based on the selected tab
            switch (clickedButton.Text)
            {
                case "Current Usages":
                    UsageCost.Text = $"£{MeterReading.Cost}";
                    UsageKW.Text = $"{MeterReading.Usage}KW";
                    break;
                case "Today's Usage":
                    UsageCost.Text = "£1.81";
                    UsageKW.Text = "18.7KW";
                    break;
                case "Week Usage":
                    UsageCost.Text = "£1.82";
                    UsageKW.Text = "18.7KW";
                    break;
            }
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        timer?.Stop();
        timer?.Dispose();
    }
}