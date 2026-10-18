namespace NutriCam;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // La charte NutriCam n'existe qu'en thème clair.
        UserAppTheme = AppTheme.Light;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell()) { Title = "NutriCam" };

#if WINDOWS || MACCATALYST
        // Les écrans sont dessinés pour un téléphone : sur ordinateur, on garde un format portrait confortable.
        window.Width = 430;
        window.Height = 860;
        window.MinimumWidth = 380;
        window.MinimumHeight = 640;
        window.MaximumWidth = 560;
#endif
        return window;
    }
}
