using CommunityToolkit.Mvvm.ComponentModel;
using NutriCam.Services;

namespace NutriCam.ViewModels;

public abstract class BaseViewModel : ObservableObject
{
    private bool _isBusy;

    protected BaseViewModel(ProgramStore store) => Store = store;

    protected ProgramStore Store { get; }

    /// <summary>Vrai pendant une opération (connexion, génération…) : désactive les boutons.</summary>
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(IsNotBusy));
                OnBusyChanged();
            }
        }
    }

    public bool IsNotBusy => !IsBusy;

    protected virtual void OnBusyChanged() { }

    /// <summary>Appelé à chaque affichage de la page (rafraîchit les données partagées).</summary>
    public virtual void OnAppearing() { }

    protected static Task Go(string route) => Shell.Current.GoToAsync(route);

    protected static Task Alert(string title, string message) => Shell.Current.DisplayAlert(title, message, "OK");
}
