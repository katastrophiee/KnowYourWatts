namespace KnowYourWatts.ClientUI;

public interface IRandomisedValueProvider
{
    string GenerateMpanForClient();

    decimal GenerateRandomReading();

    int GenerateRandomTimeDelay();
    int GenerateRandomTarrif();
    int GenerateRandomBillingPeriod();
    decimal GenerateRandomStandingCharge();
}
