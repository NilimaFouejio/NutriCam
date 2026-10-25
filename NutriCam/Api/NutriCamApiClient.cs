using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using NutriCam.Api.Dtos;
using NutriCam.Data;

namespace NutriCam.Api;

/// <summary>Implémentation HTTP/JSON de <see cref="INutriCamApi"/>. Les chemins sont relatifs à ApiSettings.BaseUrl.</summary>
public sealed class NutriCamApiClient : INutriCamApi
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _http;
    private readonly ITokenStore _tokens;

    public NutriCamApiClient(HttpClient http, ITokenStore tokens)
    {
        _http = http;
        _tokens = tokens;
    }

    // ---------- Auth ----------
    public Task<SessionDto> InscrireAsync(InscriptionRequest request, CancellationToken ct = default)
        => SendAsync<SessionDto>(HttpMethod.Post, "auth/inscription", request, null, ct);

    public Task<SessionDto> ConnecterAsync(ConnexionRequest request, CancellationToken ct = default)
        => SendAsync<SessionDto>(HttpMethod.Post, "auth/connexion", request, null, ct);

    public Task SupprimerCompteAsync(CancellationToken ct = default)
        => SendAsync(HttpMethod.Delete, "utilisateurs/moi", null, null, ct);

    // ---------- Profil ----------
    public async Task<ProfilCompletDto?> GetProfilAsync(CancellationToken ct = default)
    {
        try
        {
            return await SendAsync<ProfilCompletDto>(HttpMethod.Get, "profils/moi", null, null, ct);
        }
        catch (ApiException e) when (e.Kind == ApiErrorKind.NotFound)
        {
            return null; // PROFIL_ABSENT : pas une erreur
        }
    }

    public Task<ProfilCompletDto> SauvegarderProfilAsync(ProfilCompletDto profil, CancellationToken ct = default)
        => SendAsync<ProfilCompletDto>(HttpMethod.Put, "profils/moi", profil, null, ct);

    // ---------- Catalogues ----------
    public async Task<Conditional<List<AlimentDto>>> GetAlimentsAsync(string? etag, CancellationToken ct = default)
    {
        using var response = await SendRawAsync(HttpMethod.Get, "aliments", null,
            r => { if (!string.IsNullOrEmpty(etag)) r.Headers.TryAddWithoutValidation("If-None-Match", etag); }, ct);

        if (response.StatusCode == HttpStatusCode.NotModified)
            return new Conditional<List<AlimentDto>>(null, etag, true);

        var items = await ReadAsync<List<AlimentDto>>(response, ct);
        return new Conditional<List<AlimentDto>>(items, response.Headers.ETag?.ToString(), false);
    }

    public Task<List<ActiviteDto>> GetActivitesAsync(CancellationToken ct = default)
        => SendAsync<List<ActiviteDto>>(HttpMethod.Get, "activites", null, null, ct);

    // ---------- Programme ----------
    public Task<ProgrammeDto> CreerProgrammeAsync(CreerProgrammeRequest request, string idempotencyKey, CancellationToken ct = default)
        => SendAsync<ProgrammeDto>(HttpMethod.Post, "profils/moi/programmes", request,
            r => r.Headers.TryAddWithoutValidation("Idempotency-Key", idempotencyKey), ct);

    public Task<ProgrammeDto> GetSemaineAsync(string idProgramme, CancellationToken ct = default)
        => SendAsync<ProgrammeDto>(HttpMethod.Get, $"programmes/{Uri.EscapeDataString(idProgramme)}/semaine", null, null, ct);

    public Task MajRepasAsync(string idProgramme, string idRepasProgramme, MajRepasRequest request, CancellationToken ct = default)
        => SendAsync(new HttpMethod("PATCH"),
            $"programmes/{Uri.EscapeDataString(idProgramme)}/repas/{Uri.EscapeDataString(idRepasProgramme)}", request, null, ct);

    public Task<List<RecetteDto>> GetAlternativesAsync(string idRecette, CancellationToken ct = default)
        => SendAsync<List<RecetteDto>>(HttpMethod.Get, $"recettes/{Uri.EscapeDataString(idRecette)}/alternatives", null, null, ct);

    // ---------- Activité ----------
    public Task<List<RecommandationActiviteDto>> GetRecommandationsActiviteAsync(CancellationToken ct = default)
        => SendAsync<List<RecommandationActiviteDto>>(HttpMethod.Get, "profils/moi/recommandations-activite", null, null, ct);

    // ---------- Plomberie HTTP ----------
    private async Task<T> SendAsync<T>(HttpMethod method, string path, object? body, Action<HttpRequestMessage>? configure, CancellationToken ct)
    {
        using var response = await SendRawAsync(method, path, body, configure, ct);
        return await ReadAsync<T>(response, ct);
    }

    private async Task SendAsync(HttpMethod method, string path, object? body, Action<HttpRequestMessage>? configure, CancellationToken ct)
    {
        using var response = await SendRawAsync(method, path, body, configure, ct);
    }

    private async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var value = await response.Content.ReadFromJsonAsync<T>(Json, ct);
            return value ?? throw new ApiException(ApiErrorKind.Unknown, "Réponse vide du serveur.");
        }
        catch (JsonException)
        {
            throw new ApiException(ApiErrorKind.Unknown, "Réponse du serveur illisible.");
        }
    }

    /// <summary>Envoie la requête (jeton compris) et transforme tout échec en <see cref="ApiException"/>.</summary>
    private async Task<HttpResponseMessage> SendRawAsync(
        HttpMethod method, string path, object? body, Action<HttpRequestMessage>? configure, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body is not null)
            request.Content = JsonContent.Create(body, body.GetType(), null, Json);

        var token = await _tokens.GetAsync();
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        configure?.Invoke(request);

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request, ct);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new ApiException(ApiErrorKind.Timeout, "Le serveur met trop de temps à répondre. Réessayez dans un instant.");
        }
        catch (HttpRequestException)
        {
            throw new ApiException(ApiErrorKind.Network, "Connexion impossible. Vérifiez votre réseau et réessayez.");
        }

        if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotModified)
            return response;

        using (response)
            throw await ToExceptionAsync(response, ct);
    }

    private static async Task<ApiException> ToExceptionAsync(HttpResponseMessage response, CancellationToken ct)
    {
        ErreurDto? error = null;
        try
        {
            error = await response.Content.ReadFromJsonAsync<ErreurDto>(Json, ct);
        }
        catch
        {
            // Corps absent ou non JSON : on s'appuie sur le code HTTP.
        }

        var status = (int)response.StatusCode;
        var code = error?.Code;
        var kind = (status, code) switch
        {
            (401, _) => ApiErrorKind.Unauthorized,
            (404, _) => ApiErrorKind.NotFound,
            (409, "PROFIL_INCOMPLET") => ApiErrorKind.ProfileIncomplete,
            (409, _) => ApiErrorKind.Conflict,
            (422, "AUCUN_PROGRAMME_COMPATIBLE") => ApiErrorKind.NoCompatibleProgram,
            (422, _) => ApiErrorKind.Validation,
            ( >= 500, _) => ApiErrorKind.Server,
            _ => ApiErrorKind.Unknown
        };

        var message = !string.IsNullOrWhiteSpace(error?.Message)
            ? error!.Message
            : kind switch
            {
                ApiErrorKind.Unauthorized => "Votre session a expiré. Reconnectez-vous.",
                ApiErrorKind.Server => "Le service rencontre un problème. Réessayez plus tard.",
                ApiErrorKind.NotFound => "Élément introuvable.",
                _ => "Une erreur est survenue. Réessayez."
            };

        return new ApiException(kind, message, code, status);
    }
}
