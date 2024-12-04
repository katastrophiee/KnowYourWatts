using KnowYourWatts.ClientUI.DTO.Enums;
using KnowYourWatts.ClientUI.DTO.Models;
using KnowYourWatts.ClientUI.Interfaces;

namespace KnowYourWatts.ClientUI;

public partial class MainPage : ContentPage
{
    public MeterReadings _currentMeterReading = null!;
    public MeterReadings _dailyMeterReading = null!;
    public MeterReadings _weeklyMeterReading = null!;

    private DateTime _resetDailyReadingsDate;
    private DateTime _resetWeeklyReadingsDate;

    readonly IRandomisedValueProvider _randomisedValueProvider;
    readonly IServerRequestHandler _serverRequestHandler;

    private TariffType _tariffType;
    private decimal _standingCharge;
    private string _mpan;

    private Button _activeTab = null!;

    private readonly IMainThreadService _mainThreadService; 

    public MainPage(IRandomisedValueProvider randomisedValueProvider, IServerRequestHandler serverRequestHandler, IMainThreadService mainThreadService)
    {
        _randomisedValueProvider = randomisedValueProvider;
        _serverRequestHandler = serverRequestHandler;
        _mainThreadService = mainThreadService;

        _serverRequestHandler.ErrorMessage += ShowError;

        _currentMeterReading = new()
        {
            Cost = 0m,
            Usage = 0m
        };

        _dailyMeterReading = new()
        {
            Cost = 0m,
            Usage = 0m
        };

        _weeklyMeterReading = new()
        {
            Cost = 0m,
            Usage = 0m
        };

        _tariffType = (TariffType)Enum.ToObject(typeof(TariffType), _randomisedValueProvider.GenerateRandomTariff());
        _standingCharge = _randomisedValueProvider.GenerateRandomStandingCharge();
        _mpan = _randomisedValueProvider.GenerateMpanForClient();

        InitializeComponent();

        SelectTab(CurrentUsageTab, "Current Usages");
        StartClock();
        StartRandomReadingTimer();

        _resetDailyReadingsDate = DateTime.Now.Date.AddDays(1).AddTicks(-1);
        _resetWeeklyReadingsDate = DateTime.Now.Date.AddDays((DayOfWeek.Monday - DateTime.Now.DayOfWeek + 7) % 7).AddTicks(-1); ;

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
            Interval = _randomisedValueProvider.GenerateRandomTimeDelay(),
            AutoReset = false
        };

        timer.Elapsed += async (sender, e) =>
        {
            timer.Stop();

            var randomReading = _randomisedValueProvider.GenerateRandomReading();

            _currentMeterReading.Usage = randomReading;
            _dailyMeterReading.Usage += randomReading;
            _weeklyMeterReading.Usage += randomReading;

            await SendReadingToServerCurrent();
            await SendReadingToServerDaily();
            await SendReadingToServerWeekly();

            timer.Start();
        };

        timer.Start();
    }

    public async Task SendReadingToServerCurrent()
    {
        try
        {
            var response = await _serverRequestHandler.SendRequestToServer(
                _currentMeterReading.Usage,
                _currentMeterReading.Cost,
                RequestType.CurrentUsage,
                _tariffType,
                1,
                _standingCharge,
                _mpan
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
                _currentMeterReading.Cost = response.Cost.Value;
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
                _dailyMeterReading.Usage,
                _dailyMeterReading.Cost,
                RequestType.TodaysUsage,
                _tariffType,
                1,
                _standingCharge,
                _mpan
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
                _dailyMeterReading.Cost += response.Cost.Value;
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
                _weeklyMeterReading.Usage,
                _weeklyMeterReading.Cost,
                RequestType.WeeklyUsage,
                _tariffType,
                7,
                _standingCharge,
                _mpan
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
                _weeklyMeterReading.Cost += response.Cost.Value;
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
            UsageCost.Text = $"£{_currentMeterReading.Cost}";
            UsageKW.Text = $"{decimal.Round(_currentMeterReading.Usage,2)}kWh";
        }
        else if (_activeTab == TodayUsageTab)
        {
            UsageCost.Text = $"£{_dailyMeterReading.Cost}";
            UsageKW.Text = $"{decimal.Round(_dailyMeterReading.Usage,2)}kW";
        }
        else if (_activeTab == WeekUsageTab)
        {
            UsageCost.Text = $"£{_weeklyMeterReading.Cost}";
            UsageKW.Text = $"{decimal.Round(_weeklyMeterReading.Usage, 2)}kW";
        }
    }

    private void UpdateTimeDisplay()
    {
        var currentTime = DateTime.Now;

        TimeDisplay.Text = currentTime.ToString("HH:mm");

        if (currentTime >= _resetDailyReadingsDate)
        {
            _dailyMeterReading.Cost = 0;
            _dailyMeterReading.Usage = 0;

            _resetDailyReadingsDate = DateTime.Now.Date.AddDays(1).AddTicks(-1);

            RefreshActiveTab();
        }

        if (currentTime >= _resetWeeklyReadingsDate)
        {
            _weeklyMeterReading.Cost = 0;
            _weeklyMeterReading.Usage = 0;

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

