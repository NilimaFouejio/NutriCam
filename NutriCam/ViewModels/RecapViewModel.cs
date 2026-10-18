using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriCam.Helpers;
using NutriCam.Models;
using NutriCam.Services;

namespace NutriCam.ViewModels;

/// <summary>
/// Étape 5/5 : récapitulatif de tout ce qui sert à composer le programme, besoin énergétique estimé,
/// activité physique suggérée par le système, puis génération.
/// </summary>
public partial class RecapViewModel : BaseViewModel
{
    private static readonly string[] GoalNames = { "Perdre du poids", "Maintenir mon poids", "Prendre du poids", "Mieux équilibrer mon alimentation" };
    private static readonly string[] ActivityNames = { "faible", "modérée", "élevée" };
    private static readonly string[] BudgetNames = { "économique", "moyen", "confort" };

    public RecapViewModel(ProgramStore store) : base(store) { }

    public ObservableCollection<SettingsRow> Rows { get; } = new();

    [ObservableProperty] private IReadOnlyList<ActivitySession> sessions = Array.Empty<ActivitySession>();
    [ObservableProperty] private string activityHeadline = "";
    [ObservableProperty] private string activityNote = "";
    [ObservableProperty] private string energyHeadline = "";
    [ObservableProperty] private string energyNote = "";
    [ObservableProperty] private bool hasEnergyNote;

    public string GenerateText => IsBusy ? "Préparation en cours…" : Store.EditMode ? "Mettre à jour mon programme" : "Générer mon programme";

    protected override void OnBusyChanged() => OnPropertyChanged(nameof(GenerateText));

    public override void OnAppearing()
    {
        // On revient d'une étape modifiée (ou on arrive de l'étape 4) : le récapitulatif reflète les choix à jour.
        Store.ReturnToRecap = false;
        RefreshRows();
        RefreshEnergy();
        RefreshActivity();
        OnPropertyChanged(nameof(GenerateText));
    }

    // --- Récapitulatif ---
    private void RefreshRows()
    {
        var data = Store.Data;
        var p = data.Profile;

        var sex = p.Sex is null ? "" : p.Sex == Sex.Homme ? "Homme · " : "Femme · ";
        var profile = p.Age is null ? "À renseigner" : $"{sex}{p.Age} ans · {p.HeightCm:0.#} cm · {p.WeightKg:0.#} kg";
        var activityAndMeals = $"Activité {ActivityNames[(int)(p.Activity ?? ActivityLevel.Modere)]} · {p.MealsPerDay ?? 3} repas par jour";

        var tastes = new List<string>
        {
            data.Liked.Count == 0 ? "Aucun aliment favori" : $"Aimés : {Join(data.Liked)}"
        };
        if (data.Allergies.Count > 0) tastes.Add($"Allergies : {Join(data.Allergies)}");
        if (data.Avoided.Count > 0) tastes.Add($"Évités : {Join(data.Avoided)}");
        if (data.Allergies.Count == 0 && data.Avoided.Count == 0) tastes.Add("Aucune exclusion");

        var budget = $"Budget {BudgetNames[(int)(data.Budget ?? BudgetLevel.Moyen)]}\nFaciles à trouver : {Join(data.Available)}";

        Rows.Clear();
        Rows.Add(new SettingsRow("Profil", profile, Icons.UserRound, () => Edit("profil")));
        Rows.Add(new SettingsRow("Activité et repas", activityAndMeals, Icons.Utensils, () => Edit("profil")));
        Rows.Add(new SettingsRow("Objectif", GoalNames[(int)(data.Goal ?? Goal.Equilibrer)], Icons.Scale, () => Edit("objectif")));
        Rows.Add(new SettingsRow("Goûts et exclusions", string.Join("\n", tastes), Icons.Carrot, () => Edit("preferences")));
        Rows.Add(new SettingsRow("Budget et disponibilité", budget, Icons.Wheat, () => Edit("budget")));
        Rows.Add(new SettingsRow("Santé", "Aucune maladie ni situation à suivre signalée", Icons.CircleCheck, () => Edit("profil")));
    }

    private static string Join(IEnumerable<string> items) => string.Join(", ", items.Select(i => i.ToLowerInvariant()));

    /// <summary>Rouvre une étape ; une fois validée, elle revient ici.</summary>
    private Task Edit(string route)
    {
        Store.ReturnToRecap = true;
        return Go(route);
    }

    // --- Besoin énergétique ---
    private void RefreshEnergy()
    {
        var target = NutritionCalculator.DailyTarget(Store.Data);
        EnergyHeadline = $"≈ {target:N0} kcal par jour";
        var note = NutritionCalculator.TargetNote(Store.Data);
        EnergyNote = note;
        HasEnergyNote = note.Length > 0;
    }

    // --- Activité physique (suggérée par le système) ---
    private void RefreshActivity()
    {
        var plan = ActivityPlanner.Build(Store.Data);

        Sessions = plan.Days;
        ActivityHeadline = $"{ActivityPlanner.SessionsLabel(plan.Sessions)} par semaine, {plan.MinutesPerSession} minutes";
        ActivityNote = $"Suggestion selon votre objectif et votre niveau d’activité : environ {plan.TotalMinutes} minutes par semaine. {plan.Note}";
    }

    // --- Génération ---
    [RelayCommand]
    private async Task Generate()
    {
        if (Store.IsBlocked)
        {
            await Go("orientation");
            return;
        }

        if (Store.Data.Profile.Age is null)
        {
            await Alert("Profil incomplet", "Renseignez votre profil avant de générer votre programme.");
            return;
        }

        IsBusy = true;
        await Task.Delay(1600); // simule la génération côté serveur
        Store.GenerateProgram();
        IsBusy = false;
        Store.EditMode = false;

        await Go("//dashboard");
    }
}
