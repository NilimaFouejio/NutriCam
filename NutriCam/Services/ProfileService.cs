using NutriCam.Api;
using NutriCam.Mapping;

namespace NutriCam.Services;

/// <summary>Synchronisation du profil, de l'objectif et des critères alimentaires avec l'API (PUT/GET /profils/moi).</summary>
public class ProfileService
{
    private readonly ProgramStore _store;
    private readonly INutriCamApi _api;
    private readonly CatalogService _catalog;

    public ProfileService(ProgramStore store, INutriCamApi api, CatalogService catalog)
    {
        _store = store;
        _api = api;
        _catalog = catalog;
    }

    /// <summary>Envoie l'état local au serveur (idempotent). Retourne les avertissements éventuels (aliments non reconnus…).</summary>
    public async Task<IReadOnlyList<string>> PushAsync(CancellationToken ct = default)
    {
        var catalog = await _catalog.EnsureAsync(ct: ct);
        var request = ApiMapper.ToProfilComplet(_store.Data, catalog);
        var saved = await _api.SauvegarderProfilAsync(request, ct);

        _store.Data.Remote.ProfileId = saved.Profil.IdProfil;
        _store.Data.Remote.ObjectiveId = saved.Objectif.IdObjectif;
        _store.Save();
        return saved.Avertissements ?? new List<string>();
    }

    /// <summary>
    /// Après une connexion : si l'appareil n'a pas encore de profil, récupère celui du serveur (et son programme).
    /// Ne remplace jamais des données locales existantes. Retourne vrai si quelque chose a été restauré.
    /// </summary>
    public async Task<bool> RestoreIfEmptyAsync(CancellationToken ct = default)
    {
        if (_store.Data.Profile.Age is not null) return false;

        var remote = await _api.GetProfilAsync(ct);
        if (remote is null) return false;

        var catalog = await _catalog.EnsureAsync(ct: ct);
        ApiMapper.ApplyProfil(_store.Data, remote, catalog);

        if (remote.IdProgrammeCourant is { Length: > 0 } programId)
            _store.ApplyRemoteProgram(await _api.GetSemaineAsync(programId, ct));
        else
            _store.Save();
        return true;
    }
}
