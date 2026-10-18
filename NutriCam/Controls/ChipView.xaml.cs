using System.Windows.Input;
using NutriCam.Helpers;

namespace NutriCam.Controls;

/// <summary>Pastille sélectionnable (aliments aimés, évités, disponibles).</summary>
public partial class ChipView : ContentView
{
    public static readonly BindableProperty TextProperty =
        Bind.Prop<ChipView, string>(nameof(Text), "", (c, v) => c.Caption.Text = v);

    public static readonly BindableProperty IsSelectedProperty =
        Bind.Prop<ChipView, bool>(nameof(IsSelected), false, (c, _) => c.Refresh());

    public static readonly BindableProperty SelectedGlyphProperty =
        Bind.Prop<ChipView, string>(nameof(SelectedGlyph), Icons.Check, (c, _) => c.Refresh());

    public static readonly BindableProperty CommandProperty =
        Bind.Prop<ChipView, ICommand?>(nameof(Command), null);

    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public bool IsSelected { get => (bool)GetValue(IsSelectedProperty); set => SetValue(IsSelectedProperty, value); }

    /// <summary>Icône affichée quand la pastille est sélectionnée (coche, ou croix pour les exclusions).</summary>
    public string SelectedGlyph { get => (string)GetValue(SelectedGlyphProperty); set => SetValue(SelectedGlyphProperty, value); }

    public ICommand? Command { get => (ICommand?)GetValue(CommandProperty); set => SetValue(CommandProperty, value); }

    public ChipView()
    {
        InitializeComponent();
        Refresh();
    }

    private void Refresh()
    {
        Pill.BackgroundColor = IsSelected ? Palette.Teinte : Colors.White;
        Pill.Stroke = IsSelected ? Palette.Terracotta : Palette.Bordure;
        Caption.TextColor = IsSelected ? Palette.Terracotta : Palette.Encre;
        Caption.FontFamily = IsSelected ? "InterSemiBold" : "InterRegular";
        Mark.Text = SelectedGlyph;
        Mark.IsVisible = IsSelected;
    }

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        if (Command?.CanExecute(null) == true)
            Command.Execute(null);
    }
}
