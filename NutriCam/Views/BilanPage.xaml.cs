using NutriCam.ViewModels;

namespace NutriCam.Views;

public partial class BilanPage : ContentPage
{
    public BilanPage(BilanViewModel viewModel)
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
