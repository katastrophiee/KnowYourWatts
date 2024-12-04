using KnowYourWatts.ClientUI.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net;

namespace KnowYourWatts.ClientUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<MainPage>();

        BuildClientDependencies(ref builder);

        #if DEBUG
            builder.Logging.AddDebug();
        #endif

        return builder.Build();
    }

    private static void BuildClientDependencies(ref MauiAppBuilder builder)
    {
        builder.Services.AddScoped<IRandomisedValueProvider, RandomisedValueProvider>();
        builder.Services.AddScoped<IServerRequestHandler, ServerRequestHandler>();
        builder.Services.AddScoped<IEncryptionHelper, EncryptionHelper>();
        builder.Services.AddScoped<IMainThreadService, MainThreadService>();
        builder.Services.AddScoped<ClientSocket>();
    }
}
