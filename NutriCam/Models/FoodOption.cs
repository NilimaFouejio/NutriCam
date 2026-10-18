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
    bool Plural = false);
