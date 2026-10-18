using CommunityToolkit.Mvvm.ComponentModel;
using NutriCam.Helpers;
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

    /// <summary>
    /// Demande à la page de faire défiler jusqu'à l'élément à renseigner. La clé est propre à chaque page
    /// (par exemple « age », « sex » ou « health » pour le profil).
    /// </summary>
    public event Action<string>? ScrollToRequested;

    protected void ScrollTo(string key) => ScrollToRequested?.Invoke(key);

    /// <summary>Appelé à chaque affichage de la page (rafraîchit les données partagées).</summary>
    public virtual void OnAppearing() { }

    /// <summary>Quand une étape est rouverte depuis le récapitulatif, revient directement à celui-ci.</summary>
    protected async Task<bool> ReturnToRecapIfRequested()
    {
        if (!Store.ReturnToRecap) return false;
        Store.ReturnToRecap = false;
        await Go("..");
        return true;
    }

    /// <summary>Navigation sécurisée (voir <see cref="Nav"/>) : à utiliser à la place de Shell.Current.GoToAsync.</summary>
    protected static Task Go(string route) => Nav.GoAsync(route);

    protected static Task Alert(string title, string message) => Shell.Current.DisplayAlert(title, message, "OK");
}
