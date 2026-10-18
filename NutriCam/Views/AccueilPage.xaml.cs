using NutriCam.ViewModels;

namespace NutriCam.Views;

public partial class AccueilPage : ContentPage
{
    public AccueilPage(AccueilViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as BaseViewModel)?.OnAppearing();
    }
}
