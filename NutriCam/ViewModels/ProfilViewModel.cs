using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriCam.Helpers;
using NutriCam.Models;
using NutriCam.Services;

namespace NutriCam.ViewModels;

/// <summary>
/// Étape 1/5 : âge, taille et poids (champs de saisie), sexe, niveau d'activité, repas par jour et situations de santé (cases à cocher).
/// Rien n'est rempli, choisi ni coché d'avance : tout est à renseigner par la personne.
/// Si un élément manque, la page défile jusqu'au premier élément à renseigner.
/// Une maladie ou une situation à risque déclarée oriente vers un médecin nutritionniste : aucun programme n'est généré.
/// </summary>
public partial class ProfilViewModel : BaseViewModel
{
    private const int MinMeals = 2;

    private const string NoneLabel = "Aucune de ces situations";
    private static readonly string[] ConditionChoices =
    {
        "Diabète", "Hypertension", "Maladie cardiaque", "Maladie rénale", "Maladie du foie",
        "Trouble de l’alimentation", "Grossesse ou allaitement", "Autre maladie chronique"
    };

    public ProfilViewModel(ProgramStore store) : base(store)
    {
        var p = store.Data.Profile;

        // Valeurs déjà validées par la personne (retour sur l'étape) ; sinon tout est vide.
        ageText = p.Age?.ToString() ?? "";
        heightText = p.HeightCm?.ToString("0.#") ?? "";
        weightText = p.WeightKg?.ToString("0.#") ?? "";
        sexIndex = p.Sex is null ? -1 : (int)p.Sex.Value;
        activityIndex = p.Activity is null ? -1 : (int)p.Activity.Value;
        mealsIndex = p.MealsPerDay is null ? -1 : p.MealsPerDay.Value - MinMeals;

        Conditions = new ObservableCollection<CheckItem>(
            ConditionChoices.Select(c => new CheckItem(c, OnConditionChanged, p.Conditions.Contains(c))));
        Conditions.Add(new CheckItem(NoneLabel, OnConditionChanged, p.HealthChecked && p.Conditions.Count == 0));
    }

    public ObservableCollection<CheckItem> Conditions { get; }

    [ObservableProperty] private string ageText;
    [ObservableProperty] private string heightText;
    [ObservableProperty] private string weightText;
    [ObservableProperty] private int sexIndex;
    [ObservableProperty] private int activityIndex;
    [ObservableProperty] private int mealsIndex;

    [ObservableProperty] private string ageError = "";
    [ObservableProperty] private string heightError = "";
    [ObservableProperty] private string weightError = "";
    [ObservableProperty] private string sexError = "";
    [ObservableProperty] private string activityError = "";
    [ObservableProperty] private string mealsError = "";
    [ObservableProperty] private string healthError = "";

    /// <summary>« Aucune de ces situations » est exclusive : elle décoche les maladies, et inversement.</summary>
    private void OnConditionChanged(CheckItem item)
    {
        if (!item.IsChecked) return;
        var others = item.Label == NoneLabel
            ? Conditions.Where(c => c != item)
            : Conditions.Where(c => c.Label == NoneLabel);
        foreach (var other in others) other.IsChecked = false;
    }

    [RelayCommand]
    private async Task Continue()
    {
        AgeError = Validation.NumberInRange(AgeText, 14, 100, "Entrez un âge entre 14 et 100 ans.", out var age);
        HeightError = Validation.NumberInRange(HeightText, 100, 230, "Entrez une taille entre 100 et 230 cm.", out var height);
        WeightError = Validation.NumberInRange(WeightText, 25, 250, "Entrez un poids entre 25 et 250 kg.", out var weight);
        SexError = SexIndex < 0 ? "Indiquez votre sexe." : "";
        ActivityError = ActivityIndex < 0 ? "Indiquez votre niveau d’activité." : "";
        MealsError = MealsIndex < 0 ? "Indiquez le nombre de repas par jour." : "";
        HealthError = Conditions.Any(c => c.IsChecked) ? "" : "Cochez les situations qui vous concernent, ou « Aucune de ces situations ».";

        // Premier élément manquant, dans l'ordre de la page : on y fait défiler directement.
        var firstMissing =
            AgeError != "" ? "age" :
            HeightError != "" ? "height" :
            WeightError != "" ? "weight" :
            SexError != "" ? "sex" :
            ActivityError != "" ? "activity" :
            MealsError != "" ? "meals" :
            HealthError != "" ? "health" : null;
        if (firstMissing is not null)
        {
            ScrollTo(firstMissing);
            return;
        }

        var profile = Store.Data.Profile;
        profile.Age = (int)age;
        profile.HeightCm = height;
        profile.WeightKg = weight;
        profile.Sex = (Sex)SexIndex;
        profile.Activity = (ActivityLevel)ActivityIndex;
        profile.MealsPerDay = MealsIndex + MinMeals;
        profile.Conditions = Conditions.Where(c => c.IsChecked && c.Label != NoneLabel).Select(c => c.Label).ToList();
        profile.HealthChecked = true;

        if (profile.Conditions.Count > 0)
        {
            // Maladie ou situation à risque : pas de programme, orientation vers un médecin nutritionniste.
            Store.BlockProgram();
            await Go("orientation");
            return;
        }
        Store.Save();

        if (await ReturnToRecapIfRequested()) return;
        await Go("objectif");
    }
}
