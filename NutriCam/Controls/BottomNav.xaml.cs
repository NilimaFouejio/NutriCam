using NutriCam.Helpers;

namespace NutriCam.Controls;

/// <summary>Barre de navigation du bas : Accueil, Programme, Profil. Active = "home", "programme" ou "profil".</summary>
public partial class BottomNav : ContentView
{
    public static readonly BindableProperty ActiveProperty =
        Bind.Prop<BottomNav, string>(nameof(Active), "home", (c, _) => c.Update());

    public string Active { get => (string)GetValue(ActiveProperty); set => SetValue(ActiveProperty, value); }

    public BottomNav()
    {
        InitializeComponent();
        Update();
    }

    private void Update()
    {
        Style(HomeBox, HomeIcon, HomeCaption, Active == "home");
        Style(ProgramBox, ProgramIcon, ProgramCaption, Active == "programme");
        Style(ProfileBox, ProfileIcon, ProfileCaption, Active == "profil");
    }

    private static void Style(Border box, Label icon, Label caption, bool active)
    {
        box.BackgroundColor = active ? Palette.Teinte : Colors.Transparent;
        icon.TextColor = active ? Palette.Terracotta : Palette.EncreDouce;
        caption.TextColor = active ? Palette.Terracotta : Palette.EncreDouce;
    }

    private async void OnHomeTapped(object? sender, TappedEventArgs e)
    {
        if (Active != "home") await Shell.Current.GoToAsync("//dashboard");
    }

    private async void OnProgramTapped(object? sender, TappedEventArgs e)
    {
        if (Active != "programme") await Shell.Current.GoToAsync("//programme");
    }

    private async void OnProfileTapped(object? sender, TappedEventArgs e)
    {
        if (Active != "profil") await Shell.Current.GoToAsync("//reglages");
    }
}
