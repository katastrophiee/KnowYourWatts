using KnowYourWatts.ClientUI.Interfaces;

namespace KnowYourWatts.ClientUI;

public class MainThreadService : IMainThreadService
{
    public void BeginInvokeOnMainThread(Action action)
    {
        MainThread.BeginInvokeOnMainThread(action);
    }
}
