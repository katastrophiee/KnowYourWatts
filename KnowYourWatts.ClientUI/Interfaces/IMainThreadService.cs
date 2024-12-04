namespace KnowYourWatts.ClientUI.Interfaces;

public interface IMainThreadService
{
    void BeginInvokeOnMainThread(Action action);
}
