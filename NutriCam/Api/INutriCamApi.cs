using NutriCam.Api.Dtos;

namespace NutriCam.Api;

/// <summary>
/// Contrat côté application : une méthode par route de docs/api/nutricam-openapi.yaml.
/// Toutes lèvent <see cref="ApiException"/> en cas d'échec.
/// </summary>
public interface INutriCamApi
{
    // Auth
    Task<SessionDto> InscrireAsync(InscriptionRequest request, CancellationToken ct = default);
    Task<SessionDto> ConnecterAsync(ConnexionRequest request, CancellationToken ct = default);
    Task SupprimerCompteAsync(CancellationToken ct = default);

    // Profil
    /// <summary>Null si aucun profil n'existe encore (404 PROFIL_ABSENT).</summary>
    Task<ProfilCompletDto?> GetProfilAsync(CancellationToken ct = default);
    Task<ProfilCompletDto> SauvegarderProfilAsync(ProfilCompletDto profil, CancellationToken ct = default);

    // Catalogues (lecture seule)
    Task<Conditional<List<AlimentDto>>> GetAlimentsAsync(string? etag, CancellationToken ct = default);
    Task<List<ActiviteDto>> GetActivitesAsync(CancellationToken ct = default);

    // Programme
    Task<ProgrammeDto> CreerProgrammeAsync(CreerProgrammeRequest request, string idempotencyKey, CancellationToken ct = default);
    Task<ProgrammeDto> GetSemaineAsync(string idProgramme, CancellationToken ct = default);
    Task MajRepasAsync(string idProgramme, string idRepasProgramme, MajRepasRequest request, CancellationToken ct = default);
    Task<List<RecetteDto>> GetAlternativesAsync(string idRecette, CancellationToken ct = default);

    // Activité physique
    Task<List<RecommandationActiviteDto>> GetRecommandationsActiviteAsync(CancellationToken ct = default);
}
