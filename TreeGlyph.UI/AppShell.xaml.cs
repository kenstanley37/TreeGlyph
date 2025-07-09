using TreeGlyph.UI.Views;

namespace TreeGlyph.UI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(AboutPage), typeof(AboutPage));

    }
}