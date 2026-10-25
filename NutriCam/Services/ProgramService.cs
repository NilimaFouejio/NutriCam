using System.Globalization;
using NutriCam.Api;
using NutriCam.Api.Dtos;
using NutriCam.Models;

namespace NutriCam.Services;

public enum ProgramOutcome { Created, NoCompatible, Offline, SessionExpired, Failed }

public record ProgramResult(ProgramOutcome Outcome, string Message, IReadOnlyList<string> Warnings);

/// <summary>Création et suivi du programme de 7 jours via l'API, avec repli local quand le réseau manque.</summary>
public class ProgramService
{
    private readonly ProgramStore _store;
    private readonly INutriCamApi _api;
    private readonly ProfileService _profiles;

    public ProgramService(ProgramStore store, INutriCamApi api, ProfileService profiles)
    {
        _store = store;
        _api = api;
        _profiles = profiles;
    }

    private static readonly IReadOnlyList<string> NoWarnings = Array.Empty<string>();

    /// <summary>Envoie le profil puis demande un programme compatible. Ne génère rien en local : voir <see cref="GenerateLocal"/>.</summary>
    public async Task<ProgramResult> GenerateAsync(bool regenerate, CancellationToken ct = default)
    {
        if (_store.IsBlocked)
            return new(ProgramOutcome.Failed, "Aucun programme n'est proposé dans votre situation : consultez un professionnel de santé.", NoWarnings);

        // Même clé tant que la création n'a pas abouti : un nouvel essai après une coupure ne crée jamais de doublon.
        var remote = _store.Data.Remote;
        remote.PendingProgramKey ??= Guid.NewGuid().ToString();
        _store.Save();

        try
        {
            // Démo : le serveur n'applique pas encore l'exclusion des aliments. On n'affiche donc pas ses
            // « aliment non reconnu » (bruit), seulement les avertissements du programme (budget, repas par jour).
            await _profiles.PushAsync(ct);
            var warnings = new List<string>();
            var request = new CreerProgrammeRequest(DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), regenerate);
            var program = await _api.CreerProgrammeAsync(request, remote.PendingProgramKey!, ct);

            remote.PendingProgramKey = null;
            _store.ApplyRemoteProgram(program);
            if (program.Avertissements is { Count: > 0 } extra) warnings.AddRange(extra);
            return new(ProgramOutcome.Created, "", warnings);
        }
        catch (ApiException e)
        {
            switch (e.Kind)
            {
                case ApiErrorKind.NoCompatibleProgram:
                    remote.PendingProgramKey = null; // la prochaine demande, après ajustement des critères, est une nouvelle intention
                    _store.Save();
                    return new(ProgramOutcome.NoCompatible, e.Message, NoWarnings);
                case ApiErrorKind.Unauthorized:
                    return new(ProgramOutcome.SessionExpired, e.Message, NoWarnings);
                default:
                    return e.IsConnectivity
                        ? new(ProgramOutcome.Offline, e.Message, NoWarnings)
                        : new(ProgramOutcome.Failed, e.Message, NoWarnings);
            }
        }
    }

    /// <summary>Repli hors ligne : génération sur l'appareil avec le générateur existant.</summary>
    public void GenerateLocal() => _store.GenerateProgram();

    /// <summary>Alternatives de recette proposées par le serveur (déjà filtrées sur les aliments à éviter).</summary>
    public async Task<List<RecetteDto>> GetAlternativesAsync(Meal meal, CancellationToken ct = default)
        => meal.RecipeId is null ? new List<RecetteDto>() : await _api.GetAlternativesAsync(meal.RecipeId, ct);

    /// <summary>« Marquer comme mangé » : effet immédiat et durable en local. À faire suivre de <see cref="SyncDoneAsync"/>.</summary>
    public void ToggleDone(Meal meal)
    {
        meal.DoneDirty = true; // reste vrai tant que le serveur n'a pas confirmé
        _store.ToggleDone(meal); // inverse Done et enregistre
    }

    /// <summary>Envoie l'état « mangé » au serveur ; en cas d'échec il sera renvoyé par <see cref="FlushPendingAsync"/>.</summary>
    public Task SyncDoneAsync(Meal meal) => TrySyncDoneAsync(meal);

    /// <summary>Renvoie les changements « mangé » qui n'ont pas pu partir (hors ligne).</summary>
    public async Task FlushPendingAsync()
    {
        if (!_store.Data.SignedIn) return;
        foreach (var meal in _store.Data.Program.Where(m => m.DoneDirty).ToList())
            if (!await TrySyncDoneAsync(meal)) break;
    }

    private async Task<bool> TrySyncDoneAsync(Meal meal)
    {
        var programId = _store.Data.Remote.ProgramId;
        if (programId is null || meal.RemoteId is null)
        {
            meal.DoneDirty = false; // repas local : rien à synchroniser
            _store.Save();
            return true;
        }

        try
        {
            await _api.MajRepasAsync(programId, meal.RemoteId, new MajRepasRequest(meal.Done));
            meal.DoneDirty = false;
        }
        catch (ApiException)
        {
            meal.DoneDirty = true; // sera renvoyé plus tard
        }
        _store.Save();
        return !meal.DoneDirty;
    }
}
