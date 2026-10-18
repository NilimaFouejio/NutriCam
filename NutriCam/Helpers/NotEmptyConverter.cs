using System.Globalization;

namespace NutriCam.Helpers;

/// <summary>Vrai si la chaîne n'est pas vide (affichage des messages d'erreur).</summary>
public class NotEmptyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => !string.IsNullOrWhiteSpace(value as string);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
