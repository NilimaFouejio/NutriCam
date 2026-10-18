using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriCam.Helpers;
using NutriCam.Models;
using NutriCam.Services;

namespace NutriCam.ViewModels;

/// <summary>Écran « Remplacer … » : alternatives de la même catégorie alimentaire, hors aliments évités.</summary>
public partial class AlternativesViewModel : BaseViewModel
{
    private Meal? _meal;
    private FoodCategory _category;

    public AlternativesViewModel(ProgramStore store) : base(store) { }

    public ObservableCollection<OptionItem> Options { get; } = new();

    [ObservableProperty] private string title = "";
    [ObservableProperty] private string subtitle = "";
    [ObservableProperty] private string image = "dejeuner.jpg";
    [ObservableProperty] private string momentLine = "";
    [ObservableProperty] private string currentLabel = "";
    [ObservableProperty] private string categoryLine = "";
    [ObservableProperty] private string helper = "";
    [ObservableProperty] private bool hasOptions;
    [ObservableProperty] private string emptyMessage = "";

    public string ConfirmText => "Utiliser ce remplacement";

    public override void OnAppearing()
    {
        _meal = Store.CurrentMeal;
        _category = Store.CurrentCategory;
        if (_meal is null) return;

        var current = FoodCatalog.CurrentLabel(_meal, _category) ?? "";
        var article = FoodCatalog.CurrentArticle(_meal, _category);

        Title = _category == FoodCategory.Complement && _meal.Complement is null
            ? "Choisir un complément"
            : $"Remplacer {article}";
        Subtitle = _category switch
        {
            FoodCategory.Proteines => "Gardez une source de protéines, avec un aliment qui vous convient.",
            FoodCategory.Legumes => "Gardez des légumes, avec un choix qui vous convient.",
            _ => "Gardez un féculent, avec un aliment qui vous convient."
        };
        Image = _meal.Image;
        MomentLine = $"{Store.DayName(_meal.DayIndex)} · {_meal.Moment.ToLowerInvariant()}";
        CurrentLabel = current.Length > 0 ? current : "Aucun complément pour l'instant";
        CategoryLine = _category switch
        {
            FoodCategory.Proteines => "Protéines · repère ¼",
            FoodCategory.Legumes => "Légumes · repère ½",
            _ => "Complément · repère ¼"
        };

        // Même catégorie, différent de l'actuel, sans aliment évité ; trois propositions au plus.
        var choices = FoodCatalog.For(_category)
            .Where(o => !TextUtil.SameFood(o.Label, current))
            .Where(o => !FoodCatalog.Conflicts(new[] { o.Label }, Store.Data.Excluded))
            .Take(3)
            .ToList();

        Options.Clear();
        foreach (var choice in choices)
        {
            var kcal = NutritionCalculator.OptionKcal(choice, _category, _meal);
            Options.Add(new OptionItem(choice.Label, choice.Label, $"{choice.Hint} · ≈ {kcal} kcal", choice.Icon, Select));
        }

        HasOptions = Options.Count > 0;
        EmptyMessage = HasOptions ? "" : "Aucune alternative ne correspond à vos préférences pour le moment.";
        if (HasOptions) Select(Options[0]);
    }

    private void Select(OptionItem chosen)
    {
        foreach (var option in Options)
            option.IsSelected = option == chosen;

        var next = FoodCatalog.Find(_category, chosen.Key)!;
        var verb = next.Plural ? "remplaceront" : "remplacera";
        var currentArticle = FoodCatalog.CurrentArticle(_meal!, _category);
        Helper = $"{TextUtil.Capitalize(next.Article)} {verb} {currentArticle} dans {MomentArticle(_meal!.Moment)} du {Store.DayName(_meal.DayIndex).ToLowerInvariant()}.";
    }

    private static string MomentArticle(string moment) => moment switch
    {
        "Petit-déjeuner" => "le petit-déjeuner",
        "Dîner" => "le dîner",
        "Collation" => "la collation",
        _ => "le déjeuner"
    };

    [RelayCommand]
    private async Task Confirm()
    {
        var chosen = Options.FirstOrDefault(o => o.IsSelected);
        if (chosen is null || _meal is null) return;

        Store.Replace(_meal, _category, FoodCatalog.Find(_category, chosen.Key)!);
        await Go(".."); // retour au détail du repas, qui affiche la confirmation
    }
}
