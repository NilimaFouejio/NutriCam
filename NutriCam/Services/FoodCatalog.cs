using NutriCam.Helpers;
using NutriCam.Models;

namespace NutriCam.Services;

/// <summary>Aliments proposés dans l'application (contexte camerounais) et règles de compatibilité.</summary>
public static class FoodCatalog
{
    public static readonly FoodOption[] Proteins =
    {
        new("Poulet grillé", "le poulet", "Grillé et bien assaisonné", Icons.Drumstick, "Faites griller le poulet assaisonné."),
        new("Poisson", "le poisson", "À griller, selon ce que vous trouvez", Icons.Fish, "Faites griller le poisson assaisonné."),
        new("Œufs", "les œufs", "Durs ou cuits à la poêle", Icons.Egg, "Faites cuire les œufs, durs ou à la poêle.", true),
        new("Haricots", "les haricots", "À servir avec les légumes", Icons.Bean, "Faites mijoter les haricots avec un peu d’oignon.", true),
    };

    public static readonly FoodOption[] Vegetables =
    {
        new("Carottes, courgettes, brocoli, poivron", "ce choix de légumes", "Poêlés ensemble", Icons.Carrot),
        new("Épinards, tomates, oignon", "ce choix de légumes", "Revenus à l’huile", Icons.Carrot),
        new("Chou, carottes, haricots verts", "ce choix de légumes", "Sautés ou cuits à la vapeur", Icons.Carrot),
        new("Gombo, aubergine, tomates", "ce choix de légumes", "Mijotés doucement", Icons.Carrot),
        new("Concombre, tomate, laitue", "ce choix de légumes", "Crus, en salade", Icons.Carrot),
    };

    public static readonly FoodOption[] Complements =
    {
        new("Riz", "le riz", "Nature ou parfumé", Icons.Wheat),
        new("Plantain", "le plantain", "Mûr ou vert, bouilli ou grillé", Icons.Wheat),
        new("Manioc", "le manioc", "En bâton ou en couscous", Icons.Wheat),
        new("Igname", "l’igname", "Bouillie ou en purée", Icons.Wheat),
        new("Patate douce", "la patate douce", "Cuite à la vapeur", Icons.Wheat),
    };

    public static FoodOption[] For(FoodCategory category) => category switch
    {
        FoodCategory.Proteines => Proteins,
        FoodCategory.Legumes => Vegetables,
        _ => Complements
    };

    public static FoodOption? Find(FoodCategory category, string? label)
        => label is null ? null : For(category).FirstOrDefault(o => TextUtil.SameFood(o.Label, label));

    /// <summary>L'élément du repas qui correspond à la catégorie (sa valeur actuelle).</summary>
    public static string? CurrentLabel(Meal meal, FoodCategory category) => category switch
    {
        FoodCategory.Legumes => string.Join(", ", meal.Vegetables),
        FoodCategory.Proteines => meal.Protein,
        _ => meal.Complement
    };

    /// <summary>« le poulet », « les légumes », « le complément »… utilisé dans « Remplacer le poulet ».</summary>
    public static string CurrentArticle(Meal meal, FoodCategory category)
    {
        switch (category)
        {
            case FoodCategory.Legumes:
                return "les légumes";
            case FoodCategory.Proteines:
                return Find(category, meal.Protein)?.Article ?? "la protéine";
            default:
                return meal.Complement is null ? "le complément" : Find(category, meal.Complement)?.Article ?? "le complément";
        }
    }

    /// <summary>Vrai si l'un des ingrédients contient un aliment à éviter.</summary>
    public static bool Conflicts(IEnumerable<string> ingredients, IEnumerable<string> avoided)
    {
        var list = ingredients.Where(i => !string.IsNullOrWhiteSpace(i)).ToList();
        return avoided.Any(a => list.Any(i => TextUtil.Mentions(i, a)));
    }
}
