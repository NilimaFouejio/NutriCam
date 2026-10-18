using NutriCam.ViewModels;

namespace NutriCam.Views;

public partial class ConnexionPage : ContentPage
{
    public ConnexionPage(ConnexionViewModel viewModel)
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
