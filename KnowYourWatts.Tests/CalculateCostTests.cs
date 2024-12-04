using KnowYourWatts.MockDb.Interfaces;
using KnowYourWatts.MockDb.Repository;
using KnowYourWatts.Server;
using KnowYourWatts.Server.DTO.Enums;
using KnowYourWatts.Server.DTO.Models;
using KnowYourWatts.Server.DTO.Requests;
using NSubstitute;
using NUnit.Framework.Constraints;

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
            Mpan = "1234567890",
            TariffType = TariffType.Fixed,
            CurrentReading = 2,
            CurrentCost = 10,
            BillingPeriod = 1,
            StandingCharge = 1,
            RequestType = RequestType.CurrentUsage
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
    [TestCase(1, 2, 3, 0,  0.26)]
    [TestCase(78, 546, 7, 496,  156.85)]
    public void EnsureFixedCalculationsAreCorrectDailyAndWeekly(
        decimal previousReading,
        decimal currentReading,
        int billingPeriod,
        decimal standingCharge,
        decimal expectedCostResult)
    {
        //Arrange
        _previousReadingRepository.GetPreviousReadingByMpanAndReqType(Arg.Any<string>(), Arg.Is<RequestType>(r => r != RequestType.CurrentUsage)).Returns(previousReading);

        _request.TariffType = TariffType.Fixed;
        _request.CurrentReading = currentReading;
        _request.BillingPeriod = billingPeriod;
        _request.StandingCharge = standingCharge;
        _request.RequestType = RequestType.TodaysUsage;

        //Act
        var result = _calculationProvider.CalculateCost(_request);

        //Assert
        Assert.That(string.IsNullOrEmpty(result.ErrorMessage), Is.True);
        Assert.That(result.Cost, Is.EqualTo(expectedCostResult)
        
       );
    }

    /// <summary>
    /// Test to ensure that cost calculations are correct for flex tariff types
    /// This website is used to determine the expected results: https://www.electricitybillcalculator.com/
    /// </summary>

    [TestCase(10, 20, 1, 45, 3.22)]
    [TestCase(1, 2, 3, 0, 0.28)]
    [TestCase(78, 546, 7, 496, 165.20)]
    public void EnsureFlexCalculationsAreCorrectDailyAndWeekly(
        decimal previousReading,
        decimal currentReading,
        int billingPeriod,
        decimal standingCharge,
        decimal expectedCostResult)
    {
        //Arrange
        _previousReadingRepository.GetPreviousReadingByMpanAndReqType(Arg.Any<string>(), Arg.Is<RequestType>(r => r != RequestType.CurrentUsage)).Returns(previousReading);

        _request.TariffType = TariffType.Flex;
        _request.CurrentReading = currentReading;
        _request.BillingPeriod = billingPeriod;
        _request.StandingCharge = standingCharge;
        _request.RequestType = RequestType.TodaysUsage;

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
    [TestCase(1, 2, 3, 0,  0.28)]
    [TestCase(78, 546, 7, 496, 169.38)]
    public void EnsureGreenCalculationsAreCorrectDailyAndWeekly(
        decimal previousReading,
        decimal currentReading,
        int billingPeriod,
        decimal standingCharge,
        decimal expectedCostResult)
    {
        //Arrange
        _previousReadingRepository.GetPreviousReadingByMpanAndReqType(Arg.Any<string>(), Arg.Is<RequestType>(r => r != RequestType.CurrentUsage)).Returns(previousReading);

        _request.TariffType = TariffType.Green;
        _request.CurrentReading = currentReading;
        _request.BillingPeriod = billingPeriod;
        _request.StandingCharge = standingCharge;
        _request.RequestType = RequestType.TodaysUsage;

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
    [TestCase(78, 546, 7,  496,  152.62)]
    public void EnsureOffPeakCalculationsAreCorrectDailyAndWeekly(
        decimal previousReading,
        decimal currentReading,
        int billingPeriod,
        decimal standingCharge,
        decimal expectedCostResult)
    {
        //Arrange
        _previousReadingRepository.GetPreviousReadingByMpanAndReqType(Arg.Any<string>(), Arg.Is<RequestType>(r => r != RequestType.CurrentUsage)).Returns(previousReading);

        _request.TariffType = TariffType.OffPeak;
        _request.CurrentReading = currentReading;
        _request.BillingPeriod = billingPeriod;
        _request.StandingCharge = standingCharge;
        _request.RequestType = RequestType.TodaysUsage;

        //Act
        var result = _calculationProvider.CalculateCost(_request);

        //Assert
        Assert.That(string.IsNullOrEmpty(result.ErrorMessage), Is.True);
        Assert.That(result.Cost, Is.EqualTo(expectedCostResult));
       
    }

    /// <summary>
    /// Test to ensure that cost calculations are correct for fixed tariff types
    /// This website is used to determine the expected results: https://www.electricitybillcalculator.com/
    /// </summary>
    [TestCase(10,1, 45, 3.05)]
    [TestCase(1, 3, 0, 0.26)]
    [TestCase(468, 7, 496, 156.85)]  
    public void EnsureFixedCalculationsAreCorrect(
        decimal currentReading,
        int billingPeriod,
        decimal standingCharge,
        decimal expectedCostResult)
    {
        //Arrange
        _request.TariffType = TariffType.Fixed;
        _request.CurrentReading = currentReading;
        _request.BillingPeriod = billingPeriod;
        _request.StandingCharge = standingCharge;
        _request.RequestType = RequestType.TodaysUsage;

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
    [TestCase(10, 1, 45, 3.22)]
    [TestCase(1, 3, 0, 0.28)]
    [TestCase(468, 7, 496,165.20)]
    public void EnsureFlexCalculationsAreCorrect(
        decimal currentReading,
        int billingPeriod,
        decimal standingCharge,
        decimal expectedCostResult)
    {
        //Arrange
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
    [TestCase(10, 1, 45, 3.31)]
    [TestCase(1, 3, 0,0.28)]
    [TestCase(468, 7, 496, 169.38)]
    public void EnsureGreenCalculationsAreCorrect(
        decimal currentReading,
        int billingPeriod,
        decimal standingCharge,
        decimal expectedCostResult)
    {
        //Arrange
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
    [TestCase(10, 1, 45, 2.95)]
    [TestCase(1, 3, 0,0.25)]
    [TestCase(468, 7, 496,152.62)]
    public void EnsureOffPeakCalculationsAreCorrect(
        decimal currentReading,
        int billingPeriod,
        decimal standingCharge,
        decimal expectedCostResult)
    {
        //Arrange
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
    [Test]
    public void EnsurePrevReadingNotSavedForCurrentUsage()
    {
        //Arrange
        _request.RequestType = RequestType.CurrentUsage;

        //Act
        _ = _calculationProvider.CalculateCost(_request);

        //Assert
        _previousReadingRepository.DidNotReceive().AddOrUpdatePreviousReading(
            Arg.Any<string>(),
            Arg.Any<decimal>(),
            Arg.Any<RequestType>()
        );
    }
    /// <summary>
    /// Test to ensure that the current reading is saved to the mock db as the new previous reading once calculations are done
    /// </summary>

    [TestCase(RequestType.TodaysUsage)]
    [TestCase(RequestType.WeeklyUsage)]
    public void PreviousReadingIsSaved(RequestType requestType)
    {
        //Arrange
        _request.RequestType = requestType;

        //Act
        _ = _calculationProvider.CalculateCost(_request);

        //Assert
        _previousReadingRepository.Received(1).AddOrUpdatePreviousReading(
            Arg.Is<string>(m => m == _request.Mpan),
            Arg.Is<decimal>(u => u == _request.CurrentReading),
            Arg.Any<RequestType>()
        );
    }

    //NEED TO ADD TESTS TO CHECK CURRENT DOES NOT RUN THESE BITS OF CODE
    //NEED TO REVERT TESTS BACK THAT ARE NOW THE WEEKLY AND DAILY ONES - ALL THAT WAS NEEDED WAS SOME SETUP

    /// <summary>
    /// Test to ensure that no error occurs when the previous reading is null 
    /// </summary>
    [TestCase(RequestType.TodaysUsage)]
    [TestCase(RequestType.WeeklyUsage)]
    public void NoErrorWhenNullPreviousReading(RequestType requestType)
    {
        //Arrange
        _request.RequestType = requestType;

        _previousReadingRepository.GetPreviousReadingByMpanAndReqType(Arg.Any<string>(), Arg.Any<RequestType>()).Returns(null as decimal?);

        //Act
        var result = _calculationProvider.CalculateCost(_request);

        //Assert
        _previousReadingRepository.Received(1).AddOrUpdatePreviousReading(
            Arg.Is<string>(m => m == _request.Mpan),
            Arg.Is<decimal>(u => u == _request.CurrentReading),
            Arg.Any<RequestType>()
        );

        Assert.That(string.IsNullOrEmpty(result.ErrorMessage), Is.True);
    }

    /// <summary>
    /// Test to ensure that an error message is returned when the current total reading is less than the previous total reading
    /// </summary>
    [TestCase(RequestType.TodaysUsage)]
    [TestCase(RequestType.WeeklyUsage)]
    public void ErrorWhenCurrentReadingLargerThanPrevious(RequestType requestType)
    {
        //Arrange
        _request.CurrentReading = 3;
        _request.RequestType = requestType;
        _previousReadingRepository.GetPreviousReadingByMpanAndReqType(Arg.Any<string>(), requestType).Returns(6);

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
    public void ErrorWhenTariffTypePriceNotFound()
    {
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

    /// <summary>
    /// Test to ensure that the current cost is saved to the mock db as the new previous cost once calculations are done
    /// </summary>
    [TestCase(RequestType.TodaysUsage)]
    [TestCase(RequestType.WeeklyUsage)]
    public void PreviousCostIsSaved(RequestType requestType)
    {
        //Arrange
        _request.RequestType = requestType;

        //Act
        var result = _calculationProvider.CalculateCost(_request);

        //Assert
        _costRepository.Received(1).AddOrUpdateClientTotalCost(
            Arg.Is<string>(m => m == _request.Mpan),
            Arg.Is<decimal>(u => u == _request.CurrentCost + result.Cost),
            Arg.Is<RequestType>(r => r == requestType)
        );
    }

    /// <summary>
    /// Test to ensure that no error occurs when the previous cost is null 
    /// </summary>
    [TestCase(RequestType.TodaysUsage)]
    [TestCase(RequestType.WeeklyUsage)]
    public void NoErrorWhenNullPreviousCost(RequestType requestType)
    {
        //Arrange
        _request.RequestType = requestType;

        _costRepository.GetPreviousTotalCostByMpanAndReqType(Arg.Any<string>(), requestType).Returns(null as decimal?);

        //Act
        var result = _calculationProvider.CalculateCost(_request);

        //Assert
        _costRepository.Received(1).AddOrUpdateClientTotalCost(
            Arg.Is<string>(m => m == _request.Mpan),
            Arg.Is<decimal>(u => u == _request.CurrentCost + result.Cost),
            Arg.Any<RequestType>()
        );

        Assert.That(string.IsNullOrEmpty(result.ErrorMessage), Is.True);
    }

    /// <summary>
    /// Test to ensure that an error message is returned when the current total cost is less than the previous total cost
    /// </summary>
    [TestCase(RequestType.TodaysUsage)]
    [TestCase(RequestType.WeeklyUsage)]
    public void ErrorWhenCurrentCostLargerThanPrevious(RequestType requestType)
    {
        //Arrange
        _request.CurrentCost = 3;
        _request.RequestType = requestType;

        _costRepository.GetPreviousTotalCostByMpanAndReqType(Arg.Any<string>(), requestType).Returns(20);

        //Act
        var result = _calculationProvider.CalculateCost(_request);

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(string.IsNullOrEmpty(result.ErrorMessage), Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("The new total cost is less than the previous total cost."));
        });
    }
}
