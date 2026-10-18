namespace NutriCam.Models;

/// <summary>
/// Un aliment (ou un groupe de légumes) proposé dans un repas ou comme remplacement.
/// Article : « le poulet », « les œufs »… (sert aux libellés « Remplacer le poulet »).
/// </summary>
public record FoodOption(
    string Label,
    string Article,
    string Hint,
    string Icon,
    string CookStep = "",
    bool Plural = false,
    int KcalPer100g = 0,
    int PortionG = 0)
{
    /// <summary>Énergie d'une portion standard, en kcal.</summary>
    public double PortionKcal => KcalPer100g * PortionG / 100.0;
}
