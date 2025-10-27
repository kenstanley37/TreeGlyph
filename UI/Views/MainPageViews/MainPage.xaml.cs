using TreeGlyph.UI.ViewModels.MainPageViewModel;

namespace TreeGlyph.UI.Views.MainPageViews;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel vm;

    public MainPage(MainViewModel vm)
    {
        InitializeComponent(); // ✅ This wires up the XAML
        BindingContext = this.vm = vm;

        Shell.SetNavBarIsVisible(this, false);
        //vm.OptionsPanel.PanelView = OptionsDrawer.PanelContainer;
    }

    /* Async initialization pattern for MAUI Pages
     * Reference: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/pages?page=async-initialization
     */
    /* Async void is acceptable here because OnAppearing is an event-like method called by the framework.
     * It is important to ensure that any exceptions thrown in async void methods are properly handled.
     */
    /*
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await vm.InitializeAsync(); // ✅ Safe to await here
    }
    */

    private async void OnAboutClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AboutPage));
    }
}