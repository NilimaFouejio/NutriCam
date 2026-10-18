using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriCam.Helpers;
using NutriCam.Services;

namespace NutriCam.ViewModels;

/// <summary>Onglet Profil : compte, modification de chaque section du parcours, bilan, déconnexion, suppression des données.</summary>
public partial class ReglagesViewModel : BaseViewModel
{
    private static readonly string[] GoalNames = { "Perdre du poids", "Maintenir mon poids", "Prendre du poids", "Mieux équilibrer" };
    private static readonly string[] BudgetNames = { "économique", "moyen", "confort" };

    private readonly AuthService _auth;

    public ReglagesViewModel(ProgramStore store, AuthService auth) : base(store) => _auth = auth;

    public ObservableCollection<SettingsRow> Rows { get; } = new();

    [ObservableProperty] private string initial = "N";
    [ObservableProperty] private string firstName = "";
    [ObservableProperty] private string email = "";

    public override void OnAppearing()
    {
        var data = Store.Data;
        FirstName = data.Account?.FirstName ?? "";
        Email = data.Account?.Email ?? "";
        Initial = FirstName.Length > 0 ? FirstName[..1].ToUpperInvariant() : "N";

        var p = data.Profile;
        var profile = p.Age is null ? "À renseigner" : $"{p.Age} ans · {p.HeightCm:0.#} cm · {p.WeightKg:0.#} kg";
        var excluded = data.Excluded.ToList();
        var tastes = $"{data.Liked.Count} aliment(s) aimé(s)" + (excluded.Count > 0 ? $" · sans {string.Join(", ", excluded.Select(e => e.ToLowerInvariant()))}" : "");

        Rows.Clear();
        Rows.Add(new SettingsRow("Profil", profile, Icons.UserRound, () => Edit("profil")));
        Rows.Add(new SettingsRow("Objectif", GoalNames[(int)data.Goal], Icons.Scale, () => Edit("objectif")));
        Rows.Add(new SettingsRow("Goûts et allergies", tastes, Icons.Carrot, () => Edit("preferences")));
        Rows.Add(new SettingsRow("Budget et disponibilité", $"Budget {BudgetNames[(int)data.Budget]} · {data.Available.Count} aliments faciles à trouver", Icons.Wheat, () => Edit("budget")));
        if (Store.HasProgram)
            Rows.Add(new SettingsRow("Bilan de la semaine", $"{Store.DoneCount} repas mangés sur {Store.MealCount}", Icons.CalendarDays, () => Go("bilan")));
    }

    /// <summary>Rouvre le parcours à l'étape choisie ; il se termine par la mise à jour du programme.</summary>
    private Task Edit(string route)
    {
        Store.EditMode = true;
        return Go(route);
    }

    [RelayCommand]
    private async Task Logout()
    {
        var ok = await Shell.Current.DisplayAlert("Se déconnecter ?", "Votre programme restera enregistré sur cet appareil.", "Se déconnecter", "Annuler");
        if (!ok) return;
        _auth.Logout();
        await Go("//accueil");
    }

    [RelayCommand]
    private async Task DeleteAccount()
    {
        var ok = await Shell.Current.DisplayAlert(
            "Supprimer mon compte ?",
            "Cette action est définitive : votre compte, vos préférences et votre programme seront effacés de cet appareil.",
            "Supprimer", "Annuler");
        if (!ok) return;
        Store.ResetPersonalData();
        await Go("//accueil");
    }
}
