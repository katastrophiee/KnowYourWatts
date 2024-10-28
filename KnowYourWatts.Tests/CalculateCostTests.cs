using KnowYourWatts.Server;
using KnowYourWatts.Server.DTO.Enums;
using KnowYourWatts.Server.DTO.Requests;

namespace KnowYourWatts.Tests;

internal sealed class CalculateCostTests
{
    private CalculationProvider _calculationProvider = null!;
    private SmartMeterCalculationRequest _request = null!;

    [SetUp]
    public void Setup()
    {
        _calculationProvider = new();

        _request = new()
        {
            TariffType = TariffType.Fixed,
            CurrentReading = 2,
            PreviousReading = 1,
            BillingPeriod = 1,
            ExistingCharge = 1
        };
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
        decimal existingCharge,
        decimal expectedCostResult)
    {
        //Arrange
        _request.TariffType = TariffType.Fixed;

        _request.PreviousReading = previousReading;
        _request.CurrentReading = currentReading;
        _request.BillingPeriod = billingPeriod;
        _request.ExistingCharge = existingCharge;

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
    [TestCase(10, 20, 1, 45, 3.05)]
    [TestCase(1, 2, 3, 0, 0.26)]
    [TestCase(78, 546, 7, 496, 156.85)]
    public void EnsureFlexCalculationsAreCorrect(
        decimal previousReading,
        decimal currentReading,
        int billingPeriod,
        decimal existingCharge,
        decimal expectedCostResult)
    {
        //Arrange
        _request.TariffType = TariffType.Flex;

        _request.PreviousReading = previousReading;
        _request.CurrentReading = currentReading;
        _request.BillingPeriod = billingPeriod;
        _request.ExistingCharge = existingCharge;

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
    [TestCase(10, 20, 1, 45, 3.05)]
    [TestCase(1, 2, 3, 0, 0.26)]
    [TestCase(78, 546, 7, 496, 156.85)]
    public void EnsureGreenCalculationsAreCorrect(
        decimal previousReading,
        decimal currentReading,
        int billingPeriod,
        decimal existingCharge,
        decimal expectedCostResult)
    {
        //Arrange
        _request.TariffType = TariffType.Green;

        _request.PreviousReading = previousReading;
        _request.CurrentReading = currentReading;
        _request.BillingPeriod = billingPeriod;
        _request.ExistingCharge = existingCharge;

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
    [TestCase(10, 20, 1, 45, 3.05)]
    [TestCase(1, 2, 3, 0, 0.26)]
    [TestCase(78, 546, 7, 496, 156.85)]
    public void EnsureOffPeakCalculationsAreCorrect(
        decimal previousReading,
        decimal currentReading,
        int billingPeriod,
        decimal existingCharge,
        decimal expectedCostResult)
    {
        //Arrange
        _request.TariffType = TariffType.OffPeak;

        _request.PreviousReading = previousReading;
        _request.CurrentReading = currentReading;
        _request.BillingPeriod = billingPeriod;
        _request.ExistingCharge = existingCharge;

        //Act
        var result = _calculationProvider.CalculateCost(_request);

        //Assert
        Assert.That(string.IsNullOrEmpty(result.ErrorMessage), Is.True);
        Assert.That(result.Cost, Is.EqualTo(expectedCostResult));
    }

    /// <summary>
    /// Test to ensure that an error message is returned when the current total reading is less than the previous total reading
    /// </summary>
    [Test]
    public void ErrorWhenCurrentReadingLargerThanPrevious()
    {
        //Arrange
        _request.CurrentReading = 3;
        _request.PreviousReading = 6;

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
        _request.TariffType = (TariffType)56;

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
