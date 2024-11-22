using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace KnowYourWatts.ClientUI.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args); // Call base to ensure proper initialization

            // Create a new MauiApp instance
            var mauiApp = CreateMauiApp();
            var services = mauiApp.Services;

            // Get the IApplication instance from the services
            var application = services.GetRequiredService<IApplication>();

            // Create a new window for this instance
            var newWindow = application.CreateWindow(null);

            // Ensure the new window is properly activated
            if (newWindow?.Handler?.PlatformView is Microsoft.UI.Xaml.Window window)
            {
                window.Activate();
            }
        }
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }

}
