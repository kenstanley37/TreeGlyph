using TreeGlyph.UI.Services;
using TreeGlyph.UI.ViewModels.UtilityViewModels;

namespace TreeGlyph.UI.Views;

public partial class LogViewPage : ContentPage
{
    public LogViewPage()
    {
        InitializeComponent();
        BindingContext = new LogViewModel();
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
}