using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using Graphics = System.Drawing.Graphics;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Bitmap = System.Drawing.Bitmap;
using Rectangle = System.Drawing.Rectangle;
using ImageFormat = System.Drawing.Imaging.ImageFormat;
using ImageLockMode = System.Drawing.Imaging.ImageLockMode;
using CopyPixelOperation = System.Drawing.CopyPixelOperation;

namespace HnVue.UI.QA.Tests;

/// <summary>
/// Screenshot capture utility for visual regression testing.
/// </summary>
public static class ScreenshotCapture
{
    #region Win32 API

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    #endregion

    /// <summary>
    /// Captures a screenshot of the specified WPF element.
    /// </summary>
    public static Bitmap? CaptureElement(FrameworkElement element)
    {
        if (element == null || element.ActualWidth == 0 || element.ActualHeight == 0)
        {
            return null;
        }

        double width = element.ActualWidth;
        double height = element.ActualHeight;

        var bitmap = new Bitmap((int)width, (int)height, PixelFormat.Format32bppArgb);
        var graphics = Graphics.FromImage(bitmap);

        // Fill with transparent background
        graphics.Clear(Color.Transparent);

        // Render the element to the bitmap
        var renderBitmap = new RenderTargetBitmap(
            (int)width, (int)height, 96, 96, PixelFormats.Pbgra32);

        renderBitmap.Render(element);

        // Convert to System.Drawing.Bitmap
        var bitmapCopy = new Bitmap((int)width, (int)height, PixelFormat.Format32bppArgb);
        var bitmapData = bitmapCopy.LockBits(
            new Rectangle(0, 0, bitmapCopy.Width, bitmapCopy.Height),
            ImageLockMode.WriteOnly,
            PixelFormat.Format32bppArgb);

        var croppedBitmap = new CroppedBitmap(renderBitmap, new Int32Rect(0, 0, (int)width, (int)height));
        croppedBitmap.CopyPixels(
            new Int32Rect(0, 0, (int)width, (int)height),
            bitmapData.Scan0,
            bitmapData.Stride * bitmapData.Height,
            bitmapData.Stride);

        bitmapCopy.UnlockBits(bitmapData);

        graphics.DrawImageUnscaled(bitmapCopy, 0, 0);

        graphics.Dispose();
        bitmapCopy.Dispose();

        return bitmap;
    }

    /// <summary>
    /// Captures the entire primary screen.
    /// </summary>
    public static Bitmap CaptureScreen()
    {
        var bounds = new System.Drawing.Rectangle(
            0, 0,
            (int)SystemParameters.PrimaryScreenWidth,
            (int)SystemParameters.PrimaryScreenHeight);
        var bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);

        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.CopyFromScreen(
                bounds.X, bounds.Y,
                0, 0,
                bounds.Size,
                CopyPixelOperation.SourceCopy);
        }

        return bitmap;
    }

    /// <summary>
    /// Saves a bitmap to the specified path.
    /// </summary>
    public static void SaveBitmap(Bitmap bitmap, string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        bitmap.Save(path, ImageFormat.Png);
    }

    /// <summary>
    /// Loads a bitmap from the specified path.
    /// </summary>
    public static Bitmap? LoadBitmap(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        return new Bitmap(path);
    }

    /// <summary>
    /// Creates a diff image showing differences between two bitmaps.
    /// </summary>
    public static Bitmap CreateDiffImage(Bitmap bitmap1, Bitmap bitmap2)
    {
        int width = Math.Max(bitmap1.Width, bitmap2.Width);
        int height = Math.Max(bitmap1.Height, bitmap2.Height);

        var diffBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixel1 = x < bitmap1.Width && y < bitmap1.Height
                    ? bitmap1.GetPixel(x, y)
                    : Color.Empty;

                Color pixel2 = x < bitmap2.Width && y < bitmap2.Height
                    ? bitmap2.GetPixel(x, y)
                    : Color.Empty;

                if (pixel1 == Color.Empty || pixel2 == Color.Empty)
                {
                    // Missing pixels in red
                    diffBitmap.SetPixel(x, y, Color.FromArgb(255, 255, 0, 0));
                }
                else
                {
                    int diffR = Math.Abs(pixel1.R - pixel2.R);
                    int diffG = Math.Abs(pixel1.G - pixel2.G);
                    int diffB = Math.Abs(pixel1.B - pixel2.B);

                    if (diffR + diffG + diffB > 30)
                    {
                        // Highlight differences in red
                        diffBitmap.SetPixel(x, y, Color.FromArgb(255, 255, 0, 0));
                    }
                    else
                    {
                        // Show original pixel in grayscale
                        int gray = (pixel1.R + pixel1.G + pixel1.B) / 3;
                        diffBitmap.SetPixel(x, y, Color.FromArgb(255, gray, gray, gray));
                    }
                }
            }
        }

        return diffBitmap;
    }
}
