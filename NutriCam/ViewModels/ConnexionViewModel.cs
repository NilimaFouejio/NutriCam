using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriCam.Helpers;
using NutriCam.Services;

namespace NutriCam.ViewModels;

public partial class ConnexionViewModel : BaseViewModel
{
    private readonly AuthService _auth;

    public ConnexionViewModel(ProgramStore store, AuthService auth) : base(store) => _auth = auth;

    [ObservableProperty] private string email = "";
    [ObservableProperty] private string password = "";
    [ObservableProperty] private string emailError = "";
    [ObservableProperty] private string passwordError = "";
    [ObservableProperty] private string formError = "";

    public string LoginText => IsBusy ? "Connexion en cours…" : "Se connecter";

    protected override void OnBusyChanged() => OnPropertyChanged(nameof(LoginText));

    [RelayCommand]
    private async Task Login()
    {
        EmailError = Validation.Email(Email);
        PasswordError = Validation.Required(Password, "Saisissez votre mot de passe.");
        FormError = "";
        var firstMissing = EmailError != "" ? "email" : PasswordError != "" ? "password" : null;
        if (firstMissing is not null)
        {
            ScrollTo(firstMissing);
            return;
        }

        IsBusy = true;
        var (ok, error) = await _auth.LoginAsync(Email, Password);
        IsBusy = false;

        if (!ok)
        {
            FormError = error ?? "Connexion impossible.";
            return;
        }

        Store.EditMode = false;
        if (Store.IsBlocked) { await Go("//accueil/orientation"); return; }
        if (Store.HasProgram) { await Go("//dashboard"); return; }
        await Go("//accueil/profil");
    }

    [RelayCommand]
    private async Task ForgotPassword()
    {
        var mail = await Shell.Current.DisplayPromptAsync(
            "Mot de passe oublié",
            "Saisissez votre e-mail pour recevoir un lien de réinitialisation.",
            "Envoyer", "Annuler", "Votre adresse e-mail",
            keyboard: Keyboard.Email, initialValue: Email);

        if (mail is null) return;
        if (Validation.Email(mail) != "")
        {
            await Alert("Adresse incorrecte", "Vérifiez votre adresse e-mail et réessayez.");
            return;
        }
        // Simulation : l'envoi réel nécessite l'API (voir recommandations).
        await Alert("Vérifiez votre boîte mail", "Si un compte existe pour cette adresse, un lien vient de vous être envoyé.");
    }

    [RelayCommand]
    private Task CreateAccount() => Go("//accueil/inscription");
}
