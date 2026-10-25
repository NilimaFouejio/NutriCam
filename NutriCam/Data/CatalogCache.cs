using System.Text.Json;
using NutriCam.Api.Dtos;

namespace NutriCam.Data;

/// <summary>Copie locale des catalogues (aliments, activités) pour fonctionner hors ligne.</summary>
public class CatalogSnapshot
{
    public List<AlimentDto> Aliments { get; set; } = new();
    public List<ActiviteDto> Activites { get; set; } = new();
    public string? AlimentsETag { get; set; }
    public DateTime FetchedAt { get; set; }
}

public interface ICatalogCache
{
    CatalogSnapshot Load();
    void Save(CatalogSnapshot snapshot);
}

public sealed class JsonCatalogCache : ICatalogCache
{
    private static string FilePath => Path.Combine(FileSystem.AppDataDirectory, "nutricam-catalogue.json");

    public CatalogSnapshot Load()
    {
        try
        {
            if (File.Exists(FilePath))
                return JsonSerializer.Deserialize<CatalogSnapshot>(File.ReadAllText(FilePath)) ?? new CatalogSnapshot();
        }
        catch
        {
            // Cache illisible : il sera retéléchargé.
        }
        return new CatalogSnapshot();
    }

    public void Save(CatalogSnapshot snapshot)
    {
        try
        {
            var tmp = FilePath + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(snapshot));
            File.Move(tmp, FilePath, overwrite: true);
        }
        catch
        {
            // Sans conséquence : le catalogue sera retéléchargé.
        }
    }
}
