using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace TreeGlyph.UI.ViewModels.UtilityViewModels;

public class LogDateItem
{
    public DateTime Date { get; }

    public ICommand LoadKeysCommand { get; }

    public LogDateItem(DateTime date, Action<DateTime> loadKeysAction)
    {
        Date = date;
        LoadKeysCommand = new RelayCommand(() => loadKeysAction(Date));
    }
}
