using KnowYourWatts.ClientUI;
using KnowYourWatts.ClientUI.DTO.Enums;
using KnowYourWatts.ClientUI.DTO.Response;
using KnowYourWatts.ClientUI.Interfaces;
using NSubstitute;
using System.Reflection;

namespace KnowYourWatts.Tests;

public class MainPageTests
{
    private IRandomisedValueProvider _randomiseValueProvider = null!;
    private IServerRequestHandler _serverRequestHandler = null!;
    private IMainThreadService _mainThreadService = null!;
    private MainPage _mainPage = null!;

    [SetUp]
    public void SetUp()
    {
        _randomiseValueProvider = Substitute.For<IRandomisedValueProvider>();
        _serverRequestHandler = Substitute.For<IServerRequestHandler>();
        _mainThreadService = Substitute.For<IMainThreadService>();
        _randomiseValueProvider.GenerateMpanForClient().Returns("1234567890123");
        _randomiseValueProvider.GenerateRandomReading().Returns(1.5m);
        _randomiseValueProvider.GenerateRandomStandingCharge().Returns(0.5m);
        _randomiseValueProvider.GenerateRandomTariff().Returns((int)TariffType.Fixed);
        _randomiseValueProvider.GenerateRandomTimeDelay().Returns(1);

        _mainPage = new MainPage(
            _randomiseValueProvider,
            _serverRequestHandler,
            _mainThreadService
        );
    }

    /// <summary>
    /// Verifies that SendCurrentReadingToServer updates the current meter reading 
    /// cost when the server responds with a successful result.
    /// </summary>
    [Test]
    public async Task SendCurrentReadingToServer_SuccessfulResponse_UpdatesCurrentMeterReading()
    {
        // Arrange
        var mockResponse = new SmartMeterCalculationResponse
        {
            Cost = 2.5m,
            ErrorMessage = ""
        };

        _serverRequestHandler
            .SendRequestToServer(Arg.Any<decimal>(), Arg.Any<decimal>(), Arg.Any<RequestType>(), Arg.Any<TariffType>(), Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<string>())
            .Returns(mockResponse);

        _mainThreadService.BeginInvokeOnMainThread(Arg.Any<Action>());

        // Act
        await _mainPage.SendReadingToServerCurrent();

        // Assert
        Assert.That(_mainPage._currentMeterReading.Cost, Is.EqualTo(2.5));
    }

    /// <summary>
    /// Verifies that SendCurrentReadingToServer handles server errors 
    /// by showing the appropriate error message to the user.
    /// </summary>
    [Test]
    public async Task SendCurrentReadingToServer_ErrorResponse_ShowsError()
    {
        // Arrange
        var errorMessage = "Test error";

        var mockResponse = new SmartMeterCalculationResponse
        {
            Cost = null,
            ErrorMessage = errorMessage
        };

        _serverRequestHandler.SendRequestToServer(
            Arg.Any<decimal>(),
            Arg.Any<decimal>(),
            Arg.Any<RequestType>(),
            Arg.Any<TariffType>(),
            Arg.Any<int>(),
            Arg.Any<decimal>(),
            Arg.Any<string>())
        .Returns(mockResponse);

        // Act
        await _mainPage.SendReadingToServerCurrent();

        // Assert
        await _serverRequestHandler.Received(1).SendRequestToServer(
            Arg.Any<decimal>(),
            Arg.Any<decimal>(),
            Arg.Any<RequestType>(),
            Arg.Any<TariffType>(),
            Arg.Any<int>(),
            Arg.Any<decimal>(),
            Arg.Any<string>()
        );
    }

    /// <summary>
    /// Verifies that the daily usage and cost are reset to 0 at midnight.
    /// Ensures that the `_resetDailyReadingsDate` field updates correctly for the next reset cycle.
    /// </summary>
    [Test]
    public void UpdateTimeDisplay_ResetsDailyUsageAtMidnight()
    {
        // Arrange
        _mainPage._dailyMeterReading.Usage = 10m;
        _mainPage._dailyMeterReading.Cost = 5m;
        _mainPage.GetType().GetField("_resetDailyReadingsDate", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(_mainPage, DateTime.Now.Date.AddSeconds(-1));

        // Act
        _mainPage.GetType().GetMethod("UpdateTimeDisplay", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.Invoke(_mainPage, null);

        // Assert
        Assert.That(_mainPage._dailyMeterReading.Usage, Is.EqualTo(0));
        Assert.That(_mainPage._dailyMeterReading.Cost, Is.EqualTo(0));
    }

    /// <summary>
    /// Verifies that the weekly usage and cost are reset to 0 at the end of Sunday.
    /// Ensures that `_resetWeeklyReadingsDate` is updated correctly for the next reset cycle.
    /// </summary>
    [Test]
    public void UpdateTimeDisplay_ResetsWeeklyUsageOnSunday()
    {
        // Arrange
        var lastSunday = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek).AddSeconds(-1);

        _mainPage._weeklyMeterReading.Usage = 20m;
        _mainPage._weeklyMeterReading.Cost = 10m;
        _mainPage.GetType().GetField("_resetWeeklyReadingsDate", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(_mainPage, lastSunday);

        // Act
        _mainPage.GetType().GetMethod("UpdateTimeDisplay", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.Invoke(_mainPage, null);

        // Assert
        Assert.That(_mainPage._weeklyMeterReading.Usage, Is.EqualTo(0), "Weekly usage should be reset to 0.");
        Assert.That(_mainPage._weeklyMeterReading.Cost, Is.EqualTo(0), "Weekly cost should be reset to 0.");
    }

    /// <summary>
    /// Verifies that the cost for the current meter reading is updated correctly when a valid response is received.
    /// Ensures the server response is properly processed.
    /// </summary>
    [Test]
    public async Task SendCurrentReadingToServer_ValidResponse_UpdatesCost()
    {
        // Arrange
        var mockResponse = new SmartMeterCalculationResponse
        {
            Cost = 2.5m,
            ErrorMessage = ""
        };

        _serverRequestHandler.SendRequestToServer(
            Arg.Any<decimal>(),
            Arg.Any<decimal>(),
            Arg.Any<RequestType>(),
            Arg.Any<TariffType>(),
            Arg.Any<int>(),
            Arg.Any<decimal>(),
            Arg.Any<string>())
        .Returns(mockResponse);

        // Act
        await _mainPage.SendReadingToServerCurrent();

        // Assert
        Assert.That(_mainPage._currentMeterReading.Cost, Is.EqualTo(2.5m), "Current meter reading cost should be updated.");
    }

    /// <summary>
    /// Verifies that the cost for the daily meter reading is updated correctly when a valid response is received.
    /// Ensures the server response is properly processed.
    /// </summary>
    [Test]
    public async Task SendDailyReadingToServer_ValidResponse_UpdatesCost()
    {
        // Arrange
        var mockResponse = new SmartMeterCalculationResponse
        {
            Cost = 3.5m,
            ErrorMessage = ""
        };

        _serverRequestHandler.SendRequestToServer(
            Arg.Any<decimal>(),
            Arg.Any<decimal>(),
            Arg.Any<RequestType>(),
            Arg.Any<TariffType>(),
            Arg.Any<int>(),
            Arg.Any<decimal>(),
            Arg.Any<string>())
        .Returns(mockResponse);

        // Act
        await _mainPage.SendReadingToServerDaily();

        // Assert
        Assert.That(_mainPage._dailyMeterReading.Cost, Is.EqualTo(3.5m), "Daily meter reading cost should be updated.");
    }

    /// <summary>
    /// Verifies that the cost for the weekly meter reading is updated correctly when a valid response is received.
    /// Ensures the server response is properly processed.
    /// </summary>
    [Test]
    public async Task SendWeeklyReadingToServer_ValidResponse_UpdatesCost()
    {
        // Arrange
        var mockResponse = new SmartMeterCalculationResponse
        {
            Cost = 5.0m,
            ErrorMessage = ""
        };

        _serverRequestHandler.SendRequestToServer(
            Arg.Any<decimal>(),
            Arg.Any<decimal>(),
            Arg.Any<RequestType>(),
            Arg.Any<TariffType>(),
            Arg.Any<int>(),
            Arg.Any<decimal>(),
            Arg.Any<string>())
        .Returns(mockResponse);

        // Act
        await _mainPage.SendReadingToServerWeekly();

        // Assert
        Assert.That(_mainPage._weeklyMeterReading.Cost, Is.EqualTo(5.0m), "Weekly meter reading cost should be updated.");
    }

    /// <summary>
    /// Verifies that an error response for the current meter reading is handled correctly.
    /// Ensures the error message is displayed, and no updates are made to the current meter reading cost.
    /// </summary>
    [Test]
    public async Task SendCurrentReadingToServer_ErrorResponse_ShowsErrorMessage()
    {
        // Arrange
        var errorMessage = "Test error";
        var mockResponse = new SmartMeterCalculationResponse
        {
            Cost = null,
            ErrorMessage = errorMessage
        };

        _serverRequestHandler.SendRequestToServer(
            Arg.Any<decimal>(),
            Arg.Any<decimal>(),
            Arg.Any<RequestType>(),
            Arg.Any<TariffType>(),
            Arg.Any<int>(),
            Arg.Any<decimal>(),
            Arg.Any<string>())
        .Returns(mockResponse);

        // Act
        await _mainPage.SendReadingToServerCurrent();

        // Assert
        _mainThreadService.Received().BeginInvokeOnMainThread(Arg.Any<Action>());

        Assert.That(_mainPage._currentMeterReading.Cost, Is.EqualTo(0), "Current meter reading cost should remain unchanged.");
    }

    /// <summary>
    /// Verifies that an error response for the daily meter reading is handled correctly.
    /// Ensures the error message is displayed, and no updates are made to the daily meter reading cost.
    /// </summary>
    [Test]
    public async Task SendDailyReadingToServer_ErrorResponse_ShowsErrorMessage()
    {
        // Arrange
        var errorMessage = "Daily reading error";
        var mockResponse = new SmartMeterCalculationResponse
        {
            Cost = null,
            ErrorMessage = errorMessage
        };

        _serverRequestHandler.SendRequestToServer(
            Arg.Any<decimal>(),
            Arg.Any<decimal>(),
            Arg.Any<RequestType>(),
            Arg.Any<TariffType>(),
            Arg.Any<int>(),
            Arg.Any<decimal>(),
            Arg.Any<string>())
        .Returns(mockResponse);

        // Act
        await _mainPage.SendReadingToServerDaily();

        // Assert
        _mainThreadService.Received().BeginInvokeOnMainThread(Arg.Any<Action>());

        Assert.That(_mainPage._dailyMeterReading.Cost, Is.EqualTo(0), "Daily meter reading cost should remain unchanged.");
    }

    /// <summary>
    /// Verifies that an error response for the weekly meter reading is handled correctly.
    /// Ensures the error message is displayed, and no updates are made to the weekly meter reading cost.
    /// </summary>
    [Test]
    public async Task SendWeeklyReadingToServer_ErrorResponse_ShowsErrorMessage()
    {
        // Arrange
        var errorMessage = "Weekly reading error";
        var mockResponse = new SmartMeterCalculationResponse
        {
            Cost = null,
            ErrorMessage = errorMessage
        };

        _serverRequestHandler.SendRequestToServer(
             Arg.Any<decimal>(),
             Arg.Any<decimal>(),
             Arg.Any<RequestType>(),
             Arg.Any<TariffType>(),
             Arg.Any<int>(),
             Arg.Any<decimal>(),
             Arg.Any<string>())
         .Returns(mockResponse);

        // Act
        await _mainPage.SendReadingToServerWeekly();

        // Assert
        _mainThreadService.Received().BeginInvokeOnMainThread(Arg.Any<Action>());
        Assert.That(_mainPage._weeklyMeterReading.Cost, Is.EqualTo(0), "Weekly meter reading cost should remain unchanged.");
    }
}