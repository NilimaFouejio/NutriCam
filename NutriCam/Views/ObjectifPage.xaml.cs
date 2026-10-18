using NutriCam.ViewModels;

namespace NutriCam.Views;

public partial class ObjectifPage : ContentPage
{
    public ObjectifPage(ObjectifViewModel viewModel)
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
