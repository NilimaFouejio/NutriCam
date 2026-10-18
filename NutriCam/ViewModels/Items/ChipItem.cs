using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace NutriCam.ViewModels;

/// <summary>Une pastille d'aliment (sélectionnée ou non) ; ToggleCommand prévient la page qui la contient.</summary>
public partial class ChipItem : ObservableObject
{
    public ChipItem(string label, Action<ChipItem> onToggle, bool selected = false)
    {
        Label = label;
        IsSelected = selected;
        ToggleCommand = new RelayCommand(() => onToggle(this));
    }

    public string Label { get; }

    public IRelayCommand ToggleCommand { get; }

    [ObservableProperty] private bool isSelected;
}
