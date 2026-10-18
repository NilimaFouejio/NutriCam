using CommunityToolkit.Mvvm.Input;
using NutriCam.Services;

namespace NutriCam.ViewModels;

public partial class AccueilViewModel : BaseViewModel
{
    public AccueilViewModel(ProgramStore store) : base(store) { }

    public override void OnAppearing()
    {
        // Un utilisateur déjà connecté, avec un programme, retrouve directement son tableau de bord.
        if (Store.Data.SignedIn && Store.HasProgram) MainThread.BeginInvokeOnMainThread(async () => await Go("//dashboard"));
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
