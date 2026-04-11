using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using System.Drawing;

namespace HnVue.E2ETests.Helpers;

/// <summary>
/// Manages FlaUI application lifecycle for E2E test sessions.
/// Wraps FlaUI Application + UIA3Automation with screenshot support.
/// </summary>
public sealed class AppDriver : IDisposable
{
    private Application? _app;
    private UIA3Automation? _automation;
    private bool _disposed;

    /// <summary>Default path to the release build executable.</summary>
    public static string DefaultExePath => Path.Combine(
        AppContext.BaseDirectory,
        "..", "..", "..", "..", "..", "..",
        "src", "HnVue.App", "bin", "Release",
        "net8.0-windows", "HnVue.App.exe");

    /// <summary>
    /// Launches the HnVue application and returns the main window.
    /// </summary>
    /// <param name="exePath">Executable path override. Uses DefaultExePath if null.</param>
    /// <param name="startupTimeoutMs">Maximum milliseconds to wait for main window.</param>
    /// <returns>Main window automation element.</returns>
    public Window Launch(string? exePath = null, int startupTimeoutMs = 10_000)
    {
        var path = exePath ?? DefaultExePath;

        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                $"HnVue.App executable not found at: {path}. " +
                $"Build with 'dotnet build -c Release' first.", path);
        }

        _automation = new UIA3Automation();
        _app = Application.Launch(path);
        _app.WaitWhileMainHandleIsMissing(TimeSpan.FromMilliseconds(startupTimeoutMs));

        return _app.GetMainWindow(_automation);
    }

    /// <summary>
    /// Captures a screenshot of the main window and saves it to TestReports/screenshots/.
    /// </summary>
    /// <param name="name">Screenshot filename (without extension).</param>
    /// <returns>Full path to saved screenshot, or null if capture failed.</returns>
    public string? CaptureScreenshot(string name)
    {
        try
        {
            var dir = Path.Combine("TestReports", "screenshots");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, $"{name}_{DateTime.Now:yyyyMMdd_HHmmss}.png");

            using var bmp = new Bitmap(
                System.Windows.Forms.Screen.PrimaryScreen?.Bounds.Width ?? 1920,
                System.Windows.Forms.Screen.PrimaryScreen?.Bounds.Height ?? 1080);
            using var g = Graphics.FromImage(bmp);
            g.CopyFromScreen(Point.Empty, Point.Empty, bmp.Size);
            bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);

            return path;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Screenshot capture failed: {ex.Message}");
            return null;
        }
    }

    /// <summary>Kills the launched application if still running.</summary>
    public void Close()
    {
        try { _app?.Kill(); } catch { /* best-effort */ }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Close();
        _automation?.Dispose();
    }
}
