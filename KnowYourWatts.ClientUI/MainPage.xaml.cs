using KnowYourWatts.ClientUI.DTO.Enums;
using KnowYourWatts.ClientUI.DTO.Models;
using KnowYourWatts.ClientUI.Interfaces;
using System.Timers;

namespace KnowYourWatts.ClientUI;

public partial class MainPage : ContentPage
{
    private MeterReadings CurrentMeterReading = null!;
    private MeterReadings DailyMeterReading = null!;
    private MeterReadings WeeklyMeterReading = null!;

    readonly IRandomisedValueProvider _randomisedValueProvider;
    readonly IServerRequestHandler _serverRequestHandler;

    private TariffType TariffType;
    private decimal StandingCharge;
    private string Mpan;

    public MainPage(IRandomisedValueProvider randomisedValueProvider, IServerRequestHandler serverRequestHandler)
    {
        _randomisedValueProvider = randomisedValueProvider;
        _serverRequestHandler = serverRequestHandler;

        CurrentMeterReading = new()
        {
            Cost = 0,
            Usage = _randomisedValueProvider.GenerateRandomReading()
        };

        DailyMeterReading = new()
        {
            Cost = 0,
            Usage = _randomisedValueProvider.GenerateRandomReading()
        };

        WeeklyMeterReading = new()
        {
            Cost = 0,
            Usage = _randomisedValueProvider.GenerateRandomReading()
        };

        //Generate initial Tariff Type
        TariffType = (TariffType)Enum.ToObject(typeof(TariffType),_randomisedValueProvider.GenerateRandomTarrif());

        //Generate initial Standing charge
        StandingCharge = _randomisedValueProvider.GenerateRandomStandingCharge();

        //Generate the unique identifier for the client
        Mpan = _randomisedValueProvider.GenerateMpanForClient();
        _serverRequestHandler.GetPublicKey(Mpan);

        InitializeComponent();
        
        StartClock();
        SendRandomCurrentReadingTimer();
    }

    private void SendRandomCurrentReadingTimer()
    {
        var timer = new System.Timers.Timer()
        {
            AutoReset = false
        };

        timer.Elapsed += async (sender, e) =>
        {
            // Stop the timer while the request to the server is being sent
            timer.Stop();

            await SendReadingToServer();

            timer.Interval = 1000;
            //timer.Interval = _randomisedValueProvider.GenerateRandomTimeDelay();
            timer.Start();
        };

        timer.Interval = 1000;
        //timer.Interval = _randomisedValueProvider.GenerateRandomTimeDelay();
        timer.Start();
    }

    // change to be modular and have seperate functions and timers for each req type
    private async Task SendReadingToServer()
    {
        var response = await _serverRequestHandler.SendRequestToServer(
            CurrentMeterReading.Usage,
            RequestType.CurrentUsage,
            TariffType,
            1 /*BillingPeriod*/, //billing period should be retireved from the page we're sending the request from
            StandingCharge,
            Mpan
        );

        if (!string.IsNullOrEmpty(response.ErrorMessage))
        {
            //show error on error page
        }
        else if (response.Cost is not null)
        {
            CurrentMeterReading.Cost += response.Cost.Value;
            CurrentMeterReading.Usage += _randomisedValueProvider.GenerateRandomReading();
        }
    }

    /* Clock Code */
    private void StartClock() //fix later
    {
        var timer = new System.Timers.Timer(60000);
        timer.Elapsed += Timer_Elapsed;
        timer.Start();
        UpdateTimeDisplay();
    }

    private void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(UpdateTimeDisplay);
    }

    private void UpdateTimeDisplay()
    {
        TimeDisplay.Text = DateTime.Now.ToString("HH:mm");
    }

    /* Clock Code End */

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
                    UsageCost.Text = $"£{CurrentMeterReading.Cost}";
                    UsageKW.Text = $"{CurrentMeterReading.Usage}KW";
                    break;
                case "Today's Usage":
                    UsageCost.Text = $"£{DailyMeterReading.Cost}";
                    UsageKW.Text = $"{DailyMeterReading.Usage}KW";
                    break;
                case "Week Usage":
                    UsageCost.Text = $"£{WeeklyMeterReading.Cost}";
                    UsageKW.Text = $"{WeeklyMeterReading.Usage}KW";
                    break;
            }
        }
    }
}