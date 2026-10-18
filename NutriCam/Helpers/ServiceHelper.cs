using Microsoft.Extensions.DependencyInjection;

namespace NutriCam.Helpers;

/// <summary>Accès à l'injection de dépendances depuis les contrôles (qui n'ont pas de constructeur injecté).</summary>
public static class ServiceHelper
{
    public static T Get<T>() where T : notnull
        => Application.Current!.Handler!.MauiContext!.Services.GetRequiredService<T>();
}
