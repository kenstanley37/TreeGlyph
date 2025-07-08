using System.Runtime.InteropServices;

namespace TreeGlyph.Platforms.Windows;

public static class CursorHelper
{
    [DllImport("user32.dll")]
    private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

    [DllImport("user32.dll")]
    private static extern IntPtr SetCursor(IntPtr hCursor);

    private const int IDC_SIZEWE = 32644;
    private const int IDC_ARROW = 32512;

    public static void ApplyResizeCursor()
    {
        var hCursor = LoadCursor(IntPtr.Zero, IDC_SIZEWE);
        SetCursor(hCursor);
    }

    public static void ApplyDefaultCursor()
    {
        var hCursor = LoadCursor(IntPtr.Zero, IDC_ARROW);
        SetCursor(hCursor);
    }
}