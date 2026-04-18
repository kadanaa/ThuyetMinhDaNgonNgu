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

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .UseMauiMaps();

            var connectionString = GetConnectionString();
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

        private static string GetConnectionString()
        {
#if WINDOWS
            // Chay tren Windows: dung Windows Authentication, khong can user/pass
            return "Server=localhost;" +
                   "Database=ThuyetMinhDaNgonNgu;" +
                   "Integrated Security=true;" +
                   "TrustServerCertificate=true;" +
                   "Connection Timeout=30;";
#else
            // Chay tren Android Emulator:
            // 10.0.2.2 la dia chi dac biet tro ve localhost cua may tinh
            // Neu dung thiet bi that: thay bang IP WiFi cua may (vd: 192.168.1.5)
            return "Server=10.0.2.2;" + Environment.NewLine +
                   "Database=ThuyetMinhDaNgonNgu;" +
                   "User Id=sa;" +
                   "Password=sa@123456;" +
                   "TrustServerCertificate=true;" +
                   "Connection Timeout=60;" +
                   "MultipleActiveResultSets=true;";
#endif
        }
    }
}