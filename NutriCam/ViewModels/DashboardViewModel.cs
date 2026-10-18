using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriCam.Models;
using NutriCam.Services;

namespace NutriCam.ViewModels;

/// <summary>Écran « Bonjour, à table ! » : objectif, avancée de la semaine et repas du jour.</summary>
public partial class DashboardViewModel : BaseViewModel
{
    private static readonly string[] Headlines =
    {
        "Perdre du poids, sans frustration",
        "Maintenir mon poids",
        "Prendre du poids, et bien manger",
        "Mieux équilibrer mon alimentation"
    };

    public DashboardViewModel(ProgramStore store) : base(store) { }

    public ObservableCollection<MealItem> Meals { get; } = new();

    [ObservableProperty] private string goalHeadline = "";
    [ObservableProperty] private string dayProgress = "";
    [ObservableProperty] private string todayName = "";
    [ObservableProperty] private bool weekFinished;
    [ObservableProperty] private bool weekActive = true;
    [ObservableProperty] private IReadOnlyList<DayBar> bars = Array.Empty<DayBar>();

    public override void OnAppearing()
    {
        var today = Store.TodayIndex;
        GoalHeadline = Headlines[(int)Store.Data.Goal];
        WeekFinished = Store.WeekFinished;
        WeekActive = !WeekFinished;
        DayProgress = WeekFinished ? "Semaine terminée" : $"Jour {today + 1} sur 7";
        TodayName = Store.DayName(today);
        Bars = Enumerable.Range(0, 7).Select(i => new DayBar(i <= today)).ToList();

        Meals.Clear();
        foreach (var meal in WeekFinished ? Enumerable.Empty<Meal>() : Store.MealsOfDay(today))
            Meals.Add(new MealItem(meal, false, OpenMeal));
    }

    private async void OpenMeal(Meal meal)
    {
        Store.CurrentMeal = meal;
        await Go("repas");
    }

    [RelayCommand]
    private Task OpenBilan() => Go("bilan");

    [RelayCommand]
    private Task SeeWeek() => Go("//programme");

    [RelayCommand]
    private async Task Regenerate()
    {
        var confirmed = await Shell.Current.DisplayAlert(
            "Régénérer mon programme ?",
            "Vos remplacements seront perdus et une nouvelle semaine de repas sera proposée.",
            "Régénérer", "Annuler");
        if (!confirmed) return;

        IsBusy = true;
        await Task.Delay(1200);
        Store.GenerateProgram();
        IsBusy = false;
        OnAppearing();
    }
}
