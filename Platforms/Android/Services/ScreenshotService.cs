// Platforms/Android/Services/ScreenshotService.cs
using Android.Content;
using Android.Graphics;
using Android.Provider;
using Android.Views;
using Microsoft.Maui.ApplicationModel;
using System;
using System.IO;
using System.Threading.Tasks;

// Ensure the assembly attribute points to the correct namespace and class
[assembly: Microsoft.Maui.Controls.Dependency(typeof(MauiGameOfLife.Platforms.Android.Services.ScreenshotService))]

namespace MauiGameOfLife.Platforms.Android.Services
{
    public class ScreenshotService : Interfaces.IScreenshotService
    {
        public async Task<string> CaptureScreenshotAsync()
        {
            try
            {
                // Get the current activity
                var activity = Platform.CurrentActivity;
                if (activity == null)
                    throw new Exception("Current activity is null.");

                // Get the root view
                var rootView = activity.Window.DecorView.RootView;

                // Create the bitmap
                Bitmap bitmap = Bitmap.CreateBitmap(rootView.Width, rootView.Height, Bitmap.Config.Argb8888);
                Canvas canvas = new Canvas(bitmap);
                rootView.Draw(canvas);

                // Define the filename
                string filename = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";

                // Save the bitmap to MediaStore (Gallery)
                ContentValues values = new ContentValues();
                values.Put(MediaStore.Images.Media.InterfaceConsts.DisplayName, filename);
                values.Put(MediaStore.Images.Media.InterfaceConsts.MimeType, "image/png");
                values.Put(MediaStore.Images.Media.InterfaceConsts.RelativePath, "Pictures/MauiGameOfLife");

                ContentResolver resolver = activity.ContentResolver;
                var uri = resolver.Insert(MediaStore.Images.Media.ExternalContentUri, values);

                if (uri == null)
                    throw new Exception("Failed to create new MediaStore record.");

                using (Stream stream = resolver.OpenOutputStream(uri))
                {
                    await bitmap.CompressAsync(Bitmap.CompressFormat.Png, 100, stream);
                }

                return uri.ToString();
            }
            catch (Exception ex)
            {
                // Handle exceptions as needed
                Console.WriteLine($"Screenshot capture failed: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
