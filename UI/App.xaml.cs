namespace TreeGlyph.UI;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState) =>
    new Window(new AppShell())
    {
        Width = 1700,
        Height = 800,
        X = 100,
        Y = 100
    };
    /*
    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
    */
}