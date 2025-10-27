using TreeGlyph.UI.ViewModels.MainPageViewModel;

namespace TreeGlyph.UI.Views.MainPageViews;

public partial class OptionsDrawerView : ContentView
{
    public static readonly BindableProperty ViewModelProperty =
        BindableProperty.Create(
            nameof(ViewModel),
            typeof(OptionsPanelViewModel),
            typeof(OptionsDrawerView),
            null,
            propertyChanged: OnViewModelChanged);

    public OptionsPanelViewModel ViewModel
    {
        get => (OptionsPanelViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public OptionsDrawerView()
    {
        InitializeComponent();
    }

    private static void OnViewModelChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is OptionsDrawerView view && newValue is OptionsPanelViewModel vm)
        {
            view.BindingContext = vm;
        }
    }
}