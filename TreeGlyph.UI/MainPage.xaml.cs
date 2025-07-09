using TreeGlyph.UI.ViewModels;
using TreeGlyph.UI.Views;

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

    private async void OnAboutClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AboutPage));
    }


}
