using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using NutriCam.Helpers;
using NutriCam.Models;
using NutriCam.Services;

namespace NutriCam.ViewModels;

/// <summary>Étape 2/4 : choix de l'objectif (un seul).</summary>
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
        Select(Options.First(o => o.Key == store.Data.Goal.ToString()));
    }

    public ObservableCollection<OptionItem> Options { get; }

    private void Select(OptionItem chosen)
    {
        foreach (var option in Options)
            option.IsSelected = option == chosen;
    }

    [RelayCommand]
    private async Task Continue()
    {
        var chosen = Options.First(o => o.IsSelected);
        Store.Data.Goal = Enum.Parse<Goal>(chosen.Key);
        Store.Save();
        await Go("preferences");
    }
}
