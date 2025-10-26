using CommunityToolkit.Mvvm.ComponentModel;

namespace UI.ViewModels.UtilityViewModels;

public partial class SelectableKeyItem : ObservableObject
{
    public string Key { get; }

    private bool isSelected;
    public bool IsSelected
    {
        get => isSelected;
        set
        {
            if (SetProperty(ref isSelected, value))
            {
                //BackgroundColor = value ? Colors.LightGreen : Colors.Transparent;
            }
        }

    }

    public SelectableKeyItem(string key)
    {
        Key = key;
    }

    private Color backgroundColor = Colors.Transparent;
    public Color BackgroundColor
    {
        get => backgroundColor;
        set => SetProperty(ref backgroundColor, value);
    }
}
