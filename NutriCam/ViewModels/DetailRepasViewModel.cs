using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriCam.Models;
using NutriCam.Services;

namespace NutriCam.ViewModels;

/// <summary>Écran « Détail repas » : photo, repère pour l'assiette, composition, préparation et remplacement.</summary>
public partial class DetailRepasViewModel : BaseViewModel
{
    private FoodCategory _selected = FoodCategory.Proteines;

    public DetailRepasViewModel(ProgramStore store) : base(store) { }

    public ObservableCollection<CategoryItem> Categories { get; } = new();

    [ObservableProperty] private string momentLine = "";
    [ObservableProperty] private string name = "";
    [ObservableProperty] private string description = "";
    [ObservableProperty] private string image = "dejeuner.jpg";
    [ObservableProperty] private string preparation = "";
    [ObservableProperty] private string replaceText = "";
    [ObservableProperty] private string doneText = "";
    [ObservableProperty] private string banner = "";
    [ObservableProperty] private bool bannerVisible;

    public override async void OnAppearing()
    {
        var meal = Store.CurrentMeal;
        if (meal is null) return;

        MomentLine = $"{Store.DayName(meal.DayIndex)} · {meal.Moment}".ToUpperInvariant();
        Name = meal.Name;
        Description = meal.Description;
        Image = meal.Image;
        Preparation = meal.Preparation;
        DoneText = DoneLabel(meal);

        Categories.Clear();
        Categories.Add(new CategoryItem(FoodCategory.Legumes, "LÉGUMES · REPÈRE ½", string.Join(", ", meal.Vegetables), "", Select));
        Categories.Add(new CategoryItem(FoodCategory.Proteines, "PROTÉINES · REPÈRE ¼", meal.Protein, "", Select));
        Categories.Add(new CategoryItem(
            FoodCategory.Complement,
            "COMPLÉMENT · REPÈRE ¼",
            meal.Complement ?? "À compléter selon votre envie",
            meal.Complement is null ? "Par exemple du riz, non visible sur la photo." : "Non visible sur la photo.",
            Select));
        Select(Categories.First(c => c.Category == _selected));

        // Confirmation du remplacement effectué sur l'écran Alternatives
        if (Store.LastChange.Length > 0)
        {
            Banner = Store.LastChange;
            Store.LastChange = "";
            BannerVisible = true;
            await Task.Delay(3500);
            BannerVisible = false;
        }
    }

    private void Select(CategoryItem chosen)
    {
        _selected = chosen.Category;
        foreach (var category in Categories)
            category.IsSelected = category == chosen;

        var meal = Store.CurrentMeal!;
        ReplaceText = _selected == FoodCategory.Complement && meal.Complement is null
            ? "Choisir un complément"
            : $"Remplacer {FoodCatalog.CurrentArticle(meal, _selected)}";
    }

    private static string DoneLabel(Meal meal) => meal.Done ? "Repas mangé · annuler" : "Marquer comme mangé";

    [RelayCommand]
    private void ToggleDone()
    {
        var meal = Store.CurrentMeal;
        if (meal is null) return;
        Store.ToggleDone(meal);
        DoneText = DoneLabel(meal);
    }

    [RelayCommand]
    private async Task Replace()
    {
        Store.CurrentCategory = _selected;
        await Go("alternatives");
    }
}
