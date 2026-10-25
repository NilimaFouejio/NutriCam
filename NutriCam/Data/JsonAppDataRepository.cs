using System.Text.Json;
using NutriCam.Models;

namespace NutriCam.Data;

/// <summary>
/// Stockage dans un fichier JSON du dossier de données de l'application (identique sur toutes les plateformes).
/// Les Preferences sont limitées à ~8 Ko par valeur sous Windows : trop juste pour un programme de 7 jours.
/// Pour passer à SQLite plus tard, il suffit d'écrire une autre implémentation de <see cref="IAppDataRepository"/>.
/// </summary>
public sealed class JsonAppDataRepository : IAppDataRepository
{
    private const string LegacyPreferencesKey = "nutricam.data.v1";
    private static string FilePath => Path.Combine(FileSystem.AppDataDirectory, "nutricam-data.json");

    public AppData Load()
    {
        try
        {
            if (File.Exists(FilePath))
                return JsonSerializer.Deserialize<AppData>(File.ReadAllText(FilePath)) ?? new AppData();

            // Migration : anciennes versions qui stockaient les données dans les Preferences.
            var legacy = Preferences.Default.Get(LegacyPreferencesKey, "");
            if (!string.IsNullOrEmpty(legacy))
            {
                var data = JsonSerializer.Deserialize<AppData>(legacy) ?? new AppData();
                Preferences.Default.Remove(LegacyPreferencesKey);
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

    public void Save(AppData data)
    {
        try
        {
            // Fichier temporaire puis remplacement : pas de fichier tronqué si l'app se ferme en cours d'écriture.
            var tmp = FilePath + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(data));
            File.Move(tmp, FilePath, overwrite: true);
        }
        catch
        {
            // Disque plein ou accès refusé : on ne fait pas planter l'interface.
        }
    }
}
