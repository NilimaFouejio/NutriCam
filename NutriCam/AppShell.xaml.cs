using NutriCam.Views;

namespace NutriCam;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Pages empilées : on y accède par navigation relative (le bouton retour fonctionne).
        Routing.RegisterRoute("inscription", typeof(InscriptionPage));
        Routing.RegisterRoute("connexion", typeof(ConnexionPage));
        Routing.RegisterRoute("profil", typeof(ProfilPage));
        Routing.RegisterRoute("objectif", typeof(ObjectifPage));
        Routing.RegisterRoute("preferences", typeof(PreferencesPage));
        Routing.RegisterRoute("budget", typeof(BudgetPage));
        Routing.RegisterRoute("recap", typeof(RecapPage));
        Routing.RegisterRoute("orientation", typeof(OrientationPage));
        Routing.RegisterRoute("repas", typeof(DetailRepasPage));
        Routing.RegisterRoute("alternatives", typeof(AlternativesPage));
        Routing.RegisterRoute("bilan", typeof(BilanPage));
    }
}
