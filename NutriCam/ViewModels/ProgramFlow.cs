using NutriCam.Services;

namespace NutriCam.ViewModels;

/// <summary>
/// Parcours commun « obtenir un programme » : appel API, puis dialogues pour chaque cas
/// (aucun programme compatible, session expirée, réseau absent avec nouvelle tentative ou repli hors ligne).
/// </summary>
public static class ProgramFlow
{
    /// <summary>Retourne vrai si un programme est maintenant disponible sur l'appareil.</summary>
    public static async Task<bool> RunAsync(ProgramService programs, bool regenerate)
    {
        while (true)
        {
            var result = await programs.GenerateAsync(regenerate);
            switch (result.Outcome)
            {
                case ProgramOutcome.Created:
                    if (result.Warnings.Count > 0)
                        await Shell.Current.DisplayAlert("À savoir", string.Join("\n", result.Warnings), "OK");
                    return true;

                case ProgramOutcome.NoCompatible:
                    await Shell.Current.DisplayAlert("Aucun programme compatible", result.Message, "OK");
                    return false;

                case ProgramOutcome.SessionExpired:
                    await Shell.Current.DisplayAlert("Session expirée", "Reconnectez-vous pour continuer.", "OK");
                    return false;

                case ProgramOutcome.Offline:
                    var choice = await Shell.Current.DisplayActionSheet(
                        result.Message, "Annuler", null, "Réessayer", "Générer hors ligne");
                    if (choice == "Réessayer") continue;
                    if (choice == "Générer hors ligne")
                    {
                        programs.GenerateLocal();
                        return true;
                    }
                    return false;

                default:
                    await Shell.Current.DisplayAlert("Programme non créé", result.Message, "OK");
                    return false;
            }
        }
    }
}
