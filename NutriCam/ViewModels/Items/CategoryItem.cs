using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriCam.Helpers;
using NutriCam.Models;

namespace NutriCam.ViewModels;

/// <summary>Une ligne de la composition du repas (légumes, protéines, complément).</summary>
public partial class CategoryItem : ObservableObject
{
    public CategoryItem(FoodCategory category, string caption, string value, string note, Action<CategoryItem> onSelect)
    {
        Category = category;
        Caption = caption;
        Value = value;
        Note = note;
        SelectCommand = new RelayCommand(() => onSelect(this));
    }

    public FoodCategory Category { get; }
    public string Caption { get; }
    public string Value { get; }
    public string Note { get; }
    public bool HasNote => Note.Length > 0;
    public IRelayCommand SelectCommand { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Fill), nameof(Outline), nameof(IsNotSelected))]
    private bool isSelected;

    public bool IsNotSelected => !IsSelected;
    public Color Fill => IsSelected ? Palette.Teinte : Colors.White;
    public Color Outline => IsSelected ? Palette.Terracotta : Palette.Bordure;
}
