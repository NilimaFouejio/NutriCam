using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriCam.Models;
using NutriCam.Services;

namespace NutriCam.ViewModels;

/// <summary>Étape 4/5 : budget et aliments faciles à trouver. La génération se fait depuis le récapitulatif (étape 5/5).</summary>
public partial class BudgetViewModel : BaseViewModel
{
    private static readonly string[] AvailabilityChoices = { "Riz", "Plantain", "Manioc", "Haricots", "Œufs", "Poisson", "Légumes", "Poulet" };

    public BudgetViewModel(ProgramStore store) : base(store)
    {
        budgetIndex = store.Data.Budget is { } saved ? (int)saved : -1; // -1 : aucun choix tant que la personne n'a pas répondu
        Available = new ObservableCollection<ChipItem>(
            AvailabilityChoices.Select(a => new ChipItem(a, c => c.IsSelected = !c.IsSelected,
                store.Data.Available.Any(x => Helpers.TextUtil.SameFood(x, a)))));
    }

    public ObservableCollection<ChipItem> Available { get; }

    [ObservableProperty] private int budgetIndex;

    [ObservableProperty] private string budgetError = "";

    [ObservableProperty] private string availableError = "";

    [RelayCommand]
    private async Task Continue()
    {
        var chosen = Available.Where(c => c.IsSelected).Select(c => c.Label).ToList();
        BudgetError = BudgetIndex < 0 ? "Choisissez un budget." : "";
        AvailableError = chosen.Count == 0 ? "Choisissez au moins un aliment facile à trouver." : "";
        if (BudgetError != "" || AvailableError != "")
        {
            ScrollTo(BudgetError != "" ? "budget" : "available");
            return;
        }

        Store.Data.Budget = (BudgetLevel)BudgetIndex;
        Store.Data.Available = chosen;
        Store.Save();

        if (await ReturnToRecapIfRequested()) return;
        await Go("recap");
    }
}
