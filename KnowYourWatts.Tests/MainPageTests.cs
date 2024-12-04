using KnowYourWatts.ClientUI;
using KnowYourWatts.ClientUI.DTO.Enums;
using KnowYourWatts.ClientUI.DTO.Response;
using KnowYourWatts.ClientUI.Interfaces;
using NSubstitute;
using System.Reflection;

namespace KnowYourWatts.Tests;

[TestFixture]
public class MainPageTests
{
    private IRandomisedValueProvider _mockValueProvider;
    private IServerRequestHandler _mockServerRequestHandler;
    private IMainThreadService _mockMainThreadService;
    private MainPage _mainPage;

    [SetUp]
    public void SetUp()
    {
        _mockValueProvider = Substitute.For<IRandomisedValueProvider>();
        _mockServerRequestHandler = Substitute.For<IServerRequestHandler>();
        _mockMainThreadService = Substitute.For<IMainThreadService>();
        _mockValueProvider.GenerateMpanForClient().Returns("1234567890123");
        _mockValueProvider.GenerateRandomReading().Returns(1.5m);
        _mockValueProvider.GenerateRandomStandingCharge().Returns(0.5m);
        _mockValueProvider.GenerateRandomTarrif().Returns((int)TariffType.Fixed);

        _mainPage = new MainPage(
            _mockValueProvider,
            _mockServerRequestHandler,
            _mockMainThreadService
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
        _mockServerRequestHandler
            .SendRequestToServer(Arg.Any<decimal>(), Arg.Any<decimal>(), Arg.Any<RequestType>(), Arg.Any<TariffType>(), Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<string>())
            .Returns(mockResponse);
        _mockMainThreadService.BeginInvokeOnMainThread(Arg.Any<Action>());
        // Act
        await _mainPage.SendCurrentReadingToServer();

        // Assert
        Assert.That(_mainPage.CurrentMeterReading.Cost, Is.EqualTo(2.5));
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

        _mockServerRequestHandler
            .SendRequestToServer(Arg.Any<decimal>(), Arg.Any<decimal>(), Arg.Any<RequestType>(), Arg.Any<TariffType>(), Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<string>())
            .Returns(Task.FromResult(mockResponse));

        // Act
        await _mainPage.SendCurrentReadingToServer();

        // Assert
        await _mockServerRequestHandler.Received(1).SendRequestToServer(
            Arg.Any<decimal>(),
            Arg.Any<decimal>(),
            Arg.Any<RequestType>(),
            Arg.Any<TariffType>(),
            Arg.Any<int>(),
            Arg.Any<decimal>(),
            Arg.Any<string>()
        );

        // Assuming ShowError updates a UI property, verify that error was set.
    }

    /// <summary>
    /// Verifies that the daily usage and cost are reset to 0 at midnight.
    /// Ensures that the `_resetDailyReadingsDate` field updates correctly for the next reset cycle.
    /// </summary>
    [Test]
    public void UpdateTimeDisplay_ResetsDailyUsageAtMidnight()
    {
        // Arrange
        _mainPage.DailyMeterReading.Usage = 10m;
        _mainPage.DailyMeterReading.Cost = 5m;

        _mainPage.GetType().GetField("_resetDailyReadingsDate", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(_mainPage, DateTime.Now.Date.AddSeconds(-1));

        // Act
        _mainPage.GetType().GetMethod("UpdateTimeDisplay", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.Invoke(_mainPage, null);

        // Assert
        Assert.That(_mainPage.DailyMeterReading.Usage, Is.EqualTo(0));
        Assert.That(_mainPage.DailyMeterReading.Cost, Is.EqualTo(0));
    }

    /// <summary>
    /// Verifies that the weekly usage and cost are reset to 0 at the end of Sunday.
    /// Ensures that `_resetWeeklyReadingsDate` is updated correctly for the next reset cycle.
    /// </summary>
    [Test]
    public void UpdateTimeDisplay_ResetsWeeklyUsageOnSunday()
    {
        // Arrange
        _mainPage.WeeklyMeterReading.Usage = 20m;
        _mainPage.WeeklyMeterReading.Cost = 10m;

        // Set the _resetWeeklyReadingsDate to a time that has just passed
        var lastSunday = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek).AddSeconds(-1);
        _mainPage.GetType().GetField("_resetWeeklyReadingsDate", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(_mainPage, lastSunday);

        // Act
        _mainPage.GetType().GetMethod("UpdateTimeDisplay", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.Invoke(_mainPage, null);

        // Assert
        Assert.That(_mainPage.WeeklyMeterReading.Usage, Is.EqualTo(0), "Weekly usage should be reset to 0.");
        Assert.That(_mainPage.WeeklyMeterReading.Cost, Is.EqualTo(0), "Weekly cost should be reset to 0.");
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
            ErrorMessage = null
        };

        _mockServerRequestHandler.SendRequestToServer(Arg.Any<decimal>(), Arg.Any<decimal>(), Arg.Any<RequestType>(),
                                                      Arg.Any<TariffType>(), Arg.Any<int>(), Arg.Any<decimal>(),
                                                      Arg.Any<string>())
                                 .Returns(mockResponse);

        // Act
        await _mainPage.SendCurrentReadingToServer();

        // Assert
        Assert.That(_mainPage.CurrentMeterReading.Cost, Is.EqualTo(2.5m), "Current meter reading cost should be updated.");
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
            ErrorMessage = null
        };

        _mockServerRequestHandler.SendRequestToServer(Arg.Any<decimal>(), Arg.Any<decimal>(), Arg.Any<RequestType>(),
                                                      Arg.Any<TariffType>(), Arg.Any<int>(), Arg.Any<decimal>(),
                                                      Arg.Any<string>())
                                 .Returns(mockResponse);

        // Act
        await _mainPage.SendReadingToServerDaily();

        // Assert
        Assert.That(_mainPage.DailyMeterReading.Cost, Is.EqualTo(3.5m), "Daily meter reading cost should be updated.");
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
            ErrorMessage = null
        };

        _mockServerRequestHandler.SendRequestToServer(Arg.Any<decimal>(), Arg.Any<decimal>(), Arg.Any<RequestType>(),
                                                      Arg.Any<TariffType>(), Arg.Any<int>(), Arg.Any<decimal>(),
                                                      Arg.Any<string>())
                                 .Returns(mockResponse);

        // Act
        await _mainPage.SendReadingToServerWeekly();

        // Assert
        Assert.That(_mainPage.WeeklyMeterReading.Cost, Is.EqualTo(5.0m), "Weekly meter reading cost should be updated.");
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

        _mockServerRequestHandler.SendRequestToServer(Arg.Any<decimal>(), Arg.Any<decimal>(), Arg.Any<RequestType>(),
                                                      Arg.Any<TariffType>(), Arg.Any<int>(), Arg.Any<decimal>(),
                                                      Arg.Any<string>())
                                 .Returns(mockResponse);

        // Act
        await _mainPage.SendCurrentReadingToServer();

        // Assert
        _mockMainThreadService.Received().BeginInvokeOnMainThread(Arg.Any<Action>());
        Assert.That(_mainPage.CurrentMeterReading.Cost, Is.EqualTo(0), "Current meter reading cost should remain unchanged.");
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

        _mockServerRequestHandler.SendRequestToServer(Arg.Any<decimal>(), Arg.Any<decimal>(), Arg.Any<RequestType>(),
                                                      Arg.Any<TariffType>(), Arg.Any<int>(), Arg.Any<decimal>(),
                                                      Arg.Any<string>())
                                 .Returns(mockResponse);

        // Act
        await _mainPage.SendReadingToServerDaily();

        // Assert
        _mockMainThreadService.Received().BeginInvokeOnMainThread(Arg.Any<Action>());
        Assert.That(_mainPage.DailyMeterReading.Cost, Is.EqualTo(0), "Daily meter reading cost should remain unchanged.");
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

        _mockServerRequestHandler.SendRequestToServer(Arg.Any<decimal>(), Arg.Any<decimal>(), Arg.Any<RequestType>(),
                                                      Arg.Any<TariffType>(), Arg.Any<int>(), Arg.Any<decimal>(),
                                                      Arg.Any<string>())
                                 .Returns(mockResponse);

        // Act
        await _mainPage.SendReadingToServerWeekly();

        // Assert
        _mockMainThreadService.Received().BeginInvokeOnMainThread(Arg.Any<Action>());
        Assert.That(_mainPage.WeeklyMeterReading.Cost, Is.EqualTo(0), "Weekly meter reading cost should remain unchanged.");
    }

}


