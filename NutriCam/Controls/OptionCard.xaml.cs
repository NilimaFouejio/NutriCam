using System.Windows.Input;
using NutriCam.Helpers;

namespace NutriCam.Controls;

/// <summary>Carte de choix unique avec pictogramme (objectifs, remplacements d'aliments).</summary>
public partial class OptionCard : ContentView
{
    public static readonly BindableProperty TitleProperty =
        Bind.Prop<OptionCard, string>(nameof(Title), "", (c, v) => c.TitleLabel.Text = v);

    public static readonly BindableProperty SubtitleProperty =
        Bind.Prop<OptionCard, string>(nameof(Subtitle), "", (c, v) => c.SubtitleLabel.Text = v);

    public static readonly BindableProperty GlyphProperty =
        Bind.Prop<OptionCard, string>(nameof(Glyph), "", (c, v) => c.Pictogram.Text = v);

    public static readonly BindableProperty IsSelectedProperty =
        Bind.Prop<OptionCard, bool>(nameof(IsSelected), false, (c, _) => c.Refresh());

    public static readonly BindableProperty CommandProperty =
        Bind.Prop<OptionCard, ICommand?>(nameof(Command), null);

    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public string Subtitle { get => (string)GetValue(SubtitleProperty); set => SetValue(SubtitleProperty, value); }
    public string Glyph { get => (string)GetValue(GlyphProperty); set => SetValue(GlyphProperty, value); }
    public bool IsSelected { get => (bool)GetValue(IsSelectedProperty); set => SetValue(IsSelectedProperty, value); }
    public ICommand? Command { get => (ICommand?)GetValue(CommandProperty); set => SetValue(CommandProperty, value); }

    public OptionCard()
    {
        InitializeComponent();
        Refresh();
    }

    private void Refresh()
    {
        Card.BackgroundColor = IsSelected ? Palette.Teinte : Colors.White;
        Card.Stroke = IsSelected ? Palette.Terracotta : Palette.Bordure;
        Card.StrokeThickness = IsSelected ? 1.5 : 1;
        Radio.BackgroundColor = IsSelected ? Palette.Terracotta : Colors.White;
        Radio.Stroke = IsSelected ? Palette.Terracotta : Palette.Bordure;
    }

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        if (Command?.CanExecute(null) == true)
            Command.Execute(null);
    }
}
