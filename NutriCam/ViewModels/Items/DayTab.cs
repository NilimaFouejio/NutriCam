using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriCam.Helpers;

namespace NutriCam.ViewModels;

/// <summary>Un jour de la semaine (Lun, Mar…) dans le sélecteur de l'écran Programme.</summary>
public partial class DayTab : ObservableObject
{
    public DayTab(int index, string label, Action<DayTab> onSelect)
    {
        Index = index;
        Label = label;
        SelectCommand = new RelayCommand(() => onSelect(this));
    }

    public int Index { get; }
    public string Label { get; }
    public IRelayCommand SelectCommand { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Fill), nameof(TextColor), nameof(Marker))]
    private bool isSelected;

    public Color Fill => IsSelected ? Palette.Terracotta : Colors.White;
    public Color TextColor => IsSelected ? Colors.White : Palette.EncreDouce;
    public string Marker => IsSelected ? "●" : "·";
}
