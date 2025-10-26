using UI.Services;
using UI.ViewModels.UtilityViewModels;

namespace UI.Views;

public partial class LogViewPage : ContentPage
{
    public LogViewPage()
    {
        InitializeComponent();
        BindingContext = new LogViewModel();
    }

    private void OnCategoryCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox checkbox && checkbox.BindingContext is string category)
        {
            var vm = BindingContext as LogViewModel;
            if (vm == null || string.IsNullOrWhiteSpace(category)) return;

            if (e.Value)
            {
                if (!vm.SelectedCategories.Contains(category))
                    vm.SelectedCategories.Add(category);
            }
            else
            {
                if (vm.SelectedCategories.Contains(category))
                    vm.SelectedCategories.Remove(category);
            }

            DebugLogger.WriteLine($"[CheckedChanged] SelectedCategories = {string.Join(", ", vm.SelectedCategories)}");
        }
    }
}