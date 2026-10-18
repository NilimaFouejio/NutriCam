using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NutriCam.Services;
using NutriCam.ViewModels;
using NutriCam.Views;

namespace NutriCam;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Outfit-SemiBold.ttf", "OutfitSemiBold");
                fonts.AddFont("Inter-Regular.ttf", "InterRegular");
                fonts.AddFont("Inter-SemiBold.ttf", "InterSemiBold");
                fonts.AddFont("Lucide.ttf", "Lucide"); // police d'icônes
            });

        // Les champs de saisie de la maquette n'ont pas de soulignement : le cadre est géré par la page.
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("SansSoulignement", (handler, view) =>
        {
#if ANDROID
            handler.PlatformView.BackgroundTintList =
                Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#elif IOS || MACCATALYST
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#elif WINDOWS
            var transparent = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            var textBox = handler.PlatformView;
            textBox.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            textBox.Background = transparent;
            // Le TextBox Windows change de fond/bordure au survol et au focus : on neutralise ces états.
            textBox.Resources["TextControlBackground"] = transparent;
            textBox.Resources["TextControlBackgroundPointerOver"] = transparent;
            textBox.Resources["TextControlBackgroundFocused"] = transparent;
            textBox.Resources["TextControlBorderBrush"] = transparent;
            textBox.Resources["TextControlBorderBrushPointerOver"] = transparent;
            textBox.Resources["TextControlBorderBrushFocused"] = transparent;
            textBox.Resources["TextControlBorderThemeThicknessFocused"] = new Microsoft.UI.Xaml.Thickness(0);
#endif
        });

        // Services (état partagé, authentification locale)
        builder.Services.AddSingleton<ProgramStore>();
        builder.Services.AddSingleton<AuthService>();

        // Écran 1 à 3 : accueil, connexion, inscription
        builder.Services.AddTransient<AccueilViewModel>();
        builder.Services.AddTransient<AccueilPage>();
        builder.Services.AddTransient<ConnexionViewModel>();
        builder.Services.AddTransient<ConnexionPage>();
        builder.Services.AddTransient<InscriptionViewModel>();
        builder.Services.AddTransient<InscriptionPage>();

        // Écrans 4 à 7 : parcours « Votre programme » 1/4 à 4/4
        builder.Services.AddTransient<ProfilViewModel>();
        builder.Services.AddTransient<ProfilPage>();
        builder.Services.AddTransient<ObjectifViewModel>();
        builder.Services.AddTransient<ObjectifPage>();
        builder.Services.AddTransient<PreferencesViewModel>();
        builder.Services.AddTransient<PreferencesPage>();
        builder.Services.AddTransient<BudgetViewModel>();
        builder.Services.AddTransient<BudgetPage>();

        // Écrans 8 et 9 : tableau de bord, programme de la semaine
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<ProgrammeViewModel>();
        builder.Services.AddTransient<ProgrammePage>();

        // Écrans 10 et 11 : détail d'un repas, alternatives
        builder.Services.AddTransient<DetailRepasViewModel>();
        builder.Services.AddTransient<DetailRepasPage>();
        builder.Services.AddTransient<AlternativesViewModel>();
        builder.Services.AddTransient<AlternativesPage>();

        // Écrans ajoutés à l'étape 5 : profil et réglages, bilan de la semaine
        builder.Services.AddTransient<ReglagesViewModel>();
        builder.Services.AddTransient<ReglagesPage>();
        builder.Services.AddTransient<BilanViewModel>();
        builder.Services.AddTransient<BilanPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
