namespace TreeGlyph.UI.Controls;

public class HorizontalSplitterView : Grid
{
    public static readonly BindableProperty Panel1ContentProperty =
        BindableProperty.Create(nameof(Panel1Content), typeof(View), typeof(HorizontalSplitterView), propertyChanged: OnContentChanged);

    public static readonly BindableProperty Panel2ContentProperty =
        BindableProperty.Create(nameof(Panel2Content), typeof(View), typeof(HorizontalSplitterView), propertyChanged: OnContentChanged);

    public static readonly BindableProperty Panel1WidthProperty =
        BindableProperty.Create(nameof(Panel1Width), typeof(double), typeof(HorizontalSplitterView), 300.0, BindingMode.TwoWay, propertyChanged: OnWidthChanged);

    public double Panel1Width
    {
        get => (double)GetValue(Panel1WidthProperty);
        set => SetValue(Panel1WidthProperty, value);
    }

    public View? Panel1Content
    {
        get => (View?)GetValue(Panel1ContentProperty);
        set => SetValue(Panel1ContentProperty, value);
    }

    public View? Panel2Content
    {
        get => (View?)GetValue(Panel2ContentProperty);
        set => SetValue(Panel2ContentProperty, value);
    }

    public double MinWidth { get; set; } = 240;
    public double MaxWidth { get; set; } = 640;

    public HorizontalSplitterView()
    {
        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Panel1Width, GridUnitType.Absolute) });
        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(6) });
        ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

        HorizontalOptions = LayoutOptions.Fill;
        VerticalOptions = LayoutOptions.Fill;

        // Force build at creation to include the splitter — even before content arrives
        Loaded += (_, _) => BuildContent();

    }

    private static void OnContentChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is HorizontalSplitterView splitter && splitter.IsLoaded)
        {
            splitter.BuildContent();
        }
    }

    private static void OnWidthChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is HorizontalSplitterView splitter)
        {
            splitter.ColumnDefinitions[0].Width = new GridLength((double)newValue, GridUnitType.Absolute);
        }
    }

    private void BuildContent()
    {
        Children.Clear();

        if (Panel1Content is View p1)
        {
            Children.Add(p1);
            SetColumn((BindableObject)p1, 0);
        }

        var splitter = new ContentView
        {
            BackgroundColor = Colors.Gray,
            WidthRequest = 6,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };

        var pan = new PanGestureRecognizer();
        pan.PanUpdated += OnSplitterDragged;
        splitter.GestureRecognizers.Add(pan);

        Children.Add(splitter);
        SetColumn((BindableObject)splitter, 1);

        if (Panel2Content is View p2)
        {
            Children.Add(p2);
            SetColumn((BindableObject)p2, 2);
        }
    }

    private void OnSplitterDragged(object? sender, PanUpdatedEventArgs e)
    {
        if (e.StatusType == GestureStatus.Running)
        {
            var newWidth = Math.Clamp(Panel1Width + e.TotalX, MinWidth, MaxWidth);
            Panel1Width = newWidth;
            ColumnDefinitions[0].Width = new GridLength(Panel1Width, GridUnitType.Absolute);
        }
    }
}