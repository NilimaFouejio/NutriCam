using NutriCam.ViewModels;

namespace NutriCam.Views;

public partial class RecapPage : ContentPage
{
    public RecapPage(RecapViewModel viewModel)
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
