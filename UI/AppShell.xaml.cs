using TreeGlyph.UI.Views;
using TreeGlyph.UI.Views.MainPageViews;

namespace TreeGlyph.UI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        Routing.RegisterRoute(nameof(AboutPage), typeof(AboutPage));
        Routing.RegisterRoute(nameof(LogViewPage), typeof(LogViewPage));
    }
}