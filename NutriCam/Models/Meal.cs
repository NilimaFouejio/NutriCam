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
