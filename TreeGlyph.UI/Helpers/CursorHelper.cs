using System.Runtime.InteropServices;

namespace TreeGlyph.UI.Helpers;

public static class CursorHelper
{
    [DllImport("user32.dll")]
    private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

    [DllImport("user32.dll")]
    private static extern IntPtr SetCursor(IntPtr hCursor);

    private const int IDC_SIZEWE = 32644;
    private const int IDC_ARROW = 32512;

    public static void SetResizeCursor()
    {
        var cursor = LoadCursor(IntPtr.Zero, IDC_SIZEWE);
        SetCursor(cursor);
    }

    public static void SetDefaultCursor()
    {
        var cursor = LoadCursor(IntPtr.Zero, IDC_ARROW);
        SetCursor(cursor);
    }
}