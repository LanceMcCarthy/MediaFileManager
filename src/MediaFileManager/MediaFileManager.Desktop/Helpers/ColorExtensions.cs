namespace MediaFileManager.Desktop.Helpers;

public static class ColorExtensions
{
    public static System.Windows.Media.Color ToWindowsMediaColor(this System.Drawing.Color color)
    {
        return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
    }

    public static System.Drawing.Color ToSystemDrawingColor(this System.Windows.Media.Color color)
    {
        return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
    }
}