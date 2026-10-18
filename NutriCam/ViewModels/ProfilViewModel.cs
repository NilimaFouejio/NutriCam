using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriCam.Helpers;
using NutriCam.Models;
using NutriCam.Services;

namespace NutriCam.ViewModels;

/// <summary>Étape 1/4 : âge, taille, poids, niveau d'activité, nombre de repas par jour.</summary>
public partial class ProfilViewModel : BaseViewModel
{
    private const int MinMeals = 2;
    private const int MaxMeals = 5;

    public ProfilViewModel(ProgramStore store) : base(store)
    {
        var p = store.Data.Profile;
        ageText = p.Age?.ToString() ?? "";
        heightText = p.HeightCm?.ToString("0.#") ?? "";
        weightText = p.WeightKg?.ToString("0.#") ?? "";
        activityIndex = (int)p.Activity;
        mealsPerDay = Math.Clamp(p.MealsPerDay, MinMeals, MaxMeals);
    }

    [ObservableProperty] private string ageText;
    [ObservableProperty] private string heightText;
    [ObservableProperty] private string weightText;
    [ObservableProperty] private string ageError = "";
    [ObservableProperty] private string heightError = "";
    [ObservableProperty] private string weightError = "";
    [ObservableProperty] private int activityIndex;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MealsLabel))]
    [NotifyCanExecuteChangedFor(nameof(DecreaseCommand))]
    [NotifyCanExecuteChangedFor(nameof(IncreaseCommand))]
    private int mealsPerDay;

    public string MealsLabel => $"{MealsPerDay} repas";

    [RelayCommand(CanExecute = nameof(CanDecrease))]
    private void Decrease() => MealsPerDay--;
    private bool CanDecrease() => MealsPerDay > MinMeals;

    [RelayCommand(CanExecute = nameof(CanIncrease))]
    private void Increase() => MealsPerDay++;
    private bool CanIncrease() => MealsPerDay < MaxMeals;

    [RelayCommand]
    private async Task Continue()
    {
        AgeError = Validation.NumberInRange(AgeText, 14, 100, "Entrez un âge entre 14 et 100 ans.", out var age);
        HeightError = Validation.NumberInRange(HeightText, 100, 230, "Entrez une taille entre 100 et 230 cm.", out var height);
        WeightError = Validation.NumberInRange(WeightText, 25, 250, "Entrez un poids entre 25 et 250 kg.", out var weight);
        if (AgeError != "" || HeightError != "" || WeightError != "") return;

        var profile = Store.Data.Profile;
        profile.Age = (int)age;
        profile.HeightCm = height;
        profile.WeightKg = weight;
        profile.Activity = (ActivityLevel)ActivityIndex;
        profile.MealsPerDay = MealsPerDay;
        Store.Save();

        await Go("objectif");
    }
}
