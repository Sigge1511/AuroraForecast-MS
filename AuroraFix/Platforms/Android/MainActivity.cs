using Android.App;
using Android.Content.PM;
using Android.OS;

namespace AuroraFix
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetAnimatedGifWindowBackground();
        }

        private void SetAnimatedGifWindowBackground()
        {
            // AnimatedImageDrawable requires API 28+. Below that we gracefully skip —
            // the ContentPage background colour provides a plain dark fallback.
            if (!OperatingSystem.IsAndroidVersionAtLeast(28) || Assets is null)
                return;

            try
            {
                // Read the GIF from Android assets (MauiAsset in Resources/Raw/)
                using var memStream = new System.IO.MemoryStream();
                using var assetStream = Assets.Open("giphy.gif");
                assetStream.CopyTo(memStream);

                var byteBuffer = Java.Nio.ByteBuffer.Wrap(memStream.ToArray());
                var source = Android.Graphics.ImageDecoder.CreateSource(byteBuffer);
                var drawable = Android.Graphics.ImageDecoder.DecodeDrawable(source)
                    as Android.Graphics.Drawables.AnimatedImageDrawable;

                if (drawable is null) return;

                // Match the XAML Opacity="0.5" on the original Image control
                drawable.Alpha = 127;
                drawable.Start();

                // Render at the Window decor-view level — completely below the MAUI
                // view hierarchy. Animation frames call invalidate() only on the window
                // background, never touching MAUI layout → zero layout invalidation → no shake.
                Window?.SetBackgroundDrawable(drawable);
            }
            catch
            {
                // Graceful fallback — leave the default window background
            }
        }
    }
}
