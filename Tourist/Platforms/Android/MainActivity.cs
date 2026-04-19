using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace Tourist
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    [IntentFilter(
        new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "touristapp",
        DataHost = "poi")]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HandleDeepLinkIntent(Intent);
        }

        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);
            HandleDeepLinkIntent(intent);
        }

        private static void HandleDeepLinkIntent(Intent? intent)
        {
            if (intent?.Action != Intent.ActionView)
                return;

            var data = intent.DataString;
            if (string.IsNullOrWhiteSpace(data))
                return;

            App.PendingDeepLinkUri = data;
        }
    }
}
