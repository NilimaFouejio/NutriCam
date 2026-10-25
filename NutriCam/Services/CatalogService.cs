using NutriCam.Api;
using NutriCam.Data;

namespace NutriCam.Services;

/// <summary>Catalogues (aliments, activités) : lus depuis le cache local, rafraîchis depuis l'API au plus une fois par jour.</summary>
public class CatalogService
{
    private static readonly TimeSpan MaxAge = TimeSpan.FromHours(24);

    private readonly INutriCamApi _api;
    private readonly ICatalogCache _cache;
    private CatalogSnapshot? _snapshot;

    public CatalogService(INutriCamApi api, ICatalogCache cache)
    {
        _api = api;
        _cache = cache;
    }

    public CatalogSnapshot Current => _snapshot ??= _cache.Load();

    /// <summary>Retourne le catalogue ; hors ligne, retourne simplement la copie locale (éventuellement vide).</summary>
    public async Task<CatalogSnapshot> EnsureAsync(bool force = false, CancellationToken ct = default)
    {
        var snapshot = Current;
        var stale = snapshot.Aliments.Count == 0 || DateTime.UtcNow - snapshot.FetchedAt > MaxAge;
        if (!force && !stale) return snapshot;

        try
        {
            var foods = await _api.GetAlimentsAsync(snapshot.AlimentsETag, ct);
            if (!foods.NotModified && foods.Value is not null)
            {
                snapshot.Aliments = foods.Value;
                snapshot.AlimentsETag = foods.ETag;
            }
            snapshot.Activites = await _api.GetActivitesAsync(ct);
            snapshot.FetchedAt = DateTime.UtcNow;
            _cache.Save(snapshot);
        }
        catch (ApiException)
        {
            // Hors ligne ou session expirée : on garde la copie locale.
        }
        return snapshot;
    }
}
