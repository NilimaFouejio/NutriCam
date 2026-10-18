using System.Globalization;
using System.Text;

namespace NutriCam.Helpers;

public static class TextUtil
{
    /// <summary>« A, B & C » (affichage compact des cartes de repas).</summary>
    public static string JoinAmp(IEnumerable<string> items)
    {
        var list = items.ToList();
        return list.Count <= 1 ? string.Concat(list) : $"{string.Join(", ", list.Take(list.Count - 1))} & {list[^1]}";
    }

    /// <summary>« A, B et C » (phrases).</summary>
    public static string JoinEt(IEnumerable<string> items)
    {
        var list = items.ToList();
        return list.Count <= 1 ? string.Concat(list) : $"{string.Join(", ", list.Take(list.Count - 1))} et {list[^1]}";
    }

    public static string Capitalize(string s)
        => string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0], CultureInfo.CurrentCulture) + s[1..];

    /// <summary>Minuscules, sans accents, œ→oe, pluriels simplifiés : sert à comparer « Œufs » et « oeuf ».</summary>
    public static string Normalize(string s)
    {
        var lower = s.Trim().ToLowerInvariant().Replace("œ", "oe");
        var decomposed = lower.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in decomposed)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        var words = sb.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(w => w.Length > 3 && w.EndsWith('s') ? w[..^1] : w);
        return string.Join(' ', words);
    }

    public static bool SameFood(string a, string b) => Normalize(a) == Normalize(b);

    /// <summary>Vrai si le texte contient l'aliment cherché (ex. « Poulet grillé » contient « Poulet »).</summary>
    public static bool Mentions(string text, string food)
    {
        var f = Normalize(food);
        return f.Length > 0 && Normalize(text).Contains(f);
    }
}
