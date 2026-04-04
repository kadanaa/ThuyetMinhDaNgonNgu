using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Maps;
using Tourist.Data;
using Tourist.Services;

namespace Tourist
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            // Load configuration
            var connectionString = GetConnectionString();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .UseMauiMaps(); // Add Maps support

            // Register Services
            builder.Services.AddScoped(_ => new ThuyetMinhDbContext(connectionString));
            builder.Services.AddScoped<IPoiService, PoiService>();
            builder.Services.AddScoped<ITranslationService, TranslationService>();
            builder.Services.AddScoped<ITtsService, TtsService>();
            builder.Services.AddScoped<ILocationService, LocationService>();
            builder.Services.AddScoped<MainPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        private static string GetConnectionString()
        {
#if WINDOWS
            // Windows: localhost works
            return "Server=localhost;Database=ThuyetMinhDaNgonNgu;Integrated Security=true;TrustServerCertificate=true;Connection Timeout=30;";
#else
            // Android/iOS: Use 10.0.2.2 to reach host machine from emulator
            // Or replace with your actual server IP/hostname
            return "Server=10.0.2.2;Database=ThuyetMinhDaNgonNgu;User Id=sa;Password=sa@123456;TrustServerCertificate=true;Connection Timeout=30;";
#endif
        }
    }
}
