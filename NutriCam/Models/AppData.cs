using System.Text.Json.Serialization;

namespace NutriCam.Models;

public class UserAccount
{
    public string FirstName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Salt { get; set; } = "";
    public string PasswordHash { get; set; } = "";
}

public class UserProfile
{
    public int? Age { get; set; }
    public double? HeightCm { get; set; }
    public double? WeightKg { get; set; }
    // Aucune valeur n'est choisie d'avance : null tant que la personne n'a pas répondu.
    public ActivityLevel? Activity { get; set; }
    public int? MealsPerDay { get; set; }

    /// <summary>Sexe, utilisé pour estimer le besoin énergétique quotidien.</summary>
    public Sex? Sex { get; set; }

    /// <summary>Maladies ou situations déclarées qui nécessitent un suivi médical (aucun programme n'est alors généré).</summary>
    public List<string> Conditions { get; set; } = new();

    /// <summary>Vrai quand la personne a répondu à la question sur sa santé.</summary>
    public bool HealthChecked { get; set; }
}

/// <summary>Identifiants attribués par le serveur (UUID) : ils relient les données locales aux lignes de la base.</summary>
public class RemoteIds
{
    public string? UserId { get; set; }
    public string? ProfileId { get; set; }
    public string? ObjectiveId { get; set; }
    public string? ProgramId { get; set; }

    /// <summary>Clé d'idempotence de la création de programme en cours : réutilisée tant qu'elle n'a pas abouti, pour ne jamais créer de doublon.</summary>
    public string? PendingProgramKey { get; set; }
}

/// <summary>Tout ce que l'application mémorise sur l'appareil (sérialisé en JSON).</summary>
public class AppData
{
    public UserAccount? Account { get; set; }
    public bool SignedIn { get; set; }
    public UserProfile Profile { get; set; } = new();
    public Goal? Goal { get; set; }
    public List<string> Liked { get; set; } = new();
    public List<string> Allergies { get; set; } = new();
    public List<string> Avoided { get; set; } = new();
    public BudgetLevel? Budget { get; set; }
    public List<string> Available { get; set; } = new();
    public List<Meal> Program { get; set; } = new();
    public DateTime? StartDate { get; set; }
    public RemoteIds Remote { get; set; } = new();

    /// <summary>Allergies et aliments évités : tout ce qui doit être exclu du programme.</summary>
    [JsonIgnore]
    public IEnumerable<string> Excluded => Allergies.Concat(Avoided);
}
