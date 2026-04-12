namespace Tourist
{
    public partial class App : Application
    {
        // IServiceProvider được MAUI inject tự động
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            // Lấy MainPage từ DI container
            // DI sẽ tự động inject IPoiService, ITranslationService,
            // ITtsService, ILocationService vào constructor của MainPage
            MainPage = serviceProvider.GetRequiredService<MainPage>();
        }
    }
}