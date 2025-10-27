using System.Collections.ObjectModel;
using System.Globalization;
using TreeGlyph.UI.Services;
using TreeGlyph.UI.ViewModels.UtilityViewModels;

namespace TreeGlyph.UI.Converters;

public class CategorySelectionConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var selected = value as ObservableCollection<string>;
        var category = parameter as string;

        return category != null && selected?.Contains(category) == true;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isChecked = value as bool? ?? false;
        var category = parameter as string;

        var viewModel = Application.Current?.Windows.FirstOrDefault()?.Page?.BindingContext as LogViewModel;

        if (viewModel == null || category == null)
        {
            DebugLogger.WriteLine($"[CategorySelectionConverter] ViewModel or category is null");
            return Binding.DoNothing;
        }

        var selected = viewModel.SelectedCategories;

        if (isChecked)
        {
            if (!selected.Contains(category))
            {
                selected.Add(category);
                DebugLogger.WriteLine($"[CategorySelectionConverter] Added category: {category}");
            }
        }
        else
        {
            if (selected.Contains(category))
            {
                selected.Remove(category);
                DebugLogger.WriteLine($"[CategorySelectionConverter] Removed category: {category}");
            }
        }

        return Binding.DoNothing;
    }
}