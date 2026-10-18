using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriCam.Services;

namespace NutriCam.ViewModels;

/// <summary>
/// Écran affiché quand la personne déclare une maladie ou une situation à risque (diabète, hypertension…) :
/// aucun programme n'est généré, elle est invitée à consulter un médecin nutritionniste.
/// </summary>
public partial class OrientationViewModel : BaseViewModel
{
    public OrientationViewModel(ProgramStore store) : base(store) { }

    [ObservableProperty] private string declared = "";

    public override void OnAppearing()
    {
        var conditions = Store.Data.Profile.Conditions;
        Declared = conditions.Count == 0
            ? ""
            : "Vous nous avez indiqué : " + string.Join(", ", conditions.Select(c => c.ToLowerInvariant())) + ".";
    }

    /// <summary>Corriger une réponse donnée par erreur.</summary>
    [RelayCommand]
    private Task ChangeAnswers() => Go("..");

    [RelayCommand]
    private Task Understood() => Go("//accueil");
}
