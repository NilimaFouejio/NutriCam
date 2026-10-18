using NutriCam.ViewModels;

namespace NutriCam.Views;

public partial class OrientationPage : ContentPage
{
    public OrientationPage(OrientationViewModel viewModel)
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
