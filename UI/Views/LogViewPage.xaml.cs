using System.ComponentModel;
using TreeGlyph.UI.Services;
using TreeGlyph.UI.ViewModels.UtilityViewModels;

namespace TreeGlyph.UI.Views;

public partial class LogViewPage : ContentPage
{
    private LogViewModel viewModel;

    public LogViewPage()
    {
        InitializeComponent();
        viewModel = new LogViewModel();
        BindingContext = viewModel;
    }

    private void OnCategoryCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox checkbox && checkbox.BindingContext is SelectableKeyItem keyItem)
        {
            if (BindingContext is not LogViewModel vm || string.IsNullOrWhiteSpace(keyItem.Key))
                return;

            if (e.Value)
            {
                if (!vm.SelectedCategories.Contains(keyItem.Key))
                    vm.SelectedCategories.Add(keyItem.Key);
            }
            else
            {
                if (vm.SelectedCategories.Contains(keyItem.Key))
                    vm.SelectedCategories.Remove(keyItem.Key);
            }

            DebugLogger.WriteLine($"[CheckedChanged] SelectedCategories = {string.Join(", ", vm.SelectedCategories)}");
        }
    }

    private void OnSelectAllCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (BindingContext is LogViewModel vm)
        {
            vm.IsAllKeysSelected = e.Value;
            DebugLogger.WriteLine($"[SelectAll] Toggled to: {e.Value}");
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(viewModel.FullLogText))
        {
            LogEditor.Text = viewModel.FullLogText;
            LogEditor.CursorPosition = viewModel.FullLogText.Length;
        }
    }


}