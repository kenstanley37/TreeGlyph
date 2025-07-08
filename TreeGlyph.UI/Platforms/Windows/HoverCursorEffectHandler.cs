using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml.Input;

[assembly: ExportEffect(typeof(TreeGlyph.WinUI.Effects.HoverCursorEffectHandler), "HoverCursorEffect")]
namespace TreeGlyph.WinUI.Effects;

public class HoverCursorEffectHandler : PlatformEffect
{
    private Microsoft.UI.Xaml.FrameworkElement? _element;

    protected override void OnAttached()
    {
        _element = Control as Microsoft.UI.Xaml.FrameworkElement ?? Container as Microsoft.UI.Xaml.FrameworkElement;

        if (_element is not null)
        {
            _element.PointerEntered += OnPointerEntered;
            _element.PointerExited += OnPointerExited;
        }
    }

    protected override void OnDetached()
    {
        if (_element is not null)
        {
            _element.PointerEntered -= OnPointerEntered;
            _element.PointerExited -= OnPointerExited;
        }
    }

    private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        var window = Microsoft.UI.Xaml.Window.Current;
        window.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(
            Windows.UI.Core.CoreCursorType.SizeWestEast, 1);
    }

    private void OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        var window = Microsoft.UI.Xaml.Window.Current;
        window.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(
            Windows.UI.Core.CoreCursorType.Arrow, 1);
    }
}