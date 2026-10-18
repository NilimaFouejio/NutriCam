using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriCam.Models;
using NutriCam.Services;

namespace NutriCam.ViewModels;

/// <summary>Étape 4/4 : budget, aliments faciles à trouver, résumé puis génération du programme.</summary>
public partial class BudgetViewModel : BaseViewModel
{
    private static readonly string[] AvailabilityChoices = { "Riz", "Plantain", "Manioc", "Haricots", "Œufs", "Poisson", "Légumes", "Poulet" };
    private static readonly string[] BudgetNames = { "économique", "moyen", "confort" };
    private static readonly string[] GoalNames = { "Perdre du poids", "Maintenir mon poids", "Prendre du poids", "Mieux équilibrer" };

    public BudgetViewModel(ProgramStore store) : base(store)
    {
        budgetIndex = (int)store.Data.Budget;
        Available = new ObservableCollection<ChipItem>(
            AvailabilityChoices.Select(a => new ChipItem(a, c => c.IsSelected = !c.IsSelected,
                store.Data.Available.Any(x => Helpers.TextUtil.SameFood(x, a)))));
    }

    public ObservableCollection<ChipItem> Available { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Summary))]
    private int budgetIndex;

    [ObservableProperty] private string availableError = "";

    public string GoalLabel => GoalNames[(int)Store.Data.Goal];

    public string Summary
    {
        get
        {
            var line = $"{Store.Data.Profile.MealsPerDay} repas par jour · budget {BudgetNames[BudgetIndex]}";
            var avoided = Store.Data.Excluded.ToList();
            return avoided.Count == 0 ? line : $"{line}\nSans {string.Join(", ", avoided.Select(a => a.ToLowerInvariant()))}";
        }
    }

    public string GenerateText => IsBusy ? "Préparation en cours…" : Store.EditMode ? "Mettre à jour mon programme" : "Générer mon programme";

    protected override void OnBusyChanged() => OnPropertyChanged(nameof(GenerateText));

    public override void OnAppearing()
    {
        // Le résumé reflète les choix des étapes précédentes (retour arrière compris).
        OnPropertyChanged(nameof(GoalLabel));
        OnPropertyChanged(nameof(Summary));
        OnPropertyChanged(nameof(GenerateText));
    }

    [RelayCommand]
    private async Task Generate()
    {
        var chosen = Available.Where(c => c.IsSelected).Select(c => c.Label).ToList();
        if (chosen.Count == 0)
        {
            AvailableError = "Choisissez au moins un aliment facile à trouver.";
            return;
        }
        AvailableError = "";

        Store.Data.Budget = (BudgetLevel)BudgetIndex;
        Store.Data.Available = chosen;

        IsBusy = true;
        await Task.Delay(1600); // simule la génération côté serveur
        Store.GenerateProgram();
        IsBusy = false;
        Store.EditMode = false;

        await Go("//dashboard");
    }
}
