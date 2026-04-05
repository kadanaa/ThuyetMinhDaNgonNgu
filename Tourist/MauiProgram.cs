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

            // ----------------------------------------------------------------
            // DbContext — dùng AddTransient thay vì AddScoped
            // Lý do: trong MAUI không có "request scope" như web app,
            // AddScoped = Singleton trong thực tế → DbContext dùng chung
            // toàn app → không thread-safe khi có nhiều query đồng thời.
            // AddTransient = tạo mới mỗi lần inject → an toàn hơn.
            // ----------------------------------------------------------------
            var connectionString = GetConnectionString();
            builder.Services.AddTransient(_ => new ThuyetMinhDbContext(connectionString));

            // ----------------------------------------------------------------
            // Services — AddTransient vì mỗi lần dùng nên có instance riêng
            // ----------------------------------------------------------------
            builder.Services.AddTransient<IPoiService, PoiService>();
            builder.Services.AddTransient<ITranslationService, TranslationService>();
            builder.Services.AddTransient<ITtsService, TtsService>();

            // LocationService — Singleton vì cần giữ tọa độ giả lập xuyên suốt app
            // Nếu dùng Transient thì mỗi lần inject sẽ reset tọa độ về mặc định
            builder.Services.AddSingleton<ILocationService, LocationService>();

            // ----------------------------------------------------------------
            // MainPage — đăng ký để DI tự inject các service vào constructor
            // Không được new MainPage() trực tiếp nữa
            // ----------------------------------------------------------------
            builder.Services.AddTransient<MainPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        private static string GetConnectionString()
        {
#if WINDOWS
            return "Server=localhost;Database=ThuyetMinhDaNgonNgu;" +
                   "Integrated Security=true;TrustServerCertificate=true;Connection Timeout=30;";
#else
            // Android emulator: 10.0.2.2 trỏ đến localhost của máy host
            // Thay bằng IP thật nếu dùng thiết bị thật (ví dụ: 192.168.1.x)
            return "Server=10.0.2.2;Database=ThuyetMinhDaNgonNgu;" +
                   "User Id=sa;Password=sa@123456;" +
                   "TrustServerCertificate=true;Connection Timeout=30;";
#endif
        }
    }
}