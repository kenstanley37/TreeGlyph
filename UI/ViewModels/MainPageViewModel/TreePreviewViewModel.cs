using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;

namespace TreeGlyph.UI.ViewModels.MainPageViewModel;

public partial class TreePreviewViewModel : ObservableObject
{
    private MainViewModel? parent;
    public MainViewModel? Parent
    {
        get => parent;
        set
        {
            if (SetProperty(ref parent, value))
            {
                if (parent is not null)
                {
                    parent.PropertyChanged += (_, args) =>
                    {
                        switch (args.PropertyName)
                        {
                            case nameof(MainViewModel.ElapsedTime):
                                OnPropertyChanged(nameof(ElapsedTime));
                                break;
                            case nameof(MainViewModel.MemoryUsage):
                                OnPropertyChanged(nameof(MemoryUsage));
                                break;
                            case nameof(MainViewModel.HasSelectedFolder):
                                OnPropertyChanged(nameof(HasSelectedFolder));
                                break;
                            case nameof(MainViewModel.TreeOutput):
                                OnPropertyChanged(nameof(TreeOutput));
                                break;
                            case nameof(MainViewModel.SkippedPathsLog):
                                OnPropertyChanged(nameof(SkippedPathsLog));
                                break;
                            case nameof(MainViewModel.SelectedFolderPath):
                                OnPropertyChanged(nameof(SelectedFolderPath));
                                break;
                        }
                    };
                }

                // Initial raise to populate bindings
                OnPropertyChanged(nameof(ElapsedTime));
                OnPropertyChanged(nameof(MemoryUsage));
                OnPropertyChanged(nameof(HasSelectedFolder));
                OnPropertyChanged(nameof(TreeOutput));
                OnPropertyChanged(nameof(SkippedPathsLog));
                OnPropertyChanged(nameof(SelectedFolderPath));
            }
        }
    }

    public string? ElapsedTime => Parent?.ElapsedTime;
    public string? MemoryUsage => Parent?.MemoryUsage;
    public bool HasSelectedFolder => Parent?.HasSelectedFolder ?? false;
    public string? TreeOutput => Parent?.TreeOutput;
    public string? SkippedPathsLog => Parent?.SkippedPathsLog;
    public string? SelectedFolderPath => Parent?.SelectedFolderPath;

    public ICommand CopyPreviewCommand => Parent?.CopyPreviewCommand!;
}