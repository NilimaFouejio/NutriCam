using NutriCam.Controls;

namespace NutriCam.Helpers;

/// <summary>Fait défiler une page jusqu'à l'élément qui manque, pour que la personne voie tout de suite quoi renseigner.</summary>
public static class ScrollHelper
{
    public static async Task ScrollToAsync(ScrollView scroller, VisualElement? target)
    {
        if (target is null) return;

        await Task.Delay(80); // laisse la mise en page afficher les messages d'erreur avant de défiler
        await scroller.ScrollToAsync(target, ScrollToPosition.Start, true);

        // Un champ texte reçoit directement le curseur de saisie.
        if (target is FormField field) field.FocusInput();
    }
}
