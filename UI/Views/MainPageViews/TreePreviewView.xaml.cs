using TreeGlyph.UI.ViewModels.MainPageViewModel;

namespace TreeGlyph.UI.Views.MainPageViews;

public partial class TreePreviewView : ContentView
{
    public static readonly BindableProperty ViewModelProperty =
        BindableProperty.Create(
            nameof(ViewModel),
            typeof(TreePreviewViewModel),
            typeof(TreePreviewView),
            null,
            propertyChanged: OnViewModelChanged);

    public TreePreviewViewModel ViewModel
    {
        get => (TreePreviewViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public TreePreviewView()
    {
        InitializeComponent();
    }

    private static void OnViewModelChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TreePreviewView view && newValue is TreePreviewViewModel vm)
        {
            view.BindingContext = vm;
        }
    }
}