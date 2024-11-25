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

    private DateTime LastUpdatedDate = DateTime.Now.Date;

    private Button _activeTab = null!; // Reference to the active tab

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

        TariffType = (TariffType)Enum.ToObject(typeof(TariffType), _randomisedValueProvider.GenerateRandomTarrif());
        StandingCharge = _randomisedValueProvider.GenerateRandomStandingCharge();
        Mpan = _randomisedValueProvider.GenerateMpanForClient();

        InitializeComponent();

        SelectTab(CurrentUsageTab, "Current Usages");
        StartClock(); // Start clock independently
        StartRandomCurrentReadingTimer();
    }

    private void StartClock()
    {
        var clockTimer = new System.Timers.Timer(1000) { AutoReset = true };
        clockTimer.Elapsed += (sender, e) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UpdateTimeDisplay();
                CheckAndUpdateDailyUsage();
            });
        };
        clockTimer.Start();
    }

    private void StartRandomCurrentReadingTimer()
    {
        var timer = new System.Timers.Timer
        {
            Interval = 1000,
            AutoReset = true
        };

        timer.Elapsed += async (sender, e) =>
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await SendReadingToServer();
            });
        };

        timer.Start();
    }

    private async Task SendReadingToServer()
    {
        try
        {
            var response = await _serverRequestHandler.SendRequestToServer(
                CurrentMeterReading.Usage,
                RequestType.CurrentUsage,
                TariffType,
                1, // BillingPeriod
                StandingCharge,
                Mpan
            );

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                await MainThread.InvokeOnMainThreadAsync(() => ShowError(response.ErrorMessage));
            }
            else if (response.Cost is not null)
            {
                CurrentMeterReading.Cost += response.Cost.Value;
                CurrentMeterReading.Usage += _randomisedValueProvider.GenerateRandomReading();

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    RefreshActiveTab(); // Refresh the active tab only
                });
            }
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(() => ShowError($"Unexpected error: {ex.Message}"));
        }
    }

    private void RefreshActiveTab()
    {
        if (_activeTab == CurrentUsageTab)
        {
            UsageCost.Text = $"£{CurrentMeterReading.Cost}";
            UsageKW.Text = $"{CurrentMeterReading.Usage}KW";
        }
        else if (_activeTab == TodayUsageTab)
        {
            UsageCost.Text = $"£{DailyMeterReading.Cost}";
            UsageKW.Text = $"{DailyMeterReading.Usage}KW";
        }
        else if (_activeTab == WeekUsageTab)
        {
            UsageCost.Text = $"£{WeeklyMeterReading.Cost}";
            UsageKW.Text = $"{WeeklyMeterReading.Usage}KW";
        }
    }

    private void CheckAndUpdateDailyUsage()
    {
        var currentDate = DateTime.Now.Date;
        if (currentDate > LastUpdatedDate)
        {
            LastUpdatedDate = currentDate;

            DailyMeterReading = new MeterReadings
            {
                Cost = 0,
                Usage = _randomisedValueProvider.GenerateRandomReading()
            };

            MainThread.BeginInvokeOnMainThread(() => RefreshActiveTab());
        }
    }

    private void UpdateTimeDisplay()
    {
        TimeDisplay.Text = DateTime.Now.ToString("HH:mm");
    }

    private void OnTabClicked(object sender, EventArgs e)
    {
        if (sender is Button clickedButton)
        {
            SelectTab(clickedButton, clickedButton.Text);
        }
    }

    private void SelectTab(Button selectedButton, string tabName)
    {
        // Update active tab reference
        _activeTab = selectedButton;

        CurrentUsageTab.BackgroundColor = Color.FromArgb("#323232");
        TodayUsageTab.BackgroundColor = Color.FromArgb("#323232");
        WeekUsageTab.BackgroundColor = Color.FromArgb("#323232");

        selectedButton.BackgroundColor = Color.FromArgb("#345365");

        RefreshActiveTab(); // Update display for the selected tab
    }

    private void ShowError(string message)
    {
        ErrorMessage.Text = message;
        ErrorOverlay.IsVisible = true;
    }

    private async Task ShowErrorOverlay(string errorMessage)
    {
        ErrorMessage.Text = errorMessage;
        ErrorOverlay.Opacity = 0;
        ErrorOverlay.IsVisible = true;
        await ErrorOverlay.FadeTo(1, 250);
    }

    private async void OnErrorDismissed(object sender, EventArgs e)
    {
        await ErrorOverlay.FadeTo(0, 250);
        ErrorOverlay.IsVisible = false;
    }
}

