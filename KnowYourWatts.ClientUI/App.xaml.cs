using System.Net;

namespace KnowYourWatts.ClientUI;

public partial class App : Application
{
    public App(ClientSocket clientSocket)
    {
        InitializeComponent();
        MainPage = new AppShell();
    }
}
