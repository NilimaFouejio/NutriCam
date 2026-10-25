using System.Text.Json.Serialization;

namespace NutriCam.Api.Dtos;

// Objets échangés avec l'API. Ils reproduisent docs/api/nutricam-openapi.yaml :
// les noms sont convertis en snake_case à l'envoi (IdProgrammeCourant -> id_programme_courant).
// Aucun autre code de l'application ne dépend du format JSON : seul le mapping (Mapping/ApiMapper.cs) les connaît.

/// <summary>Valeurs fixes du contrat.</summary>
public static class ApiValues
{
    public const string CategorieLegume = "legume";
    public const string CategorieProteine = "proteine";
    public const string CategorieFeculent = "feculent";

    public const string MotifPreference = "preference";
    public const string MotifAllergie = "allergie";
    public const string MotifIntolerance = "intolerance";
}

// --- Auth ---
public record InscriptionRequest(string Nom, string Email, string MotDePasse);
public record ConnexionRequest(string Email, string MotDePasse);
public record UtilisateurDto(string IdUtilisateur, string Nom, string Email);
public record SessionDto(string Jeton, DateTime? ExpireLe, UtilisateurDto Utilisateur);

// --- Profil, objectif, critères ---
public record ProfilDto(
    string? IdProfil, int Age, double TailleCm, double PoidsActuelKg,
    string? NiveauActivite, string? Sexe, int? RepasParJour);

public record ObjectifDto(string? IdObjectif, string TypeObjectif);

public record AlimentRefDto(string? IdAliment, string? NomAliment);

public record EvitementDto(string? IdAliment, string? NomAliment, string? Motif);

public record CriteresDto(
    string Budget,
    List<AlimentRefDto>? AlimentsPreferes,
    List<AlimentRefDto>? AlimentsDisponibles,
    [property: JsonPropertyName("aliments_a_eviter")] List<EvitementDto>? AlimentsAEviter);

public record ProfilCompletDto(
    ProfilDto Profil, ObjectifDto Objectif, CriteresDto Criteres,
    string? IdProgrammeCourant = null, List<string>? Avertissements = null);

// --- Catalogue ---
public record AlimentDto(
    string IdAliment, string NomAliment, string? Categorie,
    [property: JsonPropertyName("energie_kcal_100g")] double? EnergieKcal100g,
    [property: JsonPropertyName("proteines_100g")] double? Proteines100g,
    [property: JsonPropertyName("glucides_100g")] double? Glucides100g,
    [property: JsonPropertyName("lipides_100g")] double? Lipides100g);

public record ActiviteDto(
    string IdActivite, string NomActivite, string? Description, string? Niveau, string? ObjectifAssocie);

public record RecommandationActiviteDto(
    string IdRecommandation, ActiviteDto Activite, int? FrequenceHebdomadaire);

// --- Programme ---
public record IngredientDto(
    string IdAliment, string NomAliment, string? Categorie,
    double Quantite, string Unite, double ApportEnergetiqueKcal);

public record RecetteDto(
    string IdRecette, string NomRecette, string? Description, string? Preparation,
    double ValeurEnergetiqueTotaleKcal, List<IngredientDto>? Ingredients);

public record RepasProgrammeDto(
    string IdRepasProgramme, int NumeroJour, string TypeRepas,
    double NombrePortions, bool EstRealise, RecetteDto Recette);

public record ProgrammeDto(
    string IdProgramme, string? IdObjectif, string DateDebut,
    List<RepasProgrammeDto>? Repas, List<string>? Avertissements);

public record CreerProgrammeRequest(string DateDebut, bool Regenerer);

public record MajRepasRequest(bool EstRealise);

// --- Erreurs ---
public record ErreurDetailDto(string? Champ, string? Message);
public record ErreurDto(string Code, string Message, List<ErreurDetailDto>? Details);

/// <summary>Réponse d'une lecture conditionnelle (ETag) : NotModified = le cache local est à jour.</summary>
public record Conditional<T>(T? Value, string? ETag, bool NotModified);
