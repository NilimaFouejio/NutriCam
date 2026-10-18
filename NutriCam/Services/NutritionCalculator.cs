using NutriCam.Helpers;
using NutriCam.Models;

namespace NutriCam.Services;

/// <summary>
/// Estimation du besoin énergétique quotidien (formule de Mifflin-St Jeor) et de la valeur énergétique de chaque repas.
/// Ce sont des estimations moyennes, qui ne remplacent pas un calcul fait par un professionnel de santé.
/// </summary>
public static class NutritionCalculator
{
    /// <summary>Huile et assaisonnement d'un repas cuisiné (environ une demi-cuillère à soupe d'huile).</summary>
    private const double SeasoningKcal = 90;

    /// <summary>Quantité de légumes d'une assiette (½ de l'assiette), en grammes.</summary>
    private const double VegetablePortionG = 250;

    // ---------- Besoin quotidien ----------

    public static double Bmi(UserProfile p)
    {
        var meters = (p.HeightCm ?? 170) / 100.0;
        return (p.WeightKg ?? 65) / (meters * meters);
    }

    /// <summary>Une réduction des apports n'est pas proposée aux mineurs ni aux personnes déjà en insuffisance pondérale.</summary>
    private static bool ReductionAllowed(UserProfile p) => (p.Age ?? 30) >= 18 && Bmi(p) >= 18.5;

    public static int DailyTarget(AppData data)
    {
        var p = data.Profile;
        var weight = p.WeightKg ?? 65;
        var height = p.HeightCm ?? 170;
        var age = p.Age ?? 30;

        var sexConstant = p.Sex switch { Sex.Homme => 5.0, Sex.Femme => -161.0, _ => -78.0 };
        var metabolism = 10 * weight + 6.25 * height - 5 * age + sexConstant;
        var activity = p.Activity switch { ActivityLevel.Faible => 1.375, ActivityLevel.Eleve => 1.725, _ => 1.55 };
        var need = metabolism * activity;

        var adjustment = data.Goal switch
        {
            Goal.Perdre => ReductionAllowed(p) ? 0.85 : 1.0,
            Goal.Prendre => 1.12,
            _ => 1.0
        };

        var floor = p.Sex == Sex.Homme ? 1500 : 1200;
        var target = Math.Max(need * adjustment, floor);
        return (int)(Math.Round(target / 50) * 50);
    }

    /// <summary>Explication à afficher quand l'objectif a dû être adapté pour rester prudent.</summary>
    public static string TargetNote(AppData data)
    {
        if (data.Goal != Goal.Perdre || ReductionAllowed(data.Profile)) return "";
        return (data.Profile.Age ?? 30) < 18
            ? "À votre âge, nous ne proposons pas de réduction des apports : votre corps est encore en croissance."
            : "Votre poids est déjà bas : nous ne proposons pas de réduction des apports.";
    }

    // ---------- Énergie des repas ----------

    // kcal pour 100 g, et grammes fixes pour les ingrédients consommés en petite quantité.
    private static readonly Dictionary<string, (int Kcal100, int? FixedG)> Vegetables = BuildVegetables();

    private static Dictionary<string, (int, int?)> BuildVegetables()
    {
        var raw = new (string Name, int Kcal, int? Grams)[]
        {
            ("Concombre", 15, null), ("Tomate", 18, null), ("Laitue", 15, null), ("Salade", 15, null),
            ("Carottes", 41, null), ("Courgettes", 17, null), ("Brocoli", 34, null), ("Poivron", 26, null),
            ("Épinards", 23, null), ("Chou", 25, null), ("Haricots verts", 31, null), ("Gombo", 33, null),
            ("Aubergine", 25, null), ("Oignon", 40, 30), ("Olives", 115, 20), ("Fruit de saison", 55, 120),
        };
        return raw.ToDictionary(r => Key(r.Name), r => (r.Kcal, r.Grams));
    }

    private static string Key(string name) => name.Trim().ToLowerInvariant().TrimEnd('s');

    /// <summary>Énergie des légumes (et de l'avocat éventuel) d'un repas, quelle que soit la taille des portions.</summary>
    private static double VegetablesKcal(Meal meal)
    {
        var items = meal.Vegetables.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v.Trim()).ToList();
        var known = items.Select(v => Vegetables.TryGetValue(Key(v), out var info) ? info : (Kcal100: 30, FixedG: (int?)null)).ToList();

        var fixedGrams = known.Where(k => k.FixedG is not null).Sum(k => k.FixedG!.Value);
        var generic = known.Count(k => k.FixedG is null);
        var genericGrams = generic == 0 ? 0 : Math.Max(0, VegetablePortionG - fixedGrams) / generic;

        var kcal = known.Sum(k => k.Kcal100 * (k.FixedG ?? genericGrams) / 100.0);

        // L'avocat n'est pas dans la liste des légumes : il figure dans l'accompagnement.
        if (meal.Accompaniment.Contains("avocat", StringComparison.OrdinalIgnoreCase)) kcal += 112;
        return kcal;
    }

    private static double MomentFactor(Meal meal) => meal.Moment == "Collation" ? 0.5 : 1.0;

    private static double ProteinKcal(Meal meal)
        => (FoodCatalog.Find(FoodCategory.Proteines, meal.Protein)?.PortionKcal ?? 0) * MomentFactor(meal);

    private static double ComplementKcal(Meal meal)
        => (FoodCatalog.Find(FoodCategory.Complement, meal.Complement)?.PortionKcal ?? 0) * MomentFactor(meal);

    /// <summary>Partie fixe (légumes, huile) et partie ajustable (protéines, féculents) d'un repas, à portion standard.</summary>
    private static (double Fixed, double Scalable) Parts(Meal meal)
    {
        var seasoning = meal.Moment == "Collation" ? 0 : SeasoningKcal;
        return (VegetablesKcal(meal) + seasoning, ProteinKcal(meal) + ComplementKcal(meal));
    }

    private static int Round(double kcal) => (int)(Math.Round(kcal / 10) * 10);

    /// <summary>Taille des portions pour que la journée des repas donnés corresponde au besoin estimé.</summary>
    public static double CalibrateFactor(IReadOnlyCollection<Meal> meals, int dailyTarget)
    {
        if (meals.Count == 0) return 1;
        var days = Math.Max(1, meals.Select(m => m.DayIndex).Distinct().Count());
        var fixedPerDay = meals.Sum(m => Parts(m).Fixed) / days;
        var scalablePerDay = meals.Sum(m => Parts(m).Scalable) / days;
        if (scalablePerDay <= 0) return 1;
        return Math.Clamp((dailyTarget - fixedPerDay) / scalablePerDay, 0.6, 1.5);
    }

    public static void Apply(Meal meal, double factor)
    {
        meal.PortionFactor = factor;
        var (fixedPart, scalable) = Parts(meal);
        meal.Kcal = Round(fixedPart + factor * scalable);
    }

    /// <summary>Recalcule le repas après un remplacement d'aliment (même taille de portion).</summary>
    public static void Recompute(Meal meal) => Apply(meal, meal.PortionFactor);

    /// <summary>
    /// Calcule l'énergie de tous les repas d'un programme. Les portions sont ajustées jour par jour :
    /// une journée riche en féculents reçoit des portions plus petites, une journée légère des portions plus grandes.
    /// </summary>
    public static void ApplyToProgram(IReadOnlyCollection<Meal> meals, int dailyTarget)
    {
        foreach (var day in meals.GroupBy(m => m.DayIndex))
        {
            var dayMeals = day.ToList();
            var factor = CalibrateFactor(dayMeals, dailyTarget);
            foreach (var meal in dayMeals) Apply(meal, factor);
        }
    }

    /// <summary>Énergie par composante du repas : légumes, protéines, complément (kcal).</summary>
    public static (int Vegetables, int Protein, int Complement) Breakdown(Meal meal)
    {
        var seasoning = meal.Moment == "Collation" ? 0 : SeasoningKcal;
        return (Round(VegetablesKcal(meal) + seasoning),
                Round(ProteinKcal(meal) * meal.PortionFactor),
                Round(ComplementKcal(meal) * meal.PortionFactor));
    }

    /// <summary>Énergie d'un aliment proposé en remplacement, pour le repas concerné.</summary>
    public static int OptionKcal(FoodOption option, FoodCategory category, Meal meal)
    {
        if (category == FoodCategory.Legumes)
        {
            var test = new Meal { Moment = meal.Moment, Accompaniment = "légumes", Vegetables = option.Label.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList() };
            return Round(VegetablesKcal(test) + (meal.Moment == "Collation" ? 0 : SeasoningKcal));
        }
        return Round(option.PortionKcal * MomentFactor(meal) * meal.PortionFactor);
    }

    /// <summary>Énergie totale des repas d'une journée.</summary>
    public static int DayKcal(IEnumerable<Meal> meals) => meals.Sum(m => m.Kcal);
}
