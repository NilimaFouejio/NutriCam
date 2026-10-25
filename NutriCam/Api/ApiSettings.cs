namespace NutriCam.Api;

/// <summary>
/// Adresse de l'API. Par défaut : le backend NutriCam lancé sur l'ordinateur de développement
/// (`docker compose up`, port 8080, préfixe /api/v1).
///   - Émulateur Android : 10.0.2.2 désigne l'ordinateur hôte.
///   - Téléphone réel : remplacez DevHost par l'adresse IP de l'ordinateur sur le Wi-Fi (ex. 192.168.1.20),
///     le téléphone et l'ordinateur devant être sur le même réseau.
///   - Autre adresse sans recompiler : Preferences.Default.Set("nutricam.api.url", "https://…/api/v1/").
/// </summary>
public static class ApiSettings
{
    private const string OverrideKey = "nutricam.api.url";

    private const int Port = 8080;

    /// <summary>Laissez vide pour l'émulateur / le simulateur / Windows ; sinon l'adresse IP de l'ordinateur (téléphone réel).</summary>
    private static readonly string DevHost = "";

    private static string DefaultUrl
    {
        get
        {
            var host = DevHost.Length > 0
                ? DevHost
                : DeviceInfo.Platform == DevicePlatform.Android ? "10.0.2.2" : "localhost";
            return $"http://{host}:{Port}/api/v1/";
        }
    }

    public static string BaseUrl
    {
        get
        {
            var custom = Preferences.Default.Get(OverrideKey, "");
            var url = custom.Length > 0 ? custom : DefaultUrl;
            return url.EndsWith('/') ? url : url + "/";
        }
    }

    public static TimeSpan Timeout => TimeSpan.FromSeconds(15);
}
