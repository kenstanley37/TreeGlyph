using TreeGlyph.UI.ViewModels;

namespace TreeGlyph.UI;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;


#if WINDOWS
#endif
    }

}
