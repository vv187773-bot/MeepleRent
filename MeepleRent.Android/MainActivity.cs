using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;

namespace MeepleRent.Android;

[Activity(
    Label = "MeepleRent.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
}
