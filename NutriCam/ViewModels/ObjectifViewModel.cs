using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriCam.Helpers;
using NutriCam.Models;
using NutriCam.Services;

namespace NutriCam.ViewModels;

/// <summary>Étape 2/5 : choix de l'objectif (un seul). Rien n'est coché d'avance : c'est à la personne de choisir.</summary>
public partial class ObjectifViewModel : BaseViewModel
{
    public ObjectifViewModel(ProgramStore store) : base(store)
    {
        Options = new ObservableCollection<OptionItem>
        {
            new(nameof(Goal.Perdre), "Perdre du poids", "Doucement, sans frustration", Icons.TrendingDown, Select),
            new(nameof(Goal.Maintenir), "Maintenir mon poids", "Garder mon équilibre", Icons.Scale, Select),
            new(nameof(Goal.Prendre), "Prendre du poids", "Manger plus, et bien", Icons.TrendingUp, Select),
            new(nameof(Goal.Equilibrer), "Mieux équilibrer", "Varier mes repas au quotidien", Icons.Utensils, Select),
        };
        // On ne coche que le choix déjà validé par la personne (quand elle revient modifier cette étape).
        if (store.Data.Goal is { } saved)
            Select(Options.First(o => o.Key == saved.ToString()));
    }

    [ObservableProperty] private string error = "";

    public ObservableCollection<OptionItem> Options { get; }

    private void Select(OptionItem chosen)
    {
        foreach (var option in Options)
            option.IsSelected = option == chosen;
    }

    [RelayCommand]
    private async Task Continue()
    {
        var chosen = Options.FirstOrDefault(o => o.IsSelected);
        if (chosen is null)
        {
            Error = "Choisissez un objectif pour continuer.";
            ScrollTo("goal");
            return;
        }
        Error = "";
        Store.Data.Goal = Enum.Parse<Goal>(chosen.Key);
        Store.Save();
        if (await ReturnToRecapIfRequested()) return;
        await Go("preferences");
    }
}
