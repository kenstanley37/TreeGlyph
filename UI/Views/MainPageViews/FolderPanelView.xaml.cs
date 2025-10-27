using Infrastructure.Services;
using TreeGlyph.UI.ViewModels.MainPageViewModel;

namespace TreeGlyph.UI.Views.MainPageViews;

public partial class FolderPanelView : ContentView
{
    public static readonly BindableProperty ViewModelProperty =
        BindableProperty.Create(
            propertyName: nameof(ViewModel),
            returnType: typeof(FolderPanelViewModel),
            declaringType: typeof(FolderPanelView),
            defaultValue: null,
            propertyChanged: OnViewModelChanged);

    public FolderPanelViewModel ViewModel
    {
        get => (FolderPanelViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public FolderPanelView()
    {
        InitializeComponent();
        LogService.Write("UI", "📂 FolderPanelView initialized.");
    }

    private static void OnViewModelChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is FolderPanelView view && newValue is FolderPanelViewModel vm)
        {
            view.BindingContext = vm;
            LogService.Write("UI", $"✅ FolderPanelViewModel assigned. BindingContext set to: {vm.GetType().Name}");

            if (vm.Parent is not null)
            {
                LogService.Write("UI", $"🔗 FolderPanelViewModel.Parent is set: {vm.Parent.GetType().Name}");
                LogService.Write("UI", $"📁 SelectedFolderPath: {vm.SelectedFolderPath}");
                LogService.Write("UI", $"🚫 Exclusions: {vm.Exclusions}");
            }
            else
            {
                LogService.Write("UI", "⚠️ FolderPanelViewModel.Parent is null.");
            }
        }
        else
        {
            LogService.Write("UI", "⚠️ ViewModel assignment failed or type mismatch.");
        }
    }
}