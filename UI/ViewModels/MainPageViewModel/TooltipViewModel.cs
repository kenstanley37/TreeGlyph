using CommunityToolkit.Mvvm.ComponentModel;

namespace TreeGlyph.UI.ViewModels.MainPageViewModel;

public partial class TooltipViewModel : ObservableObject
{
    public MainViewModel? Parent { get; set; }

    private bool isTooltipOpen;
    public bool IsTooltipOpen
    {
        get => isTooltipOpen;
        set => SetProperty(ref isTooltipOpen, value);
    }

    private string tooltipMessage = string.Empty;
    public string TooltipMessage
    {
        get => tooltipMessage;
        set => SetProperty(ref tooltipMessage, value);
    }

    public void Show(string message)
    {
        TooltipMessage = message;
        IsTooltipOpen = true;
    }

    public void Hide()
    {
        IsTooltipOpen = false;
        TooltipMessage = string.Empty;
    }
}