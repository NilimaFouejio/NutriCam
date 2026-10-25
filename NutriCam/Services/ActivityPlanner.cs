using NutriCam.Models;

namespace NutriCam.Services;

/// <summary>
/// Propose une activité physique adaptée à l'objectif et au niveau d'activité : nombre de séances par semaine,
/// durée, jours et type de séance. C'est le système qui suggère la fréquence : la personne ne la choisit pas.
/// Repères généraux, sans calcul médical.
/// </summary>
public static class ActivityPlanner
{
    // Séances conseillées par semaine : ligne = objectif (Perdre, Maintenir, Prendre, Équilibrer),
    // colonne = niveau d'activité actuel (très sédentaire, légèrement actif, modérément actif, très actif).
    // On commence doucement quand le niveau est bas.
    private static readonly int[][] Table =
    {
        new[] { 3, 3, 4, 5 },
        new[] { 3, 3, 3, 4 },
        new[] { 2, 2, 3, 3 },
        new[] { 3, 3, 3, 4 },
    };

    private static readonly int[] MinutesByLevel = { 25, 30, 35, 45 };

    private static readonly (string Title, string Detail) Marche =
        ("Marche rapide", "Un pas soutenu : vous pouvez parler, mais difficilement chanter.");
    private static readonly (string Title, string Detail) Renfo =
        ("Renforcement au poids du corps", "Squats, pompes (au genou si besoin) et gainage, en 3 tours.");
    private static readonly (string Title, string Detail) Danse =
        ("Danse", "Mettez de la musique et bougez sans vous arrêter.");
    private static readonly (string Title, string Detail) Velo =
        ("Vélo", "Un rythme régulier, sans forcer dans les montées.");
    private static readonly (string Title, string Detail) Foot =
        ("Football ou jeu collectif", "Un match entre amis compte comme une séance.");
    private static readonly (string Title, string Detail) Corde =
        ("Corde à sauter", "Séries d’une minute, avec de courtes pauses.");
    private static readonly (string Title, string Detail) Douce =
        ("Marche tranquille", "Une sortie douce qui aide à récupérer.");

    // Séances proposées, dans l'ordre, pour chaque objectif (même ordre que l'énumération Goal).
    private static readonly (string Title, string Detail)[][] Pools =
    {
        new[] { Marche, Renfo, Velo, Danse, Marche, Corde },
        new[] { Marche, Foot, Renfo, Danse, Velo, Marche },
        new[] { Renfo, Douce, Renfo, Foot, Renfo, Douce },
        new[] { Marche, Danse, Renfo, Foot, Velo, Marche },
    };

    // Jours répartis pour laisser un temps de repos entre les séances.
    private static readonly string[][] DayPatterns =
    {
        new[] { "Samedi" },
        new[] { "Mardi", "Samedi" },
        new[] { "Lundi", "Mercredi", "Samedi" },
        new[] { "Lundi", "Mercredi", "Vendredi", "Dimanche" },
        new[] { "Lundi", "Mardi", "Jeudi", "Vendredi", "Samedi" },
        new[] { "Lundi", "Mardi", "Jeudi", "Vendredi", "Samedi", "Dimanche" },
    };

    private static int GoalIndex(AppData data) => (int)(data.Goal ?? Goal.Equilibrer);

    private static int LevelIndex(AppData data) => (int)(data.Profile.Activity ?? ActivityLevel.ModerementActif);

    private static bool IsMinor(AppData data) => data.Profile.Age is < 18;

    /// <summary>Nombre de séances par semaine suggéré selon l'objectif et le niveau d'activité.</summary>
    public static int SessionsPerWeek(AppData data)
    {
        var sessions = Table[GoalIndex(data)][LevelIndex(data)];
        return Math.Clamp(IsMinor(data) ? Math.Max(sessions, 4) : sessions, 1, DayPatterns.Length);
    }

    public static ActivityPlan Build(AppData data)
    {
        var sessions = SessionsPerWeek(data);
        var minutes = IsMinor(data) ? 60 : MinutesByLevel[LevelIndex(data)];
        var pool = Pools[GoalIndex(data)];
        var days = DayPatterns[sessions - 1];

        var list = new List<ActivitySession>();
        for (var i = 0; i < sessions; i++)
        {
            var (title, detail) = pool[i % pool.Length];
            list.Add(new ActivitySession(days[i], title, detail, $"{minutes} min"));
        }

        var total = sessions * minutes;
        return new ActivityPlan(sessions, minutes, total, list, Note(data, total));
    }

    private static string Note(AppData data, int total)
    {
        if (IsMinor(data))
            return "À votre âge, on recommande environ 60 minutes d’activité par jour : jeux, sport, marche… tout compte.";
        if (total < 150)
            return "Les adultes sont invités à viser au moins 150 minutes d’activité modérée par semaine : augmentez peu à peu, au rythme qui vous convient.";
        return "Vous atteignez le repère de 150 minutes par semaine. Échauffez-vous 5 minutes avant chaque séance.";
    }

    public static string SessionsLabel(int sessions) => sessions == 1 ? "1 séance" : $"{sessions} séances";
}
