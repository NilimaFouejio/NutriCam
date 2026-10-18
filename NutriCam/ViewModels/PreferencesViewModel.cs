using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriCam.Helpers;
using NutriCam.Services;

namespace NutriCam.ViewModels;

/// <summary>
/// Étape 3/4 : aliments aimés, allergies ou intolérances (exclusion stricte) et aliments à éviter (préférence).
/// Un même aliment ne peut figurer que dans une seule des trois listes.
/// </summary>
public partial class PreferencesViewModel : BaseViewModel
{
    private static readonly string[] LikeChoices = { "Plantain", "Riz", "Manioc", "Haricots", "Poisson", "Œufs", "Légumes", "Poulet" };
    private static readonly string[] AllergyChoices = { "Arachides", "Fruits de mer", "Gluten", "Lait", "Œufs" };
    private static readonly string[] AvoidChoices = { "Porc", "Viande rouge", "Piment" };

    public PreferencesViewModel(ProgramStore store) : base(store)
    {
        var data = store.Data;
        Liked = new ObservableCollection<ChipItem>(
            LikeChoices.Select(l => new ChipItem(l, ToggleLiked, data.Liked.Any(x => TextUtil.SameFood(x, l)))));

        Allergies = new ObservableCollection<ChipItem>(
            AllergyChoices.Select(a => new ChipItem(a, ToggleAllergy, data.Allergies.Any(x => TextUtil.SameFood(x, a)))));

        Avoided = new ObservableCollection<ChipItem>(
            AvoidChoices.Select(a => new ChipItem(a, ToggleAvoided, data.Avoided.Any(x => TextUtil.SameFood(x, a)))));

        // Aliments saisis à la main lors d'une précédente visite
        foreach (var custom in data.Avoided.Where(x => !AvoidChoices.Any(a => TextUtil.SameFood(a, x))))
            Avoided.Add(new ChipItem(custom, ToggleAvoided, true));
    }

    public ObservableCollection<ChipItem> Liked { get; }
    public ObservableCollection<ChipItem> Allergies { get; }
    public ObservableCollection<ChipItem> Avoided { get; }

    [ObservableProperty] private string newAvoided = "";

    private void ToggleLiked(ChipItem chip)
    {
        chip.IsSelected = !chip.IsSelected;
        if (!chip.IsSelected) return;
        Unselect(Allergies, chip.Label);
        Unselect(Avoided, chip.Label);
    }

    private void ToggleAllergy(ChipItem chip)
    {
        chip.IsSelected = !chip.IsSelected;
        if (!chip.IsSelected) return;
        Unselect(Liked, chip.Label);
        Unselect(Avoided, chip.Label);
    }

    private void ToggleAvoided(ChipItem chip)
    {
        chip.IsSelected = !chip.IsSelected;
        if (chip.IsSelected)
        {
            Unselect(Liked, chip.Label);
            Unselect(Allergies, chip.Label);
        }
        else if (!AvoidChoices.Any(a => TextUtil.SameFood(a, chip.Label)))
        {
            Avoided.Remove(chip); // une pastille saisie à la main disparaît quand on la décoche
        }
    }

    private static void Unselect(IEnumerable<ChipItem> chips, string label)
    {
        foreach (var chip in chips.Where(c => TextUtil.SameFood(c.Label, label)))
            chip.IsSelected = false;
    }

    [RelayCommand]
    private void AddAvoided()
    {
        var label = TextUtil.Capitalize(NewAvoided.Trim());
        if (label.Length == 0) return;

        var existing = Avoided.FirstOrDefault(c => TextUtil.SameFood(c.Label, label));
        if (existing is null)
        {
            Avoided.Add(new ChipItem(label, ToggleAvoided, true));
            Unselect(Liked, label);
            Unselect(Allergies, label);
        }
        else if (!existing.IsSelected)
        {
            ToggleAvoided(existing);
        }
        NewAvoided = "";
    }

    [RelayCommand]
    private async Task Continue()
    {
        Store.Data.Liked = Liked.Where(c => c.IsSelected).Select(c => c.Label).ToList();
        Store.Data.Allergies = Allergies.Where(c => c.IsSelected).Select(c => c.Label).ToList();
        Store.Data.Avoided = Avoided.Where(c => c.IsSelected).Select(c => c.Label).ToList();
        Store.Save();
        await Go("budget");
    }
}
