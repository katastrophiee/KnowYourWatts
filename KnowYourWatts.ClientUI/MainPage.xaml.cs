using KnowYourWatts.ClientUI.DTO.Enums;
using KnowYourWatts.ClientUI.DTO.Models;
using KnowYourWatts.ClientUI.Interfaces;

namespace KnowYourWatts.ClientUI;

public partial class MainPage : ContentPage
{
    public MeterReadings CurrentMeterReading = null!;
    public MeterReadings DailyMeterReading = null!;
    public MeterReadings WeeklyMeterReading = null!;

    private DateTime _resetDailyReadingsDate;
    private DateTime _resetWeeklyReadingsDate;

    readonly IRandomisedValueProvider _randomisedValueProvider;
    readonly IServerRequestHandler _serverRequestHandler;

    private TariffType TariffType;
    private decimal StandingCharge;
    private string Mpan;

    private Button _activeTab = null!;

    private readonly IMainThreadService _mainThreadService; 

    public MainPage(IRandomisedValueProvider randomisedValueProvider, IServerRequestHandler serverRequestHandler, IMainThreadService mainThreadService)
    {
        _randomisedValueProvider = randomisedValueProvider;
        _serverRequestHandler = serverRequestHandler;
         _mainThreadService = mainThreadService;

        _serverRequestHandler.ErrorMessage += ShowError;

        CurrentMeterReading = new()
        {
            Cost = 0m,
            Usage = 0m
        };

        DailyMeterReading = new()
        {
            Cost = 0m,
            Usage = 0m
        };

        WeeklyMeterReading = new()
        {
            Cost = 0m,
            Usage = 0m
        };

        TariffType = (TariffType)Enum.ToObject(typeof(TariffType), _randomisedValueProvider.GenerateRandomTarrif());
        StandingCharge = _randomisedValueProvider.GenerateRandomStandingCharge();
        Mpan = _randomisedValueProvider.GenerateMpanForClient();

        InitializeComponent();

        SelectTab(CurrentUsageTab, "Current Usages");
        StartClock();
        StartRandomReadingTimer();

        _resetDailyReadingsDate = DateTime.Now.Date.AddDays(1).AddTicks(-1);
        _resetWeeklyReadingsDate = DateTime.Now.Date.AddDays((7 - (int)DateTime.Now.DayOfWeek) % 7).AddTicks(-1);

    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        //await _serverRequestHandler.GetPublicKey();
    }

    private void StartClock()
    {
        var clockTimer = new System.Timers.Timer(1000) { AutoReset = true };

        clockTimer.Elapsed += (sender, e) =>
        {
            _mainThreadService.BeginInvokeOnMainThread(() =>
            {
                UpdateTimeDisplay();
            });
        };

        clockTimer.Start();
    }

    private void StartRandomReadingTimer()
    {
        var timer = new System.Timers.Timer
        {
            Interval = 2000,//_randomisedValueProvider.GenerateRandomTimeDelay(),
            AutoReset = false
        };

        timer.Elapsed += async (sender, e) =>
        {
            timer.Stop();

            var randomReading = _randomisedValueProvider.GenerateRandomReading();

            CurrentMeterReading.Usage = randomReading;
            DailyMeterReading.Usage += randomReading;
            WeeklyMeterReading.Usage += randomReading;

            await SendCurrentReadingToServer();
            await SendReadingToServerDaily();
            await SendReadingToServerWeekly();

            timer.Start();
        };

        timer.Start();
    }

    public async Task SendCurrentReadingToServer()
    {
        try
        {
            var response = await _serverRequestHandler.SendRequestToServer(
                CurrentMeterReading.Usage,
                CurrentMeterReading.Cost,
                RequestType.CurrentUsage,
                TariffType,
                1,
                StandingCharge,
                Mpan
            );

            if (response is null)
            {
                ShowError("No response was received from the server.");
                return;
            }

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                ShowError(response.ErrorMessage);
                return;
            }

            else if (response.Cost is not null)
            {
                CurrentMeterReading.Cost = response.Cost.Value;
            }

            await MainThread.InvokeOnMainThreadAsync(RefreshActiveTab);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    public async Task SendReadingToServerDaily()
    {
        try
        {
            var response = await _serverRequestHandler.SendRequestToServer(
                DailyMeterReading.Usage,
                DailyMeterReading.Cost,
                RequestType.TodaysUsage,
                TariffType,
                1,
                StandingCharge,
                Mpan
            );

            if (response is null)
            {
                ShowError("No response was received from the server.");
                return;
            }

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                ShowError(response.ErrorMessage);
                return;
            }

            else if (response.Cost is not null)
            {
                DailyMeterReading.Cost += response.Cost.Value;
            }

            await MainThread.InvokeOnMainThreadAsync(RefreshActiveTab);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    public async Task SendReadingToServerWeekly()
    {
        try
        {
            var response = await _serverRequestHandler.SendRequestToServer(
                WeeklyMeterReading.Usage,
                WeeklyMeterReading.Cost,
                RequestType.WeeklyUsage,
                TariffType,
                7,
                StandingCharge,
                Mpan
            );

            if (response is null)
            {
                ShowError("No response was received from the server.");
                return;
            }

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                ShowError(response.ErrorMessage);
                return;
            }

            else if (response.Cost is not null)
            {
                WeeklyMeterReading.Cost += response.Cost.Value;
            }

            await MainThread.InvokeOnMainThreadAsync(RefreshActiveTab);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void RefreshActiveTab()
    {
        if (_activeTab == CurrentUsageTab)
        {
            UsageCost.Text = $"£{CurrentMeterReading.Cost}";
            UsageKW.Text = $"{decimal.Round(CurrentMeterReading.Usage,2)}KW";
        }
        else if (_activeTab == TodayUsageTab)
        {
            UsageCost.Text = $"£{DailyMeterReading.Cost}";
            UsageKW.Text = $"{decimal.Round(DailyMeterReading.Usage,2)}KW";
        }
        else if (_activeTab == WeekUsageTab)
        {
            UsageCost.Text = $"£{WeeklyMeterReading.Cost}";
            UsageKW.Text = $"{decimal.Round(WeeklyMeterReading.Usage, 2)}KW";
        }
    }

    private void UpdateTimeDisplay()
    {
        var currentTime = DateTime.Now;

        TimeDisplay.Text = currentTime.ToString("HH:mm");

        if (currentTime >= _resetDailyReadingsDate)
        {
            DailyMeterReading.Cost = 0;
            DailyMeterReading.Usage = 0;

            _resetDailyReadingsDate = DateTime.Now.Date.AddDays(1).AddTicks(-1);

            RefreshActiveTab();
        }

        if (currentTime >= _resetWeeklyReadingsDate)
        {
            WeeklyMeterReading.Cost = 0;
            WeeklyMeterReading.Usage = 0;

            _resetWeeklyReadingsDate = DateTime.Now.Date.AddDays((DayOfWeek.Monday - DateTime.Now.DayOfWeek + 7) % 7).AddTicks(-1);

            RefreshActiveTab();
        }
    }

    private void OnTabClicked(object sender, EventArgs e)
    {
        if (sender is Button clickedButton)
            SelectTab(clickedButton, clickedButton.Text);
    }

    private void SelectTab(Button selectedButton, string tabName)
    {
        // Update active tab reference
        _activeTab = selectedButton;

        CurrentUsageTab.BackgroundColor = Color.FromArgb("#323232");
        TodayUsageTab.BackgroundColor = Color.FromArgb("#323232");
        WeekUsageTab.BackgroundColor = Color.FromArgb("#323232");

        selectedButton.BackgroundColor = Color.FromArgb("#345365");

        // Update display for the selected tab
        RefreshActiveTab(); 
    }

    private void ShowError(string message)
    {
        _mainThreadService.BeginInvokeOnMainThread(() =>
        {
            ErrorMessage.Text = message;
            ErrorOverlay.IsVisible = true;
        });
    }

    private async void OnErrorDismissed(object sender, EventArgs e)
    {
        await ErrorOverlay.FadeTo(0, 250);
        ErrorOverlay.IsVisible = false;
    }
}

