using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriCam.Services;

namespace NutriCam.ViewModels;

/// <summary>Bilan de la semaine : repas mangés sur le total, jour par jour, puis nouvelle semaine.</summary>
public partial class BilanViewModel : BaseViewModel
{
    public BilanViewModel(ProgramStore store) : base(store) { }

    public ObservableCollection<DayStat> Days { get; } = new();

    [ObservableProperty] private string ratioText = "";
    [ObservableProperty] private string headline = "";
    [ObservableProperty] private string message = "";
    [ObservableProperty] private double ratio;

    public override void OnAppearing()
    {
        var total = Store.MealCount;
        var done = Store.DoneCount;
        Ratio = total == 0 ? 0 : (double)done / total;
        RatioText = $"{done} repas sur {total}";
        Headline = $"{(int)Math.Round(Ratio * 100)} % de votre programme suivi";
        Message = Ratio >= 0.8
            ? "Belle régularité ! Gardez ce rythme pour la semaine prochaine."
            : Ratio >= 0.5
                ? "Un bon début. Quelques ajustements de goûts ou de budget peuvent vous aider à tenir le rythme."
                : "Chaque repas compte. Vous pouvez ajuster vos préférences pour que le programme vous ressemble davantage.";

        Days.Clear();
        for (var i = 0; i < 7; i++)
        {
            var meals = Store.MealsOfDay(i).ToList();
            var eaten = meals.Count(m => m.Done);
            Days.Add(new DayStat(Store.DayName(i), $"{eaten} / {meals.Count}", meals.Count == 0 ? 0 : (double)eaten / meals.Count));
        }
    }

    [RelayCommand]
    private async Task NewWeek()
    {
        IsBusy = true;
        await Task.Delay(1200);
        Store.GenerateProgram();
        IsBusy = false;
        await Go("//dashboard");
    }

    [RelayCommand]
    private async Task AdjustPreferences()
    {
        Store.EditMode = true;
        await Go("preferences");
    }
}
