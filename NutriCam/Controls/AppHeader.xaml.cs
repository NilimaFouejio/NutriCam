using NutriCam.Helpers;

namespace NutriCam.Controls;

/// <summary>
/// En-tête des écrans. Sans retour : marque à gauche + signature « À votre rythme ».
/// Avec retour : bouton retour à gauche, marque à droite.
/// </summary>
public partial class AppHeader : ContentView
{
    public static readonly BindableProperty ShowBackProperty =
        Bind.Prop<AppHeader, bool>(nameof(ShowBack), false, (c, _) => c.Update());

    public bool ShowBack
    {
        get => (bool)GetValue(ShowBackProperty);
        set => SetValue(ShowBackProperty, value);
    }

    public AppHeader()
    {
        InitializeComponent();
        Update();
    }

    private void Update()
    {
        BackButton.IsVisible = ShowBack;
        Tagline.IsVisible = !ShowBack;
        Grid.SetColumn(Brand, ShowBack ? 2 : 0);
    }

    private async void OnBackTapped(object? sender, TappedEventArgs e)
        => await Nav.GoAsync("..");
}
