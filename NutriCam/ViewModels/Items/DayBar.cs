using NutriCam.Helpers;

namespace NutriCam.ViewModels;

/// <summary>Un segment de la barre « Jour n sur 7 » du tableau de bord.</summary>
public record DayBar(bool Done)
{
    public Color Fill => Done ? Palette.Terracotta : Palette.Bordure;
}
