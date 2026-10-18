using CommunityToolkit.Mvvm.Input;

namespace NutriCam.ViewModels;

/// <summary>Une ligne de l'écran Profil et réglages : titre, résumé de la valeur actuelle et action au toucher.</summary>
public class SettingsRow
{
    public SettingsRow(string title, string detail, string glyph, Func<Task> action)
    {
        Title = title;
        Detail = detail;
        Glyph = glyph;
        OpenCommand = new AsyncRelayCommand(action);
    }

    public string Title { get; }
    public string Detail { get; }
    public string Glyph { get; }
    public IAsyncRelayCommand OpenCommand { get; }
}
