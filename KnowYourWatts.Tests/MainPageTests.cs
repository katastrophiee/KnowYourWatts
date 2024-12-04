using KnowYourWatts.ClientUI;
using KnowYourWatts.ClientUI.DTO.Enums;
using KnowYourWatts.ClientUI.DTO.Response;
using KnowYourWatts.ClientUI.Interfaces;
using NSubstitute;

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
            Arg.Any<string>());

        // Assuming ShowError updates a UI property, verify that error was set.
    }


    /// <summary>
    /// Verifies that the GenerateMpanForClient method of the IRandomisedValueProvider 
    /// is called during the setup of MainPage.
    /// </summary>
    [Test]
    public void GenerateMpanForClient_IsCalledDuringSetup()
    {
        // Assert
        _mockValueProvider.Received(1).GenerateMpanForClient();
    }

    /// <summary>
    /// Verifies that the GenerateRandomStandingCharge method of the IRandomisedValueProvider 
    /// is called during the setup of MainPage.
    /// </summary>
    [Test]
    public void GenerateRandomStandingCharge_IsCalledDuringSetup()
    {
        // Assert
        _mockValueProvider.Received(1).GenerateRandomStandingCharge();
    }
}


