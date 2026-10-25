using System.Text.Json;
using NutriCam.Api.Dtos;
using NutriCam.Helpers;
using NutriCam.Mapping;
using NutriCam.Models;

namespace NutriCam.Services;

/// <summary>
/// État partagé de l'application : compte, profil, préférences et programme.
/// Les données sont conservées sur l'appareil (fichier JSON local) en attendant l'API du cahier des charges technique.
/// </summary>
public class ProgramStore
{
    private const string StorageKey = "nutricam.data.v1";
    private static readonly string[] DayNames = { "Dimanche", "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi", "Samedi" };
    private static readonly string[] DayShorts = { "Dim", "Lun", "Mar", "Mer", "Jeu", "Ven", "Sam" };

    public ProgramStore()
    {
        Data = Load();
        EnsureEnergy();
    }

    public AppData Data { get; private set; }

    // --- Navigation : éléments sélectionnés d'un écran à l'autre ---
    public Meal? CurrentMeal { get; set; }
    public FoodCategory CurrentCategory { get; set; } = FoodCategory.Proteines;
    public string LastChange { get; set; } = "";

    /// <summary>Vrai quand le parcours « Votre programme » est relancé depuis l'onglet Profil.</summary>
    public bool EditMode { get; set; }

    /// <summary>Vrai quand une étape est rouverte depuis le récapitulatif : elle y revient au lieu de poursuivre le parcours.</summary>
    public bool ReturnToRecap { get; set; }

    /// <summary>Vrai si la personne a déclaré une maladie ou une situation qui exige un suivi médical : aucun programme n'est proposé.</summary>
    public bool IsBlocked => Data.Profile.Conditions.Count > 0;

    /// <summary>Retire tout programme existant : un suivi médical remplace NutriCam dans cette situation.</summary>
    public void BlockProgram()
    {
        Data.Program = new List<Meal>();
        Data.StartDate = null;
        CurrentMeal = null;
        EditMode = false;
        ReturnToRecap = false;
        Save();
    }

    /// <summary>Programmes enregistrés avant l'ajout de la valeur énergétique : on la calcule au démarrage.</summary>
    private void EnsureEnergy()
    {
        if (Data.Program.Count == 0 || Data.Program.All(m => m.Kcal > 0)) return;
        NutritionCalculator.ApplyToProgram(Data.Program, NutritionCalculator.DailyTarget(Data));
        Save();
    }

    public bool HasProgram => Data.Program.Count > 0 && Data.StartDate is not null;

    // --- Persistance ---
    // Fichier JSON dans le dossier de données de l'application (identique sur toutes les plateformes).
    // Les Preferences sont limitées à ~8 Ko par valeur sous Windows : trop juste pour un programme de 7 jours.
    private static string FilePath => Path.Combine(FileSystem.AppDataDirectory, "nutricam-data.json");

    private static AppData Load()
    {
        try
        {
            if (File.Exists(FilePath))
                return JsonSerializer.Deserialize<AppData>(File.ReadAllText(FilePath)) ?? new AppData();

            // Migration : anciennes versions qui stockaient les données dans les Preferences.
            var legacy = Preferences.Default.Get(StorageKey, "");
            if (!string.IsNullOrEmpty(legacy))
            {
                var data = JsonSerializer.Deserialize<AppData>(legacy) ?? new AppData();
                Preferences.Default.Remove(StorageKey);
                File.WriteAllText(FilePath, legacy);
                return data;
            }
        }
        catch
        {
            // Données illisibles : on repart de zéro plutôt que de planter.
        }
        return new AppData();
    }

    public void Save()
    {
        try
        {
            // Écriture dans un fichier temporaire puis remplacement : évite un fichier tronqué si l'app se ferme en cours d'écriture.
            var tmp = FilePath + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(Data));
            File.Move(tmp, FilePath, overwrite: true);
        }
        catch
        {
            // Disque plein ou accès refusé : on ne fait pas planter l'interface.
        }
    }

    public void ResetPersonalData()
    {
        Data = new AppData();
        CurrentMeal = null;
        Save();
    }

    // --- Programme ---
    public void GenerateProgram()
    {
        if (IsBlocked) return; // sécurité : aucun programme pour une personne qui doit consulter un médecin
        Data.Program = MenuGenerator.Generate(Data, Environment.TickCount);
        Data.StartDate = DateTime.Today;
        Save();
    }

    /// <summary>
    /// Remplace le programme local par celui renvoyé par l'API (création ou restauration sur un nouvel appareil).
    /// Les repas, leurs identifiants serveur, la date de début et l'identifiant du programme sont conservés ensemble.
    /// </summary>
    public void ApplyRemoteProgram(ProgrammeDto program)
    {
        Data.Program = ApiMapper.ToMeals(program);
        Data.StartDate = ApiMapper.ParseDate(program.DateDebut);
        Data.Remote.ProgramId = program.IdProgramme;
        CurrentMeal = null;
        Save();
    }

    /// <summary>Vrai quand les 7 jours du programme sont écoulés.</summary>
    public bool WeekFinished => Data.StartDate is not null && (DateTime.Today - Data.StartDate.Value.Date).Days >= 7;

    public int DoneCount => Data.Program.Count(m => m.Done);

    public int MealCount => Data.Program.Count;

    public void ToggleDone(Meal meal)
    {
        meal.Done = !meal.Done;
        Save();
    }

    public int TodayIndex => Data.StartDate is null
        ? 0
        : Math.Clamp((DateTime.Today - Data.StartDate.Value.Date).Days, 0, 6);

    public IEnumerable<Meal> MealsOfDay(int day) => Data.Program.Where(m => m.DayIndex == day);

    private DayOfWeek WeekdayOf(int day) => (Data.StartDate ?? DateTime.Today).AddDays(day).DayOfWeek;

    public string DayName(int day) => DayNames[(int)WeekdayOf(day)];

    public string DayShort(int day) => DayShorts[(int)WeekdayOf(day)];

    // --- Remplacement d'un aliment ---
    public void Replace(Meal meal, FoodCategory category, FoodOption option)
    {
        switch (category)
        {
            case FoodCategory.Proteines:
                meal.Protein = option.Label;
                break;
            case FoodCategory.Legumes:
                meal.Vegetables = option.Label.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
                meal.Accompaniment = "légumes";
                break;
            default:
                meal.Complement = option.Label;
                break;
        }

        if (category != FoodCategory.Complement)
        {
            meal.Name = $"{meal.Protein} & {meal.Accompaniment}";
            meal.Description = MenuGenerator.BuildDescription(meal.Vegetables);
            meal.Preparation = MenuGenerator.BuildPreparation(meal);
        }

        NutritionCalculator.Recompute(meal);
        LastChange = $"C’est noté : {option.Label.ToLowerInvariant()} remplace votre choix.";
        Save();
    }
}
