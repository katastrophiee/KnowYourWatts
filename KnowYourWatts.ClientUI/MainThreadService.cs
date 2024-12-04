using KnowYourWatts.ClientUI.Interfaces;

namespace KnowYourWatts.ClientUI;

public class MainThreadService : IMainThreadService
{
    public void BeginInvokeOnMainThread(Action action)
    {
        Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(action);
    }
}
