using System.Globalization;
using NutriCam.Api.Dtos;
using NutriCam.Data;
using NutriCam.Helpers;
using NutriCam.Models;

namespace NutriCam.Mapping;

/// <summary>
/// Seul endroit qui connaît à la fois les modèles de l'application et le contrat de l'API.
/// Si le contrat change, c'est ici (et dans Api/Dtos) qu'on adapte.
/// </summary>
public static class ApiMapper
{
    // ---------- Valeurs simples ----------
    public static string ToApi(Goal goal) => goal switch
    {
        Goal.Perdre => "perte_de_poids",
        Goal.Prendre => "prise_de_poids",
        _ => "maintien" // « Mieux équilibrer » n'existe pas dans la base : il est enregistré comme maintien
    };

    /// <summary>Retour serveur -> objectif local. Si le choix local « Équilibrer » correspond déjà à « maintien », on le garde.</summary>
    public static Goal GoalFromApi(string type, Goal? current)
    {
        var goal = type switch
        {
            "perte_de_poids" => Goal.Perdre,
            "prise_de_poids" => Goal.Prendre,
            _ => Goal.Maintenir
        };
        return goal == Goal.Maintenir && current == Goal.Equilibrer ? Goal.Equilibrer : goal;
    }

    private static string ToApi(ActivityLevel level) => level switch
    {
        ActivityLevel.Faible => "faible",
        ActivityLevel.Eleve => "eleve",
        _ => "modere"
    };

    private static ActivityLevel ActivityFromApi(string? value) => value switch
    {
        "faible" => ActivityLevel.Faible,
        "eleve" => ActivityLevel.Eleve,
        _ => ActivityLevel.Modere
    };

    private static string ToApi(BudgetLevel budget) => budget switch
    {
        BudgetLevel.Economique => "economique",
        BudgetLevel.Confort => "confort",
        _ => "moyen"
    };

    private static BudgetLevel BudgetFromApi(string? value) => value switch
    {
        "economique" => BudgetLevel.Economique,
        "confort" => BudgetLevel.Confort,
        _ => BudgetLevel.Moyen
    };

    private static string MomentFromApi(string type) => type switch
    {
        "petit_dejeuner" => "Petit-déjeuner",
        "diner" => "Dîner",
        "collation" => "Collation",
        _ => "Déjeuner"
    };

    // ---------- Aliments : libellé local <-> id du catalogue ----------
    private static AlimentDto? Find(string label, CatalogSnapshot catalog)
        => catalog.Aliments.FirstOrDefault(a => TextUtil.SameFood(a.NomAliment, label))
           ?? catalog.Aliments.FirstOrDefault(a => TextUtil.Mentions(a.NomAliment, label));

    private static AlimentRefDto Ref(string label, CatalogSnapshot catalog) => new(Find(label, catalog)?.IdAliment, label);

    private static EvitementDto Avoid(string label, string motif, CatalogSnapshot catalog) => new(Find(label, catalog)?.IdAliment, label, motif);

    private static string NameOf(string? id, string? name, CatalogSnapshot catalog)
        => !string.IsNullOrWhiteSpace(name)
            ? name!
            : catalog.Aliments.FirstOrDefault(a => a.IdAliment == id)?.NomAliment ?? "";

    // ---------- Application -> API ----------
    /// <summary>Profil, objectif et critères en un seul objet. Les conditions de santé ne sont volontairement PAS envoyées.</summary>
    public static ProfilCompletDto ToProfilComplet(AppData data, CatalogSnapshot catalog)
    {
        var p = data.Profile;
        var profil = new ProfilDto(
            data.Remote.ProfileId,
            p.Age ?? throw new InvalidOperationException("Âge manquant"),
            p.HeightCm ?? throw new InvalidOperationException("Taille manquante"),
            p.WeightKg ?? throw new InvalidOperationException("Poids manquant"),
            ToApi(p.Activity ?? ActivityLevel.Modere),
            p.Sex is null ? null : p.Sex == Sex.Homme ? "homme" : "femme",
            p.MealsPerDay);

        var objectif = new ObjectifDto(data.Remote.ObjectiveId, ToApi(data.Goal ?? Goal.Equilibrer));

        var avoided = data.Allergies.Select(a => Avoid(a, ApiValues.MotifAllergie, catalog))
            .Concat(data.Avoided.Select(a => Avoid(a, ApiValues.MotifPreference, catalog)))
            .ToList();

        var criteres = new CriteresDto(
            ToApi(data.Budget ?? BudgetLevel.Moyen),
            data.Liked.Select(l => Ref(l, catalog)).ToList(),
            data.Available.Select(a => Ref(a, catalog)).ToList(),
            avoided);

        return new ProfilCompletDto(profil, objectif, criteres);
    }

    // ---------- API -> application ----------
    /// <summary>Restaure sur l'appareil un profil enregistré sur le serveur (nouvel appareil, réinstallation).</summary>
    public static void ApplyProfil(AppData data, ProfilCompletDto dto, CatalogSnapshot catalog)
    {
        var p = data.Profile;
        p.Age = dto.Profil.Age;
        p.HeightCm = dto.Profil.TailleCm;
        p.WeightKg = dto.Profil.PoidsActuelKg;
        p.Activity = ActivityFromApi(dto.Profil.NiveauActivite);
        p.Sex = dto.Profil.Sexe switch { "homme" => Sex.Homme, "femme" => Sex.Femme, _ => null };
        p.MealsPerDay = dto.Profil.RepasParJour ?? 3;
        p.HealthChecked = true; // le serveur n'enregistre un profil qu'après la question de santé

        data.Goal = GoalFromApi(dto.Objectif.TypeObjectif, data.Goal);
        data.Budget = BudgetFromApi(dto.Criteres.Budget);
        data.Liked = (dto.Criteres.AlimentsPreferes ?? new()).Select(r => NameOf(r.IdAliment, r.NomAliment, catalog)).Where(n => n != "").ToList();
        data.Available = (dto.Criteres.AlimentsDisponibles ?? new()).Select(r => NameOf(r.IdAliment, r.NomAliment, catalog)).Where(n => n != "").ToList();

        var avoided = dto.Criteres.AlimentsAEviter ?? new();
        bool Strict(EvitementDto e) => e.Motif is ApiValues.MotifAllergie or ApiValues.MotifIntolerance;
        data.Allergies = avoided.Where(Strict).Select(e => NameOf(e.IdAliment, e.NomAliment, catalog)).Where(n => n != "").ToList();
        data.Avoided = avoided.Where(e => !Strict(e)).Select(e => NameOf(e.IdAliment, e.NomAliment, catalog)).Where(n => n != "").ToList();

        data.Remote.ProfileId = dto.Profil.IdProfil;
        data.Remote.ObjectiveId = dto.Objectif.IdObjectif;
    }

    public static List<Meal> ToMeals(ProgrammeDto program)
    {
        var meals = new List<Meal>();
        foreach (var r in (program.Repas ?? new()).OrderBy(r => r.NumeroJour))
        {
            var ingredients = r.Recette.Ingredients ?? new();
            var meal = new Meal
            {
                RemoteId = r.IdRepasProgramme,
                RecipeId = r.Recette.IdRecette,
                DayIndex = Math.Clamp(r.NumeroJour, 1, 7) - 1,
                Moment = MomentFromApi(r.TypeRepas),
                Name = r.Recette.NomRecette,
                Accompaniment = "légumes",
                Vegetables = ingredients.Where(i => i.Categorie == ApiValues.CategorieLegume).Select(i => i.NomAliment).ToList(),
                Protein = ingredients.FirstOrDefault(i => i.Categorie == ApiValues.CategorieProteine)?.NomAliment ?? "",
                Complement = ingredients.FirstOrDefault(i => i.Categorie == ApiValues.CategorieFeculent)?.NomAliment,
                Done = r.EstRealise,
                PortionFactor = 1,
                Kcal = (int)Math.Round(r.Recette.ValeurEnergetiqueTotaleKcal * r.NombrePortions)
            };
            meal.Description = !string.IsNullOrWhiteSpace(r.Recette.Description)
                ? r.Recette.Description!
                : meal.Vegetables.Count > 0 ? Services.MenuGenerator.BuildDescription(meal.Vegetables) : "";
            meal.Preparation = !string.IsNullOrWhiteSpace(r.Recette.Preparation)
                ? r.Recette.Preparation!
                : Services.MenuGenerator.BuildPreparation(meal);
            meals.Add(meal);
        }
        return meals;
    }

    public static DateTime ParseDate(string? value)
        => DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : DateTime.Today;
}
