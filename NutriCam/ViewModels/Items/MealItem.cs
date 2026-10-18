using CommunityToolkit.Mvvm.Input;
using NutriCam.Models;

namespace NutriCam.ViewModels;

/// <summary>Un repas tel qu'affiché dans une liste ; OpenCommand ouvre son détail.</summary>
public class MealItem
{
    public MealItem(Meal meal, bool featured, Action<Meal> open)
    {
        Meal = meal;
        IsFeatured = featured;
        OpenCommand = new RelayCommand(() => open(meal));
    }

    public Meal Meal { get; }
    public bool IsFeatured { get; }
    public string Moment => Meal.Done ? $"{Meal.Moment} · mangé" : Meal.Moment;
    public string Name => Meal.Name;
    public string Ingredients => Meal.VegetablesSummary;
    public string Image => Meal.Image;
    public IRelayCommand OpenCommand { get; }
}
