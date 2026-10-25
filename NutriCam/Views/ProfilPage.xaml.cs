using NutriCam.Helpers;
using NutriCam.ViewModels;

namespace NutriCam.Views;

public partial class ProfilPage : ContentPage
{
    public ProfilPage(ProfilViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is BaseViewModel vm)
        {
            vm.ScrollToRequested += OnScrollRequested;
            vm.OnAppearing();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is BaseViewModel vm) vm.ScrollToRequested -= OnScrollRequested;
    }

    /// <summary>Fait défiler jusqu'au premier élément à renseigner.</summary>
    private void OnScrollRequested(string key)
    {
        VisualElement? target = key switch
        {
            "age" => AgeField,
            "height" => HeightField,
            "weight" => WeightField,
            "sex" => SexBlock,
            "activity" => ActivityBlock,
            "health" => HealthBlock,
            _ => null
        };
        _ = ScrollHelper.ScrollToAsync(Scroller, target);
    }
}
