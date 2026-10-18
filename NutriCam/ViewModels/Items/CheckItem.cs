using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace NutriCam.ViewModels;

/// <summary>Une case à cocher (libellé + état). Le rappel prévient la page quand la case change.</summary>
public partial class CheckItem : ObservableObject
{
    private readonly Action<CheckItem> _onChanged;

    public CheckItem(string label, Action<CheckItem> onChanged, bool isChecked = false)
    {
        Label = label;
        _onChanged = onChanged;
        this.isChecked = isChecked;
        ToggleCommand = new RelayCommand(() => IsChecked = !IsChecked);
    }

    public string Label { get; }

    public IRelayCommand ToggleCommand { get; }

    [ObservableProperty] private bool isChecked;

    partial void OnIsCheckedChanged(bool value) => _onChanged(this);
}
