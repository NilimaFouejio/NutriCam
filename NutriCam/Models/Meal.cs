using System.Text.Json.Serialization;
using NutriCam.Helpers;

namespace NutriCam.Models;

/// <summary>Un repas du programme : légumes (½ de l'assiette), protéine (¼) et complément féculent (¼).</summary>
public class Meal
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public int DayIndex { get; set; }
    public string Moment { get; set; } = "";
    public string Name { get; set; } = "";
    public string Accompaniment { get; set; } = "légumes";
    public List<string> Vegetables { get; set; } = new();
    public string Protein { get; set; } = "";
    public string? Complement { get; set; }
    public string Description { get; set; } = "";
    public string Preparation { get; set; } = "";
    public bool Done { get; set; }

    /// <summary>id_repas_programme côté serveur (null pour un repas généré hors ligne).</summary>
    public string? RemoteId { get; set; }

    /// <summary>id_recette côté serveur : sert à demander les alternatives.</summary>
    public string? RecipeId { get; set; }

    /// <summary>Vrai quand « mangé / pas mangé » n'a pas encore pu être envoyé au serveur.</summary>
    public bool DoneDirty { get; set; }

    /// <summary>Valeur énergétique estimée du repas, en kcal (0 = non calculée).</summary>
    public int Kcal { get; set; }

    /// <summary>Taille des portions de protéines et de féculents, ajustée au besoin énergétique de la personne (1 = portion standard).</summary>
    public double PortionFactor { get; set; } = 1;

    [JsonIgnore]
    public bool HasKcal => Kcal > 0;

    [JsonIgnore]
    public string KcalText => Kcal > 0 ? $"≈ {Kcal} kcal" : "";

    [JsonIgnore]
    public string Image => Moment switch
    {
        "Petit-déjeuner" => "petit_dejeuner.jpg",
        "Dîner" => "diner.jpg",
        _ => "dejeuner.jpg"
    };

    /// <summary>« Carottes, courgettes & brocoli » : les trois premiers légumes.</summary>
    [JsonIgnore]
    public string VegetablesSummary => TextUtil.JoinAmp(Vegetables.Take(3));
}
