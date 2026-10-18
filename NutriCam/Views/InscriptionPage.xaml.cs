using NutriCam.ViewModels;

namespace NutriCam.Views;

public partial class InscriptionPage : ContentPage
{
    public InscriptionPage(InscriptionViewModel viewModel)
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
