using NutriCam.ViewModels;

namespace NutriCam.Views;

public partial class ProgrammePage : ContentPage
{
    public ProgrammePage(ProgrammeViewModel viewModel)
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
