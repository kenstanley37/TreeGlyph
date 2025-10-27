using CommunityToolkit.Maui.Views;

namespace TreeGlyph.UI.Views;

public partial class TooltipPopup : Popup
{
    public TooltipPopup(string message)
    {
        InitializeComponent();
        TooltipLabel.Text = message;

        // Auto-dismiss the popup after 2 seconds
        Task.Run(async () =>
        {
            await Task.Delay(2000);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    this.Handler?.DisconnectHandler();
                }
                catch
                {
                    // silently fail if already disconnected
                }
            });
        });
    }
}