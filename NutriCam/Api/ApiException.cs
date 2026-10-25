namespace NutriCam.Api;

public enum ApiErrorKind
{
    Network,             // pas de connexion, serveur injoignable
    Timeout,
    Unauthorized,        // jeton absent/expiré ou identifiants invalides
    Validation,          // 422 VALIDATION
    Conflict,            // 409 (ex. e-mail déjà utilisé)
    NotFound,
    NoCompatibleProgram, // 422 AUCUN_PROGRAMME_COMPATIBLE
    ProfileIncomplete,   // 409 PROFIL_INCOMPLET
    Server,              // 5xx
    Unknown
}

/// <summary>Toute erreur d'appel API remonte sous cette forme ; Message est affichable à l'utilisateur.</summary>
public class ApiException : Exception
{
    public ApiException(ApiErrorKind kind, string message, string? code = null, int? status = null)
        : base(message)
    {
        Kind = kind;
        Code = code;
        StatusCode = status;
    }

    public ApiErrorKind Kind { get; }
    public string? Code { get; }
    public int? StatusCode { get; }

    /// <summary>Vrai quand une nouvelle tentative (ou le mode hors ligne) a un sens.</summary>
    public bool IsConnectivity => Kind is ApiErrorKind.Network or ApiErrorKind.Timeout or ApiErrorKind.Server;
}
