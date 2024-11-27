using System.Net;

namespace KnowYourWatts.ClientUI;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();
    }
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var windows = base.CreateWindow(activationState);
        windows.Height = 500;
        windows.Width = 900;
        windows.MaximumHeight = 500;
        windows.MaximumWidth = 900;
        windows.MinimumHeight = 500;
        windows.MinimumWidth = 900;
        return windows;
    }
}
