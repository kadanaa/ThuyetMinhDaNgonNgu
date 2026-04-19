using Microsoft.Extensions.Configuration;

namespace POIOwner
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("ThuyetMinhDb")
                ?? throw new InvalidOperationException("Missing connection string 'ConnectionStrings:ThuyetMinhDb' in appsettings.json.");

            var touristApkUrl = configuration["AppLinks:TouristApkUrl"]
                ?? throw new InvalidOperationException("Missing app link 'AppLinks:TouristApkUrl' in appsettings.json.");

            var touristQrBridgeUrl = configuration["AppLinks:TouristQrBridgeUrl"];

            Application.Run(new Form1(connectionString, touristApkUrl, touristQrBridgeUrl));
        }
    }
}