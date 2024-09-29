using System.Threading.Tasks;

namespace MauiGameOfLife.Interfaces
{
    public interface IScreenshotService
    {
        /// <summary>
        /// Captures a screenshot of the current screen and saves it.
        /// </summary>
        /// <returns>The file path of the saved screenshot.</returns>
        Task<string> CaptureScreenshotAsync();
    }
}