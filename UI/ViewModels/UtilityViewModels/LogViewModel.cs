using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Infrastructure.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace TreeGlyph.UI.ViewModels.UtilityViewModels;

public partial class LogViewModel : ObservableObject
{
    public ICommand LoadKeysCommand { get; }
    public ICommand LoadLogsCommand { get; }

    private DateTime selectedDate = DateTime.Today;
    public DateTime SelectedDate
    {
        get => selectedDate;
        set => SetProperty(ref selectedDate, value);
    }

    public LogViewModel()
    {
        LoadLogsCommand = new AsyncRelayCommand(LoadLogsAsync);
        LoadKeysCommand = new RelayCommand<DateTime>(LoadKeysForDate);
        LoadAvailableDates();

        if (AvailableLogDates.Count > 0)
        {
            var today = DateTime.Today;
            var match = AvailableLogDates.FirstOrDefault(d => d.Date.Date == today)
                     ?? AvailableLogDates.OrderByDescending(d => d.Date).First();

            SelectedLogDate = match;
            LoadKeysForDate(match.Date);
        }
    }

    private LogDateItem? selectedLogDate;
    public LogDateItem? SelectedLogDate
    {
        get => selectedLogDate;
        set
        {
            if (SetProperty(ref selectedLogDate, value) && value != null)
            {
                SelectedDate = value.Date;
                LoadKeysForDate(value.Date);
            }
        }
    }

    private bool isBusy;
    public bool IsBusy
    {
        get => isBusy;
        set => SetProperty(ref isBusy, value);
    }


    private string searchText = string.Empty;
    public string SearchText
    {
        get => searchText;
        set => SetProperty(ref searchText, value);
    }

    private string _fullLogText = string.Empty;
    public string FullLogText
    {
        get => _fullLogText;
        set => SetProperty(ref _fullLogText, value);
    }
    //public ObservableCollection<string> AvailableKeys { get; } = new();
    public bool CanLoadLogs => SelectedCategories?.Count > 0;

    private ObservableCollection<string> selectedCategories = new();
    public ObservableCollection<string> SelectedCategories
    {
        get => selectedCategories;
        set
        {
            if (SetProperty(ref selectedCategories, value))
            {
                OnPropertyChanged(nameof(CanLoadLogs));
            }
        }
    }

    public ObservableCollection<string> AvailableCategories { get; } = new()
    {
        "TREEGEN", "EXCLUSION", "PATH", "UI", "SYNC", "ERROR"
    };

    private ObservableCollection<string> filteredLogs = new();
    public ObservableCollection<string> FilteredLogs
    {
        get => filteredLogs;
        set => SetProperty(ref filteredLogs, value);
    }

    public ObservableCollection<SelectableKeyItem> AvailableKeys { get; } = new();
    public ObservableCollection<LogDateItem> AvailableLogDates { get; } = new();

    public void LoadAvailableDates()
    {
        AvailableLogDates.Clear();

        var files = Directory.GetFiles(LogService.LogDirectory, "exclusion-log-*.txt");

        var parsedDates = files
            .Select(file => Path.GetFileNameWithoutExtension(file))
            .Select(name => name.Replace("exclusion-log-", ""))
            .Select(raw => DateTime.TryParseExact(raw, "yyyy-MM-dd", null, DateTimeStyles.None, out var date) ? date : (DateTime?)null)
            .Where(d => d.HasValue)
            .Select(d => new LogDateItem((DateTime)d!, LoadKeysForDate))
            .OrderByDescending(d => d.Date)
            .ToList();

        foreach (var item in parsedDates)
            AvailableLogDates.Add(item);

        // ✅ Set default selection to today or most recent
        var today = DateTime.Today;
        var match = parsedDates.FirstOrDefault(d => d.Date.Date == today)
                 ?? parsedDates.FirstOrDefault();

        if (match != null)
        {
            SelectedDate = match.Date;
            LoadKeysForDate(match.Date);
        }
    }

    private bool isAllKeysSelected;
    public bool IsAllKeysSelected
    {
        get => isAllKeysSelected;
        set
        {
            if (SetProperty(ref isAllKeysSelected, value))
            {
                foreach (var key in AvailableKeys)
                    key.IsSelected = value;

                OnPropertyChanged(nameof(CanLoadLogs));
            }
        }
    }

    public ICommand SelectAllKeysCommand => new RelayCommand(SelectAllKeys);

    private void SelectAllKeys()
    {
        foreach (var keyItem in AvailableKeys)
        {
            keyItem.IsSelected = true;
        }
    }

    private void LoadKeysForDate(DateTime date)
    {
        SelectedDate = date;
        AvailableKeys.Clear();
        SelectedCategories.Clear();

        var fileName = $"exclusion-log-{date:yyyy-MM-dd}.txt";
        var filePath = Path.Combine(LogService.LogDirectory, fileName);

        if (!File.Exists(filePath))
            return;

        var lines = File.ReadAllLines(filePath);

        var keys = lines
            .SelectMany(line => Regex.Matches(line, @"\[(.*?)\]").Cast<Match>().Select(m => m.Groups[1].Value))
            .Where(key => !Regex.IsMatch(key, @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}$")) // exclude timestamps
            .Distinct()
            .OrderBy(k => k);

        foreach (var key in keys)
        {
            var item = new SelectableKeyItem(key);
            item.PropertyChanged += OnKeySelectionChanged;
            AvailableKeys.Add(item);
        }

        OnPropertyChanged(nameof(CanLoadLogs));
    }

    private void OnKeySelectionChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectableKeyItem.IsSelected) &&
            sender is SelectableKeyItem item)
        {
            if (item.IsSelected)
            {
                if (!SelectedCategories.Contains(item.Key))
                    SelectedCategories.Add(item.Key);
            }
            else
            {
                if (SelectedCategories.Contains(item.Key))
                    SelectedCategories.Remove(item.Key);
            }

            OnPropertyChanged(nameof(CanLoadLogs));
        }
    }

    private async Task ShowSpinnerWithDelay(Func<Task> action)
    {
        var delayTask = Task.Delay(300); // ⏳ delay before showing spinner
        IsBusy = true;

        var actionTask = action();

        await Task.WhenAny(actionTask, delayTask); // wait for either to finish
        await actionTask;

        IsBusy = false;
    }

    public async Task LoadLogsAsync()
    {
        // ⏳ Clear viewer immediately for visual feedback
        FullLogText = string.Empty;
        FilteredLogs.Clear();

        await ShowSpinnerWithDelay(async () =>
        {
            //await Task.Delay(10000); // delay for testing spinner

            var fileName = $"exclusion-log-{SelectedDate:yyyy-MM-dd}.txt";
            var filePath = Path.Combine(LogService.LogDirectory, fileName);

            List<string> filtered;

            if (!File.Exists(filePath))
            {
                filtered = new() { "⚠️ No logs found for selected date." };
            }
            else
            {
                var lines = await File.ReadAllLinesAsync(filePath);
                filtered = lines
                    .Where(line =>
                        SelectedCategories?.Count == 0 ||
                        (SelectedCategories?.Any(cat => line.Contains(cat, StringComparison.OrdinalIgnoreCase)) ?? false))
                    .Where(line =>
                        string.IsNullOrWhiteSpace(SearchText) ||
                        line.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            FilteredLogs = new ObservableCollection<string>(filtered);
            FullLogText = string.Join(Environment.NewLine, filtered);
        });
    }

    public ICommand ToggleCategoryCommand => new RelayCommand<string>(ToggleCategory);

    private void ToggleCategory(string? category)
    {
        if (string.IsNullOrWhiteSpace(category)) return;

        if (SelectedCategories.Contains(category))
            SelectedCategories.Remove(category);
        else
            SelectedCategories.Add(category);

        OnPropertyChanged(nameof(CanLoadLogs)); // ✅ Notify UI to re-evaluate button state
    }
}