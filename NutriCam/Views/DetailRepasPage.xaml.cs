using NutriCam.ViewModels;

namespace NutriCam.Views;

public partial class DetailRepasPage : ContentPage
{
    public DetailRepasPage(DetailRepasViewModel viewModel)
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
