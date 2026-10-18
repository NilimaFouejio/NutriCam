namespace NutriCam.Helpers;

/// <summary>
/// Point d'entrée unique pour naviguer avec Shell. Évite l'exception
/// « Pending Navigations still processing » :
/// - un double appui (ou deux navigations simultanées) est ignoré ;
/// - si Shell n'a pas fini sa navigation précédente (ou son affichage initial),
///   on patiente un court instant puis on réessaie.
/// </summary>
public static class Nav
{
    private const int MaxAttempts = 15;
    private const int RetryDelayMs = 150;

    private static readonly SemaphoreSlim Gate = new(1, 1);

    public static async Task GoAsync(string route)
    {
        // Une navigation est déjà en cours : on ignore la nouvelle demande (double appui).
        if (!await Gate.WaitAsync(0)) return;

        try
        {
            for (var attempt = 0; attempt < MaxAttempts; attempt++)
            {
                try
                {
                    var shell = Shell.Current;
                    if (shell is null)
                    {
                        await Task.Delay(RetryDelayMs);
                        continue;
                    }

                    await MainThread.InvokeOnMainThreadAsync(() => shell.GoToAsync(route));
                    return;
                }
                catch (Exception ex) when (ex.Message.Contains("Pending Navigations", StringComparison.OrdinalIgnoreCase))
                {
                    // Shell est encore occupé : on laisse passer un instant et on réessaie.
                    await Task.Delay(RetryDelayMs);
                }
            }
        }
        finally
        {
            Gate.Release();
        }
    }
}
