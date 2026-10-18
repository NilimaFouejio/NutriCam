using System.Globalization;
using System.Text.RegularExpressions;

namespace NutriCam.Helpers;

/// <summary>Règles de validation des formulaires. Chaque méthode renvoie "" si la valeur est valide.</summary>
public static class Validation
{
    public static string Email(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Saisissez votre adresse e-mail.";
        return Regex.IsMatch(value.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$")
            ? ""
            : "Cette adresse e-mail semble incorrecte.";
    }

    public static string NewPassword(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "Choisissez un mot de passe.";
        return value.Length < 8 ? "8 caractères minimum." : "";
    }

    public static string Required(string? value, string message)
        => string.IsNullOrWhiteSpace(value) ? message : "";

    /// <summary>Nombre compris entre min et max (la virgule décimale est acceptée).</summary>
    public static string NumberInRange(string? text, double min, double max, string message, out double value)
    {
        value = 0;
        var cleaned = (text ?? "").Trim().Replace(',', '.');
        if (!double.TryParse(cleaned, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
            || value < min || value > max)
            return message;
        return "";
    }
}
