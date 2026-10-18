using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using NutriCam.Models;
using NutriCam.Services;

namespace NutriCam.ViewModels;

/// <summary>Écran « Une semaine à savourer » : sélecteur de jour et menu du jour choisi.</summary>
public partial class ProgrammeViewModel : BaseViewModel
{
    public ProgrammeViewModel(ProgramStore store) : base(store) { }

    public ObservableCollection<DayTab> Days { get; } = new();
    public ObservableCollection<MealItem> Meals { get; } = new();

    [ObservableProperty] private string sectionTitle = "";

    public override void OnAppearing()
    {
        var selected = Days.FirstOrDefault(d => d.IsSelected)?.Index ?? Store.TodayIndex;

        Days.Clear();
        for (var i = 0; i < 7; i++)
            Days.Add(new DayTab(i, Store.DayShort(i), SelectDay));

        SelectDay(Days[selected]);
    }

    private void SelectDay(DayTab tab)
    {
        foreach (var day in Days)
            day.IsSelected = day == tab;

        Meals.Clear();
        foreach (var meal in Store.MealsOfDay(tab.Index))
            Meals.Add(new MealItem(meal, meal.Moment == "Déjeuner", OpenMeal));

        var kcal = NutritionCalculator.DayKcal(Meals.Select(m => m.Meal));
        SectionTitle = kcal > 0
            ? $"{Store.DayName(tab.Index)} · {Meals.Count} repas · ≈ {kcal:N0} kcal"
            : $"{Store.DayName(tab.Index)} · {Meals.Count} repas";
    }

    private async void OpenMeal(Meal meal)
    {
        Store.CurrentMeal = meal;
        await Go("repas");
    }
}
