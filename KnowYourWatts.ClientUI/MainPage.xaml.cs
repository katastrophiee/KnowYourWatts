using KnowYourWatts.ClientUI.DTO.Enums;
using KnowYourWatts.ClientUI.DTO.Models;
using KnowYourWatts.ClientUI.DTO.Response;
using KnowYourWatts.ClientUI.Interfaces;

namespace KnowYourWatts.ClientUI;

public partial class MainPage : ContentPage
{
    private MeterReadings CurrentMeterReading = null!;
    private MeterReadings DailyMeterReading = null!;
    private MeterReadings WeeklyMeterReading = null!;

    readonly IRandomisedValueProvider _randomisedValueProvider;
    readonly IServerRequestHandler _serverRequestHandler;
    readonly IEncryptionHelper _encryptionHelper;

    private TariffType TariffType;
    private decimal StandingCharge;
    private string Mpan;
    private byte[] EncryptedMpan;


    private Button _activeTab = null!;

    public MainPage(IRandomisedValueProvider randomisedValueProvider, IServerRequestHandler serverRequestHandler, IEncryptionHelper encryptionHelper)
    {
        _randomisedValueProvider = randomisedValueProvider;
        _serverRequestHandler = serverRequestHandler;
        _encryptionHelper = encryptionHelper;

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

        //we'll want the usage to go up by the same for each page, not have each one be individually calculated

        TariffType = (TariffType)Enum.ToObject(typeof(TariffType), _randomisedValueProvider.GenerateRandomTarrif());
        StandingCharge = _randomisedValueProvider.GenerateRandomStandingCharge();
        Mpan = _randomisedValueProvider.GenerateMpanForClient();

        InitializeComponent();

        SelectTab(CurrentUsageTab, "Current Usages");
        StartClock(); // Start clock independently
        StartRandomCurrentReadingTimer();
        StartRandomWeeklyReadingTimer();
    }

    private void StartClock()
    {
        var clockTimer = new System.Timers.Timer(1000) { AutoReset = true };

        clockTimer.Elapsed += (sender, e) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UpdateTimeDisplay();
            });
        };

        clockTimer.Start();
    }

    private void StartRandomCurrentReadingTimer()
    {
        var timer = new System.Timers.Timer
        {
            Interval = 1000,//_randomisedValueProvider.GenerateRandomTimeDelay(),
            AutoReset = true
        };

        timer.Elapsed += async (sender, e) =>
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await SendCurrentReadingToServer();
                await SendReadingToServerDaily();
            }); 
            
        };

        timer.Start();
    }
    private void StartRandomWeeklyReadingTimer()
    {
        //run every hour
        var timer = new System.Timers.Timer
        {
            Interval = 1000,//(60*60*1000)*24,
            AutoReset = true
        };

        timer.Elapsed += async (sender, e) =>
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await SendReadingToServerWeekly();
            });
            
        };

        timer.Start();
    }
    private async Task SendCurrentReadingToServer()
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
                Mpan,
                EncryptedMpan
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

                //move at some point
                CurrentMeterReading.Usage = _randomisedValueProvider.GenerateRandomReading();
            }
            await MainThread.InvokeOnMainThreadAsync(RefreshActiveTab);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private async Task SendReadingToServerDaily()
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
                Mpan,
                EncryptedMpan
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

                //move at some point
                DailyMeterReading.Usage += CurrentMeterReading.Usage;
            }
            
            await MainThread.InvokeOnMainThreadAsync(RefreshActiveTab);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private async Task SendReadingToServerWeekly()
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
                Mpan,
                EncryptedMpan
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

                //move at some point to it's own timer
                WeeklyMeterReading.Usage += DailyMeterReading.Usage;
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
        TimeDisplay.Text = DateTime.Now.ToString("HH:mm");
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
        ErrorMessage.Text = message;
        ErrorOverlay.IsVisible = true;
    }

    private async void OnErrorDismissed(object sender, EventArgs e)
    {
        await ErrorOverlay.FadeTo(0, 250);
        ErrorOverlay.IsVisible = false;
    }
}

