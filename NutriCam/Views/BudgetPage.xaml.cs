using NutriCam.Helpers;
using NutriCam.ViewModels;

namespace NutriCam.Views;

public partial class BudgetPage : ContentPage
{
    public BudgetPage(BudgetViewModel viewModel)
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
            "budget" => BudgetBlock,
            "available" => AvailableBlock,
            _ => null
        };
        _ = ScrollHelper.ScrollToAsync(Scroller, target);
    }
}
