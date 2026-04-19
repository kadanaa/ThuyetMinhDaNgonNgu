using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Storage;
using Tourist.Data;
using Tourist.Services;

namespace Tourist
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            using (var stream = FileSystem.OpenAppPackageFileAsync("appsettings.json").GetAwaiter().GetResult())
            {
                builder.Configuration.AddJsonStream(stream);
            }

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .UseMauiMaps();

            var connectionString = GetConnectionString(builder.Configuration);
            builder.Services.AddTransient(_ => new ThuyetMinhDbContext(connectionString));

            builder.Services.AddTransient<IPoiService, PoiService>();
            builder.Services.AddTransient<ITranslationService, TranslationService>();
            builder.Services.AddTransient<ITtsService, TtsService>();
            builder.Services.AddTransient<ITouristIdentityService, TouristIdentityService>();
            builder.Services.AddSingleton<ILocationService, LocationService>();
            builder.Services.AddTransient<MainPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        private static string GetConnectionString(IConfiguration configuration)
        {
#if WINDOWS
            return configuration.GetConnectionString("ThuyetMinhDbWindows")
                ?? throw new InvalidOperationException("Missing connection string 'ConnectionStrings:ThuyetMinhDbWindows' in Tourist appsettings.json.");
#else
            return configuration.GetConnectionString("ThuyetMinhDbMobile")
                ?? throw new InvalidOperationException("Missing connection string 'ConnectionStrings:ThuyetMinhDbMobile' in Tourist appsettings.json.");
#endif
        }
    }
}