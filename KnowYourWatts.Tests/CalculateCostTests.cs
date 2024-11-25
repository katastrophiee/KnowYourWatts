using KnowYourWatts.MockDb.Interfaces;
using KnowYourWatts.MockDb.Repository;
using KnowYourWatts.Server;
using KnowYourWatts.Server.DTO.Enums;
using KnowYourWatts.Server.DTO.Models;
using KnowYourWatts.Server.DTO.Requests;
using NSubstitute;

namespace KnowYourWatts.Tests;

internal sealed class CalculateCostTests
{
    private ITariffRepository _tariffRepository = null!;
    private CalculationProvider _calculationProvider = null!;
    private SmartMeterCalculationRequest _request = null!;
    private IPreviousReadingRepository _previousReadingRepository = null!;
    private ICostRepository _costRepository = null!;

    [SetUp]
    public void Setup()
    {
        _tariffRepository = Substitute.For<ITariffRepository>();

        _previousReadingRepository = Substitute.For<IPreviousReadingRepository>();

        _costRepository = Substitute.For<ICostRepository>();

        _calculationProvider = new(
            _tariffRepository,
            _previousReadingRepository,
            _costRepository
        );

        _request = new()
        {
            TariffType = TariffType.Fixed,
            CurrentReading = 2,
            BillingPeriod = 1,
            StandingCharge = 1
        };

        _tariffRepository.GetTariffPriceByType(Arg.Is<TariffType>(t => t == TariffType.Fixed)).Returns(new TariffTypeAndPrice { PriceInPence = 24.50m });
        _tariffRepository.GetTariffPriceByType(Arg.Is<TariffType>(t => t == TariffType.Flex)).Returns(new TariffTypeAndPrice { PriceInPence = 26.20m });
        _tariffRepository.GetTariffPriceByType(Arg.Is<TariffType>(t => t == TariffType.Green)).Returns(new TariffTypeAndPrice { PriceInPence = 27.05m });
        _tariffRepository.GetTariffPriceByType(Arg.Is<TariffType>(t => t == TariffType.OffPeak)).Returns(new TariffTypeAndPrice { PriceInPence = 23.64m });
    }

    /// <summary>
    /// Test to ensure that cost calculations are correct for fixed tariff types
    /// This website is used to determine the expected results: https://www.electricitybillcalculator.com/
    /// </summary>
    [TestCase(10, 20, 1, 45, 3.05)]
    [TestCase(1, 2, 3, 0, 0.26)]
    [TestCase(78, 546, 7, 496, 156.85)]
    public void EnsureFixedCalculationsAreCorrect(
        decimal previousReading,
        decimal currentReading,
        int billingPeriod,
        decimal standingCharge,
        decimal expectedCostResult)
    {
        //Arrange
        _previousReadingRepository.GetPreviousReadingByMpanAndReqType(Arg.Any<string>()).Returns(previousReading);

        _request.TariffType = TariffType.Fixed;

        _request.CurrentReading = currentReading;
        _request.BillingPeriod = billingPeriod;
        _request.StandingCharge = standingCharge;

        //Act
        var result = _calculationProvider.CalculateCost(_request);

        //Assert
        Assert.That(string.IsNullOrEmpty(result.ErrorMessage), Is.True);
        Assert.That(result.Cost, Is.EqualTo(expectedCostResult));
    }

    /// <summary>
    /// Test to ensure that cost calculations are correct for flex tariff types
    /// This website is used to determine the expected results: https://www.electricitybillcalculator.com/
    /// </summary>
    [TestCase(10, 20, 1, 45, 3.22)]
    [TestCase(1, 2, 3, 0, 0.28)]
    [TestCase(78, 546, 7, 496, 165.20)]
    public void EnsureFlexCalculationsAreCorrect(
        decimal previousReading,
        decimal currentReading,
        int billingPeriod,
        decimal standingCharge,
        decimal expectedCostResult)
    {
        //Arrange
        _previousReadingRepository.GetPreviousReadingByMpanAndReqType(Arg.Any<string>()).Returns(previousReading);

        _request.TariffType = TariffType.Flex;

        _request.CurrentReading = currentReading;
        _request.BillingPeriod = billingPeriod;
        _request.StandingCharge = standingCharge;

        //Act
        var result = _calculationProvider.CalculateCost(_request);

        //Assert
        Assert.That(string.IsNullOrEmpty(result.ErrorMessage), Is.True);
        Assert.That(result.Cost, Is.EqualTo(expectedCostResult));
    }


    /// <summary>
    /// Test to ensure that cost calculations are correct for green tariff types
    /// This website is used to determine the expected results: https://www.electricitybillcalculator.com/
    /// </summary>
    [TestCase(10, 20, 1, 45, 3.31)]
    [TestCase(1, 2, 3, 0, 0.28)]
    [TestCase(78, 546, 7, 496, 169.38)]
    public void EnsureGreenCalculationsAreCorrect(
        decimal previousReading,
        decimal currentReading,
        int billingPeriod,
        decimal standingCharge,
        decimal expectedCostResult)
    {
        //Arrange
        _previousReadingRepository.GetPreviousReadingByMpanAndReqType(Arg.Any<string>()).Returns(previousReading);

        _request.TariffType = TariffType.Green;

        _request.CurrentReading = currentReading;
        _request.BillingPeriod = billingPeriod;
        _request.StandingCharge = standingCharge;

        //Act
        var result = _calculationProvider.CalculateCost(_request);

        //Assert
        Assert.That(string.IsNullOrEmpty(result.ErrorMessage), Is.True);
        Assert.That(result.Cost, Is.EqualTo(expectedCostResult));
    }


    /// <summary>
    /// Test to ensure that cost calculations are correct for off peak tariff types
    /// This website is used to determine the expected results: https://www.electricitybillcalculator.com/
    /// </summary>
    [TestCase(10, 20, 1, 45, 2.95)]
    [TestCase(1, 2, 3, 0, 0.25)]
    [TestCase(78, 546, 7, 496, 152.62)]
    public void EnsureOffPeakCalculationsAreCorrect(
        decimal previousReading,
        decimal currentReading,
        int billingPeriod,
        decimal standingCharge,
        decimal expectedCostResult)
    {
        //Arrange
        _previousReadingRepository.GetPreviousReadingByMpanAndReqType(Arg.Any<string>()).Returns(previousReading);

        _request.TariffType = TariffType.OffPeak;

        _request.CurrentReading = currentReading;
        _request.BillingPeriod = billingPeriod;
        _request.StandingCharge = standingCharge;

        //Act
        var result = _calculationProvider.CalculateCost(_request);

        //Assert
        Assert.That(string.IsNullOrEmpty(result.ErrorMessage), Is.True);
        Assert.That(result.Cost, Is.EqualTo(expectedCostResult));
    }

    /// <summary>
    /// Test to ensure that the calculated cost for the energy used is added to the previous total cost in the mock db
    /// </summary>
    [Test]
    public void TotalCostIsSaved()
    {
        //Arrange
        //NA - Handled in the setup

        //Act
        var result = _calculationProvider.CalculateCost(_request);

        //Assert
        _costRepository.Received(1).AddOrUpdateClientTotalCost(
            Arg.Is<string>(m => m == _request.Mpan),
            Arg.Is<decimal>(u => u == result.Cost)
        );
    }

    /// <summary>
    /// Test to ensure that the current reading is saved to the mock db as the new previous reading once calculations are done
    /// </summary>
    [Test]
    public void PreviousReadingIsSaved()
    {
        //Arrange
        //NA - Handled in the setup

        //Act
        _ = _calculationProvider.CalculateCost(_request);

        //Assert
        _previousReadingRepository.Received(1).AddOrUpdatePreviousReading(
            Arg.Is<string>(m => m == _request.Mpan),
            Arg.Is<decimal>(u => u == _request.CurrentReading)
        );
    }

    /// <summary>
    /// Test to ensure that no error occurs when the previous reading is null 
    /// </summary>
    [Test]
    public void NoErrorWhenNullPreviousReading()
    {
        //Arrange
        _previousReadingRepository.GetPreviousReadingByMpanAndReqType(Arg.Any<string>()).Returns(null as decimal?);

        //Act
        var result = _calculationProvider.CalculateCost(_request);

        //Assert
        _previousReadingRepository.Received(1).AddOrUpdatePreviousReading(
            Arg.Is<string>(m => m == _request.Mpan),
            Arg.Is<decimal>(u => u == _request.CurrentReading)
        );

        Assert.That(string.IsNullOrEmpty(result.ErrorMessage), Is.True);
    }

    /// <summary>
    /// Test to ensure that an error message is returned when the current total reading is less than the previous total reading
    /// </summary>
    [Test]
    public void ErrorWhenCurrentReadingLargerThanPrevious()
    {
        //Arrange
        _request.CurrentReading = 3;
        _previousReadingRepository.GetPreviousReadingByMpanAndReqType(Arg.Any<string>()).Returns(6);

        //Act
        var result = _calculationProvider.CalculateCost(_request);

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(string.IsNullOrEmpty(result.ErrorMessage), Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Current energy reading cannot be less than previous energy reading."));
        });
    }

    /// <summary>
    /// Test to ensure that an error message is returned when the tariff type recieved is not recognised
    /// </summary>
    [Test]
    public void ErrorWhenTariffTypePriceNotFound() {
        //Arrange
        _tariffRepository.GetTariffPriceByType(Arg.Any<TariffType>()).Returns(null as TariffTypeAndPrice);

        //Act
        var result = _calculationProvider.CalculateCost(_request);

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(string.IsNullOrEmpty(result.ErrorMessage), Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo($"Tariff type '{_request.TariffType}' does not exist."));
        });
    }
}
