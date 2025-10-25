using UI.ViewModels.MainPageViewModel;
using UI.Views;

namespace UI;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;

        // Wire up animation target for the unified sliding container
        vm.OptionsPanel.PanelView = OptionsPanelContainer; // x:Name="OptionsPanelContainer" in XAML

#if WINDOWS
        // Add any platform-specific logic here if needed
#endif
    }

    private async void OnAboutClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AboutPage));
    }
}