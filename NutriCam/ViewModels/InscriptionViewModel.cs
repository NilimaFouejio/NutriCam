using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriCam.Helpers;
using NutriCam.Services;

namespace NutriCam.ViewModels;

public partial class InscriptionViewModel : BaseViewModel
{
    private readonly AuthService _auth;

    public InscriptionViewModel(ProgramStore store, AuthService auth) : base(store) => _auth = auth;

    [ObservableProperty] private string firstName = "";
    [ObservableProperty] private string email = "";
    [ObservableProperty] private string password = "";
    [ObservableProperty] private string confirmation = "";
    [ObservableProperty] private string firstNameError = "";
    [ObservableProperty] private string emailError = "";
    [ObservableProperty] private string passwordError = "";
    [ObservableProperty] private string confirmationError = "";
    [ObservableProperty] private string formError = "";

    public string RegisterText => IsBusy ? "Création du compte…" : "Créer mon compte";

    protected override void OnBusyChanged() => OnPropertyChanged(nameof(RegisterText));

    [RelayCommand]
    private async Task Register()
    {
        FirstNameError = Validation.Required(FirstName, "Dites-nous comment vous appeler.");
        EmailError = Validation.Email(Email);
        PasswordError = Validation.NewPassword(Password);
        ConfirmationError = Confirmation == Password ? "" : "Les deux mots de passe sont différents.";
        FormError = "";
        if (FirstNameError != "" || EmailError != "" || PasswordError != "" || ConfirmationError != "") return;

        IsBusy = true;
        var (ok, error) = await _auth.RegisterAsync(FirstName, Email, Password);
        IsBusy = false;

        if (!ok)
        {
            FormError = error ?? "Création du compte impossible.";
            return;
        }

        Store.EditMode = false;
        await Go("//accueil/profil");
    }

    [RelayCommand]
    private Task HaveAccount() => Go("//accueil/connexion");
}
