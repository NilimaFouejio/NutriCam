using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace NutriCam.ViewModels;

/// <summary>Une carte de choix unique (objectif, remplacement). Key identifie l'option.</summary>
public partial class OptionItem : ObservableObject
{
    public OptionItem(string key, string title, string subtitle, string glyph, Action<OptionItem> onSelect)
    {
        Key = key;
        Title = title;
        Subtitle = subtitle;
        Glyph = glyph;
        SelectCommand = new RelayCommand(() => onSelect(this));
    }

    public string Key { get; }
    public string Title { get; }
    public string Subtitle { get; }
    public string Glyph { get; }

    public IRelayCommand SelectCommand { get; }

    [ObservableProperty] private bool isSelected;
}
