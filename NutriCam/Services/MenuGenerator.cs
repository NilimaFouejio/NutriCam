using NutriCam.Helpers;
using NutriCam.Models;

namespace NutriCam.Services;

/// <summary>
/// Génération locale du programme de 7 jours.
/// Elle tient compte des aliments évités, des goûts, des aliments disponibles, du budget et du nombre de repas.
/// Elle est volontairement isolée ici : on pourra la remplacer par un appel à l'API du cahier des charges technique.
/// </summary>
public static class MenuGenerator
{
    private record Template(
        string Moment, string Name, string Accompaniment, string[] Vegetables,
        string Protein, string? Complement, int Cost, string? Preparation = null);

    private static readonly Template[] Templates =
    {
        // Petit-déjeuner
        new("Petit-déjeuner", "Salade d’œuf & avocat", "avocat", new[] { "Concombre", "tomate", "olives" }, "Œufs", null, 2,
            "Écalez les œufs durs, coupez l’avocat et les légumes, puis assaisonnez d’un filet d’huile et de citron."),
        new("Petit-déjeuner", "Omelette aux légumes", "légumes", new[] { "Tomate", "oignon", "poivron" }, "Œufs", "Plantain", 1,
            "Battez les œufs, ajoutez les légumes émincés et cuisez à la poêle. Servez avec du plantain."),
        new("Petit-déjeuner", "Haricots & plantain", "légumes", new[] { "Tomate", "oignon" }, "Haricots", "Plantain", 1,
            "Faites mijoter les haricots avec la tomate et l’oignon, puis servez avec du plantain."),
        new("Petit-déjeuner", "Poisson & crudités", "crudités", new[] { "Concombre", "tomate", "laitue" }, "Poisson", null, 3,
            "Effeuillez le poisson cuit, ajoutez les crudités et assaisonnez légèrement."),

        // Déjeuner
        new("Déjeuner", "Poulet grillé & légumes", "légumes", new[] { "Carottes", "courgettes", "brocoli", "poivron" }, "Poulet grillé", null, 2,
            "Faites griller le poulet assaisonné. Poêlez les carottes, les courgettes, le brocoli et le poivron, puis servez-les ensemble."),
        new("Déjeuner", "Poisson & légumes mijotés", "légumes", new[] { "Épinards", "tomates", "oignon" }, "Poisson", "Riz", 3,
            "Faites griller le poisson. Laissez mijoter les légumes à l’huile et servez avec du riz."),
        new("Déjeuner", "Haricots & légumes sautés", "légumes", new[] { "Chou", "carottes", "haricots verts" }, "Haricots", "Plantain", 1,
            "Faites mijoter les haricots. Sautez les légumes à la poêle et servez avec du plantain."),
        new("Déjeuner", "Œufs & légumes sautés", "légumes", new[] { "Chou", "carottes", "poivron" }, "Œufs", "Riz", 1,
            "Faites cuire les œufs à la poêle et sautez les légumes. Servez avec du riz."),

        // Dîner
        new("Dîner", "Filet grillé & salade croquante", "salade croquante", new[] { "Tomates", "salade", "brocoli" }, "Poulet grillé", null, 2,
            "Faites griller le filet. Préparez la salade et le brocoli vapeur, puis servez ensemble."),
        new("Dîner", "Poisson & légumes vapeur", "légumes", new[] { "Chou", "carottes", "haricots verts" }, "Poisson", "Igname", 3,
            "Cuisez le poisson à la vapeur avec les légumes. Servez avec de l’igname."),
        new("Dîner", "Soupe de haricots & légumes", "légumes", new[] { "Gombo", "aubergine", "tomates" }, "Haricots", null, 1,
            "Laissez mijoter les haricots avec les légumes jusqu’à obtenir une soupe épaisse."),
        new("Dîner", "Omelette & salade", "salade", new[] { "Concombre", "tomate", "laitue" }, "Œufs", null, 1,
            "Préparez une omelette nature et servez-la avec la salade."),

        // Collation
        new("Collation", "Plantain mûr & œuf dur", "fruit", new[] { "Fruit de saison" }, "Œufs", "Plantain", 1,
            "Faites cuire un œuf dur et grillez une tranche de plantain mûr."),
    };

    public static List<string> MomentsFor(int mealsPerDay) => mealsPerDay switch
    {
        <= 2 => new() { "Déjeuner", "Dîner" },
        3 => new() { "Petit-déjeuner", "Déjeuner", "Dîner" },
        4 => new() { "Petit-déjeuner", "Déjeuner", "Collation", "Dîner" },
        _ => new() { "Petit-déjeuner", "Collation", "Déjeuner", "Collation", "Dîner" },
    };

    public static List<Meal> Generate(AppData data, int seed)
    {
        var rng = new Random(seed);
        var moments = MomentsFor(data.Profile.MealsPerDay ?? 3);
        var budgetCost = (int)(data.Budget ?? BudgetLevel.Moyen) + 1; // 1 économique, 2 moyen, 3 confort
        var meals = new List<Meal>();
        var uses = new Dictionary<string, int>();
        var previous = new Dictionary<string, string>();

        for (var day = 0; day < 7; day++)
        {
            foreach (var moment in moments)
            {
                var pool = Templates.Where(t => t.Moment == moment).ToList();
                var compatible = pool.Where(t => !FoodCatalog.Conflicts(Ingredients(t), data.Excluded)).ToList();
                if (compatible.Count == 0) compatible = pool;

                var pick = compatible
                    .OrderByDescending(t => Score(t, data, budgetCost, uses, previous, rng))
                    .First();

                uses[pick.Name] = uses.GetValueOrDefault(pick.Name) + 1;
                previous[moment] = pick.Name;
                meals.Add(ToMeal(pick, day));
            }
        }
        // Valeur énergétique de chaque repas, avec des portions ajustées au besoin de la personne.
        NutritionCalculator.ApplyToProgram(meals, NutritionCalculator.DailyTarget(data));
        return meals;
    }

    private static IEnumerable<string> Ingredients(Template t)
        => t.Vegetables.Append(t.Protein).Append(t.Complement ?? "").Append(t.Accompaniment);

    private static double Score(Template t, AppData d, int budgetCost, Dictionary<string, int> uses,
        Dictionary<string, string> previous, Random rng)
    {
        double score = rng.NextDouble() * 0.5;
        if (d.Liked.Any(l => TextUtil.Mentions(t.Protein, l))) score += 2;
        if (t.Complement is not null && d.Liked.Any(l => TextUtil.Mentions(t.Complement, l))) score += 1;
        score += d.Available.Any(a => TextUtil.Mentions(t.Protein, a)) ? 1 : -2;
        score -= Math.Abs(t.Cost - budgetCost);
        score -= 2 * uses.GetValueOrDefault(t.Name);
        if (previous.TryGetValue(t.Moment, out var last) && last == t.Name) score -= 5;
        return score;
    }

    private static Meal ToMeal(Template t, int day)
    {
        var meal = new Meal
        {
            DayIndex = day,
            Moment = t.Moment,
            Name = t.Name,
            Accompaniment = t.Accompaniment,
            Vegetables = t.Vegetables.ToList(),
            Protein = t.Protein,
            Complement = t.Complement,
            Preparation = t.Preparation ?? ""
        };
        meal.Description = BuildDescription(meal.Vegetables);
        if (meal.Preparation == "") meal.Preparation = BuildPreparation(meal);
        return meal;
    }

    public static string BuildDescription(IEnumerable<string> vegetables)
        => $"{TextUtil.Capitalize(TextUtil.JoinEt(vegetables.Select((v, i) => i == 0 ? v : v.ToLowerInvariant())))} : une assiette colorée et savoureuse.";

    public static string BuildPreparation(Meal meal)
    {
        var cook = FoodCatalog.Find(FoodCategory.Proteines, meal.Protein)?.CookStep ?? "Préparez la protéine.";
        var veg = string.Join(", ", meal.Vegetables.Select(v => v.ToLowerInvariant()));
        return $"{cook} Préparez les légumes ({veg}) à votre convenance, puis servez le tout ensemble.";
    }
}
