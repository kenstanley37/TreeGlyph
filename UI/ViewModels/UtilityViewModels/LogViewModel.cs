using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Infrastructure.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace UI.ViewModels.UtilityViewModels;

public partial class LogViewModel : ObservableObject
{
    public ICommand LoadKeysCommand { get; }
    public ICommand LoadLogsCommand { get; }

    public LogViewModel()
    {
        LoadLogsCommand = new AsyncRelayCommand(LoadLogsAsync);
        LoadKeysCommand = new RelayCommand<DateTime>(LoadKeysForDate);
        LoadAvailableDates();

    }

    private DateTime selectedDate = DateTime.Today;
    public DateTime SelectedDate
    {
        get => selectedDate;
        set => SetProperty(ref selectedDate, value);
    }

    private string searchText = string.Empty;
    public string SearchText
    {
        get => searchText;
        set => SetProperty(ref searchText, value);
    }

    //public ObservableCollection<string> AvailableKeys { get; } = new();

    private ObservableCollection<string> selectedCategories = new();
    public ObservableCollection<string> SelectedCategories
    {
        get => selectedCategories;
        set => SetProperty(ref selectedCategories, value);
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

        foreach (var file in files)
        {
            var name = Path.GetFileNameWithoutExtension(file);
            if (DateTime.TryParseExact(name.Replace("exclusion-log-", ""), "yyyy-MM-dd", null, DateTimeStyles.None, out var date))
            {
                AvailableLogDates.Add(new LogDateItem(date, LoadKeysForDate));
            }
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
            AvailableKeys.Add(new SelectableKeyItem(key));
    }

    public async Task LoadLogsAsync()
    {
        var fileName = $"exclusion-log-{SelectedDate:yyyy-MM-dd}.txt";
        var filePath = Path.Combine(LogService.LogDirectory, fileName);

        List<string> filtered;

        if (!File.Exists(filePath))
        {
            filtered = new() { "⚠️ No logs found for selected date." };
        }
        else
        {
            //var lines = File.ReadAllLines(filePath);
            var lines = await File.ReadAllLinesAsync(filePath);
            System.Diagnostics.Debug.WriteLine($"[LoadLogs] Total lines: {lines.Length}");

            filtered = lines
                .Where(line =>
                    SelectedCategories?.Count == 0 ||
                    (SelectedCategories?.Any(cat => line.Contains(cat, StringComparison.OrdinalIgnoreCase)) ?? false))
                .Where(line =>
                    string.IsNullOrWhiteSpace(SearchText) ||
                    line.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var line in lines)
            {
                foreach (var cat in SelectedCategories)
                {
                    if (line.Contains(cat, StringComparison.OrdinalIgnoreCase))
                    {
                        //DebugLogger.WriteLine($"[Match] Line matched category '{cat}': {line}");
                    }

                }
            }


            //DebugLogger.WriteLine($"[LoadLogs] Categories: {(SelectedCategories == null ? "null" : string.Join(", ", SelectedCategories))}");
            //DebugLogger.WriteLine($"[LoadLogs] SearchText: {SearchText}");
            //DebugLogger.WriteLine($"[LoadLogs] Filtered count: {filtered.Count}");
        }

        FilteredLogs = new ObservableCollection<string>(filtered);
        OnPropertyChanged(nameof(FilteredLogs));
    }

    public ICommand ToggleCategoryCommand => new RelayCommand<string>(ToggleCategory);

    private void ToggleCategory(string? category)
    {
        if (string.IsNullOrWhiteSpace(category)) return;

        if (SelectedCategories.Contains(category))
            SelectedCategories.Remove(category);
        else
            SelectedCategories.Add(category);

        //DebugLogger.WriteLine($"[ToggleCategory] SelectedCategories = {string.Join(", ", SelectedCategories)}");
    }
}