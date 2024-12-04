namespace KnowYourWatts.ClientUI.Interfaces;

public interface IRandomisedValueProvider
{
    string GenerateMpanForClient();

    decimal GenerateRandomReading();

    int GenerateRandomTimeDelay();

    int GenerateRandomTariff();

    decimal GenerateRandomStandingCharge();
}
