using Infrastructure.Services;
using UI.ViewModels.MainPageViewModel;

namespace UI.Views.MainPageViews;

public partial class ToolbarView : ContentView
{
    public static readonly BindableProperty ViewModelProperty =
        BindableProperty.Create(
            propertyName: nameof(ViewModel),
            returnType: typeof(ToolbarViewModel),
            declaringType: typeof(ToolbarView),
            defaultValue: null,
            propertyChanged: OnViewModelChanged);

    public ToolbarViewModel ViewModel
    {
        get => (ToolbarViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public ToolbarView()
    {
        InitializeComponent();
        LogService.Write("UI", "🔧 ToolbarView initialized.");
    }

    private static void OnViewModelChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ToolbarView view && newValue is ToolbarViewModel vm)
        {
            view.BindingContext = vm;
            LogService.Write("UI", $"✅ ToolbarViewModel assigned. BindingContext set to: {vm.GetType().Name}");

            if (vm.Parent is not null)
            {
                LogService.Write("UI", $"🔗 ToolbarViewModel.Parent is set: {vm.Parent.GetType().Name}");
                LogService.Write("UI", $"📁 SelectedFolderPath: {vm.SelectedFolderPath}");
                LogService.Write("UI", $"🚫 Exclusions: {vm.Exclusions}");
            }
            else
            {
                LogService.Write("UI", "⚠️ ToolbarViewModel.Parent is null.");
            }
        }
        else
        {
            LogService.Write("UI", "⚠️ ViewModel assignment failed or type mismatch.");
        }
    }
}