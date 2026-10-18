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
    public ActivityLevel Activity { get; set; } = ActivityLevel.Modere;
    public int MealsPerDay { get; set; } = 3;
}

/// <summary>Tout ce que l'application mémorise sur l'appareil (sérialisé en JSON).</summary>
public class AppData
{
    public UserAccount? Account { get; set; }
    public bool SignedIn { get; set; }
    public UserProfile Profile { get; set; } = new();
    public Goal Goal { get; set; } = Goal.Equilibrer;
    public List<string> Liked { get; set; } = new();
    public List<string> Allergies { get; set; } = new();
    public List<string> Avoided { get; set; } = new();
    public BudgetLevel Budget { get; set; } = BudgetLevel.Moyen;
    public List<string> Available { get; set; } = new() { "Riz", "Plantain", "Manioc", "Haricots", "Œufs", "Légumes" };
    public List<Meal> Program { get; set; } = new();
    public DateTime? StartDate { get; set; }

    /// <summary>Allergies et aliments évités : tout ce qui doit être exclu du programme.</summary>
    [JsonIgnore]
    public IEnumerable<string> Excluded => Allergies.Concat(Avoided);
}
