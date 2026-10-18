using CommunityToolkit.Mvvm.Input;
using NutriCam.Services;

namespace NutriCam.ViewModels;

public partial class AccueilViewModel : BaseViewModel
{
    public AccueilViewModel(ProgramStore store) : base(store) { }

    private bool _redirecting;

    public override void OnAppearing()
    {
        // Un utilisateur déjà connecté, avec un programme, retrouve directement son tableau de bord.
        if (_redirecting) return;
        if (Store.Data.SignedIn && Store.HasProgram)
        {
            _redirecting = true;
            _ = RedirectToDashboardAsync();
        }
    }

    private async Task RedirectToDashboardAsync()
    {
        try
        {
            // Au lancement, Shell est encore en train d'afficher cette page : on le laisse terminer
            // avant de naviguer, sinon : « Pending Navigations still processing ».
            await Task.Delay(300);
            await Go("//dashboard");
        }
        finally
        {
            _redirecting = false;
        }
    }

    [RelayCommand]
    private async Task Start()
    {
        Store.EditMode = false;
        await Go("inscription");
    }

    [RelayCommand]
    private Task Login() => Go("connexion");
}
