using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace UI.ViewModels.MainPageViewModel;

public partial class OptionsPanelViewModel : ObservableObject
{
    public View? PanelView { get; set; }

    private bool isPanelOpen;
    public bool IsPanelOpen
    {
        get => isPanelOpen;
        set
        {
            if (SetProperty(ref isPanelOpen, value))
                AnimatePanel();
        }
    }

    private int maxDepth = Preferences.Get(nameof(MaxDepth), 99);
    public int MaxDepth
    {
        get => maxDepth;
        set
        {
            if (SetProperty(ref maxDepth, value))
                Preferences.Set(nameof(MaxDepth), value);
        }
    }

    private bool autoSaveIgnoreFile = Preferences.Get(nameof(AutoSaveIgnoreFile), false);
    public bool AutoSaveIgnoreFile
    {
        get => autoSaveIgnoreFile;
        set
        {
            if (SetProperty(ref autoSaveIgnoreFile, value))
                Preferences.Set(nameof(AutoSaveIgnoreFile), value);
        }
    }

    [RelayCommand]
    private void TogglePanel() => IsPanelOpen = !IsPanelOpen;

    private async void AnimatePanel()
    {
        if (PanelView == null) return;
        double targetX = IsPanelOpen ? 0 : 288;
        await PanelView.TranslateTo(targetX, 0, 250, Easing.CubicInOut);
    }
}