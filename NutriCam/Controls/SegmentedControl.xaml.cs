using Microsoft.Maui.Controls.Shapes;
using NutriCam.Helpers;

namespace NutriCam.Controls;

/// <summary>
/// Choix unique à 2-3 segments (niveau d'activité, budget).
/// Items : libellés séparés par des virgules. SelectedIndex : segment actif (liaison bidirectionnelle).
/// </summary>
public partial class SegmentedControl : ContentView
{
    private readonly List<(Border Box, Label Caption)> _cells = new();

    public static readonly BindableProperty ItemsProperty =
        Bind.Prop<SegmentedControl, string>(nameof(Items), "", (c, _) => c.Rebuild());

    public static readonly BindableProperty SelectedIndexProperty =
        Bind.Prop<SegmentedControl, int>(nameof(SelectedIndex), 0, (c, _) => c.Refresh(), BindingMode.TwoWay);

    public string Items { get => (string)GetValue(ItemsProperty); set => SetValue(ItemsProperty, value); }
    public int SelectedIndex { get => (int)GetValue(SelectedIndexProperty); set => SetValue(SelectedIndexProperty, value); }

    public SegmentedControl()
    {
        InitializeComponent();
    }

    private void Rebuild()
    {
        Row.Children.Clear();
        Row.ColumnDefinitions.Clear();
        _cells.Clear();

        var names = Items.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        for (var i = 0; i < names.Length; i++)
        {
            var index = i;
            Row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            var caption = new Label
            {
                Text = names[i],
                FontSize = 13,
                FontFamily = "InterSemiBold",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            var box = new Border
            {
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
                Content = caption
            };
            box.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => SelectedIndex = index) });

            Grid.SetColumn(box, i);
            Row.Children.Add(box);
            _cells.Add((box, caption));
        }
        Refresh();
    }

    private void Refresh()
    {
        for (var i = 0; i < _cells.Count; i++)
        {
            var selected = i == SelectedIndex;
            _cells[i].Box.BackgroundColor = selected ? Palette.Terracotta : Colors.Transparent;
            _cells[i].Caption.TextColor = selected ? Colors.White : Palette.EncreDouce;
        }
    }
}
