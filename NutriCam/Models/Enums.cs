namespace NutriCam.Models;

public enum FoodCategory { Legumes, Proteines, Complement }

public enum Goal { Perdre, Maintenir, Prendre, Equilibrer }

/// <summary>
/// Les 4 profils d'activité du tableau « 5.2 Estimer la dépense énergétique totale ».
/// L'ordre (0 à 3) est utilisé comme index : ne pas le modifier.
/// </summary>
public enum ActivityLevel { TresSedentaire, LegerementActif, ModerementActif, TresActif }

public enum BudgetLevel { Economique, Moyen, Confort }

public enum Sex { Femme, Homme }
